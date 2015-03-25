using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using Lidgren.Network;
using TomShane.Neoforce.Controls;
using Microsoft.Xna.Framework.Media;
using System.IO;

public class GameplayScreen : GameScreen
{
    GraphicsDevice GraphicsDevice;
    ParticleSystem psUp; //Voor particles die boven shadow liggen
    ParticleSystem psDown; //Voor particles die onder shadow liggen
    static Player player;
    Robot1 robot1;
    int timer = 0;
    double evilPoints = 30;
    SoundEffect beep;
    Tile[,] textureGrid;
    Texture2D tile;
    Texture2D blockTexture, longBlockTexture;

    public PointLight playerFOV;
    Visual newBlock;
    //InputManager inputManager;
    SpriteFont font;
    Vector2 oldCameraPosition;

    TextBox[] abilityExpl;
    ProgressBar[] abilityProgress;
    ProgressBar healhBar;
    Button[] abilityButton;
    Button[] soundButton;
    Button[] upgradeButton;
    Boolean[] isUpgraded;

    static List<Visual> blocks;
    List<PointLight> lights;

    RenderTarget2D colorMap;
    RenderTarget2D lightMap;
    RenderTarget2D blurMap;

    Effect lightEffect;
    Effect combineEffect;
    Effect blurEffect;

    Quad quad;

    Boolean usingAbility = false;
    int currentAbility = 9999;

    bool playingSoundEffect = false;
    float soundEffectTimer;

    EnergyCell[] energyCells;
    List<Trap> traps;
    List<Projectile> projectiles;
    public static int cellCount = 0;

    Vector2 paintStartPos = Vector2.Zero;
    Vector2 paintEndPos = Vector2.Zero;
    
    public override void LoadContent(ContentManager Content, InputManager inputManager)
    {
        base.LoadContent(Content, inputManager);
        GraphicsDevice = Options.GetValue<GraphicsDevice>("GraphicsDevice");
        font = content.Load<SpriteFont>("GUI/Fonts/debug");
        blockTexture = Content.Load<Texture2D>("Block");
        longBlockTexture = Content.Load<Texture2D>("LongBlock");
        
        beep = Content.Load<SoundEffect>("beep");
        lightEffect = Content.Load<Effect>("Light");

        int windowWidth = Options.GetValue<int>("screenWidth");
        int windowHeight = Options.GetValue<int>("screenHeight");

        colorMap = new RenderTarget2D(GraphicsDevice, windowWidth, windowHeight, false, SurfaceFormat.Color, DepthFormat.Depth16, 16, RenderTargetUsage.DiscardContents);
        lightMap = new RenderTarget2D(GraphicsDevice, windowWidth, windowHeight, false, SurfaceFormat.Color, DepthFormat.Depth16, 16, RenderTargetUsage.DiscardContents);
        blurMap = new RenderTarget2D(GraphicsDevice, windowWidth, windowHeight, false, SurfaceFormat.Color, DepthFormat.None, 16, RenderTargetUsage.DiscardContents);

        combineEffect = Content.Load<Effect>("Combine");
        lightEffect = Content.Load<Effect>("Light");
        blurEffect = Content.Load<Effect>("Blur");
        
        quad = new Quad();

        blocks = new List<Visual>();
        lights = new List<PointLight>();
        lights.Add(new PointLight(lightEffect, new Vector2(300, 300), 500, Color.White, 1.0f));

        player = new Player();
        player.LoadContent(content, inputManager);

        robot1 = new Robot1();
        robot1.LoadContent(content, inputManager);
       
        lights.Clear();

        if ((Options.GetValue<State>("role") == State.None))
            Options.SetValue("role", State.System);

        if (Options.GetValue<State>("role") == State.System)
        {
            playerFOV = new PointLight(lightEffect, Vector2.Zero, 1500, Color.White, 1.0f);
            lights.Add(playerFOV);
            
        }
        else if (Options.GetValue<State>("role") == State.Robot)
        {
            playerFOV = new PointLight(lightEffect, Vector2.Zero, 100, Color.White, 1.0f);
            lights.Add(playerFOV);
        }
        else
        {
            
        }
        Options.SetValue("lightEngine", true);
        abilityProgress = new ProgressBar[5];
        abilityButton = new Button[5];
        upgradeButton = new Button[5];
        isUpgraded = new bool[5] { false, false, false, false, false };
        abilityExpl = new TextBox[5];
        soundButton = new Button[5];
        CreateHud();
        LoadMap();
       
        psUp = new ParticleSystem();
        psDown = new ParticleSystem();

        energyCells = new EnergyCell[6]{
            new EnergyCell(new Vector2(300,300)), new EnergyCell(new Vector2(400,400)), new EnergyCell(new Vector2(500,500)),
            new EnergyCell(new Vector2(600,500)), new EnergyCell(new Vector2(500,600)), new EnergyCell(new Vector2(400,700))
        };
        traps = new List<Trap>();
        projectiles = new List<Projectile>();
    }

    public override void UnloadContent()
    {
        base.UnloadContent();
        player.UnloadContent();
        robot1.UnloadContent();
    }

    public override void NetworkMessage(NetIncomingMessage message)
    {
        message.Position = 0;
        PacketTypes type = (PacketTypes)message.ReadByte();
        if (type == PacketTypes.SOUNDEFFECT) PlaySound((int)message.ReadByte());
        else if (type == PacketTypes.ABILITIES)
        {
            State state = (State)message.ReadByte();
            if (state == State.System)
            {
                int abil = (int)message.ReadByte();
                if (abil == 0)
                {
                    string[] data = message.ReadString().Split(' ');
                    lights.Add(new PointLight(lightEffect, new Vector2(int.Parse(data[0]) / ScreenManager.Instance.DrawScale().M11, int.Parse(data[1]) / ScreenManager.Instance.DrawScale().M22), 110, Color.Red, 1.0f));
                }
            }
        }
        else if (type == PacketTypes.MOVE)
        {
            if(Options.GetValue<State>("role") != State.Robot)
            {
                string[] data = message.ReadString().Split(' ');
                player.PlayerPosition = new Vector2(float.Parse(data[0]), float.Parse(data[1]));
                player.FacingAngle = float.Parse(data[2]);
                player.Velocity = new Vector2(float.Parse(data[3]), float.Parse(data[4]));
            }
        }
    }

    public override void Update(GameTime gameTime)
    {
        inputManager.Update();
        if (Options.GetValue<State>("role") == State.System) 
            playerFOV.Position = player.PlayerPosition + Camera.CameraPosition;

        if (oldCameraPosition != Camera.CameraPosition)
        {
            foreach (Visual v in blocks)
                v.Pose.Position = v.Pose.Position - (oldCameraPosition - Camera.CameraPosition);
            oldCameraPosition = Camera.CameraPosition;
        }


        /*if (Options.GetValue<State>("role") == State.System ||
            Options.GetValue<State>("role") == State.Robot) //TODO:: uiteindelijk alleen robot??? */
        if (Options.GetValue<State>("role") == State.Robot)
        {
            player.Update(gameTime, inputManager);

            NetOutgoingMessage msg = NetworkManager.Instance.CreateMessage();
            msg.Write((byte)PacketTypes.MOVE);
            msg.Write((string)(player.PlayerPosition.X + " " + player.PlayerPosition.Y + " " + player.FacingAngle + " " + player.Velocity.X + " " + player.Velocity.Y));
            NetworkManager.Instance.SendMessage(msg);
        }

       //  else
       // {
        if (inputManager.KeyDown(Keys.Down, Keys.H))
        {
            Camera.CameraPosition = new Vector2(Camera.CameraPosition.X, Camera.CameraPosition.Y - 10);
            
        }
        if (inputManager.KeyDown(Keys.Up, Keys.Y))
        {
            Camera.CameraPosition = new Vector2(Camera.CameraPosition.X, Camera.CameraPosition.Y + 10);
        }
        if (inputManager.KeyDown(Keys.Left, Keys.G))
        {
            Camera.CameraPosition = new Vector2(Camera.CameraPosition.X + 10, Camera.CameraPosition.Y);
        }
        if (inputManager.KeyDown(Keys.Right, Keys.J))
        {
            Camera.CameraPosition = new Vector2(Camera.CameraPosition.X - 10, Camera.CameraPosition.Y);
        }
      //  }
        timer += gameTime.ElapsedGameTime.Milliseconds;
        evilPoints += 0.010;
        
        for (int i = 0; i < 5; i++) 
        {
            if (abilityProgress[i].Value < abilityProgress[i].Range)
            {
                abilityProgress[i].Value += gameTime.ElapsedGameTime.Milliseconds;
                abilityButton[i].Color = Color.Gray;
            }
            else
            {
                abilityButton[i].Color = Color.Red;
                
            }
            
        }

        for (int i = 0; i < 5; i++)
        {
                abilityButton[i].MouseUp += b_clicked;
                abilityButton[i].MouseOver += b_over;
                abilityButton[i].MouseOut += b_out;
                upgradeButton[i].MouseUp += u_clicked;
                if (Options.GetValue<State>("role") == State.System)
                    soundButton[i].MousePress += s_clicked;
        }

        psUp.update(gameTime);
        psDown.update(gameTime);

        if (usingAbility)
        {
            if (Options.GetValue<State>("role") == State.System)
                switch (currentAbility)
                {
                    case 0: SysAbZero(); break;
                    case 1: SysAbOne(); break;
                    case 2: SysAbTwo(); break;
                    case 3: SysAbThree(); break;
                    case 4: SysAbFour(); break;
                }
            if (Options.GetValue<State>("role") == State.Robot)
                switch (currentAbility)
                {
                    case 0: RobAbZero(); break;
                    case 1: RobAbOne(); break;
                    case 2: RobAbTwo(); break;
                    case 3: RobAbThree(); break;
                    case 4: RobAbFour(); break;
                }
            if (Options.GetValue<State>("role") == State.Doctor)
                switch (currentAbility)
                {
                    case 0: DocAbZero(); break;
                    case 1: DocAbOne(); break;
                    case 2: DocAbTwo(); break;
                    case 3: DocAbThree(); break;
                    case 4: DocAbFour(); break;
                }


        }
        foreach (PointLight l in lights)
        {
            if (l.Radius == 110)
            {
                l.Power -= 0.0025f;
                if (l.Power < 0.10f)
                {
                    lights.Remove(l);
                    break;
                }

                if (isUpgraded[0])
                {
                    lights[1].Position = new Vector2(Mouse.GetState().X / ScreenManager.Instance.DrawScale().M11, Mouse.GetState().Y / ScreenManager.Instance.DrawScale().M22);
                }
            }
        }

        if (playingSoundEffect)
        {
            if (soundEffectTimer > 0)
            {
                soundEffectTimer -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                MediaPlayer.Volume = Math.Min(1.0f, Options.GetValue<float>("musicVolume")) * 0.1f;
            }
            else
            {
                soundEffectTimer = 0;
                MediaPlayer.Volume = Math.Min(1.0f, Options.GetValue<float>("musicVolume"));
                playingSoundEffect = false;
            }
        }

        foreach (EnergyCell ev in energyCells)
        {
            ev.Update(gameTime, inputManager);
            ev.CheckCollision(new Rectangle((int)player.PlayerPosition.X, (int)player.PlayerPosition.Y, 32, 32));
        }
        foreach (Trap t in traps)
        {
            t.Update(gameTime, inputManager);
            if (t.CheckCollision(new Rectangle((int)player.PlayerPosition.X, (int)player.PlayerPosition.Y, 32, 32)))
            {
                psDown.CreateExplosion(40, new Vector2(player.PlayerPosition.X, player.PlayerPosition.Y), Color.Orange, true, 0.15f, 200f, 0.50f, 10f);
                psDown.CreateExplosion(30, new Vector2(player.PlayerPosition.X, player.PlayerPosition.Y), Color.Red, true, 0.15f, 300f, 0.50f, 10f);
                psDown.CreateExplosion(90, new Vector2(player.PlayerPosition.X, player.PlayerPosition.Y), Color.Gray, true, 0.05f, 500f, 0.60f, 1f);
                healhBar.Value -= 10;
            }
            
        }

        if (cellCount >= 3 && Options.GetValue<State>("role") == State.System)
            //TODO:: upgrade systemen (parts etcetera) checks hier bij deze if
        {
            for (int i = 0; i < 5; i++)
            {
                if (upgradeButton[i].Text == "Upgrade")
                {
                    upgradeButton[i].Color = Color.White;
                    upgradeButton[i].Enabled = true;
                }
            }
        }
        else
        {
            for (int i = 0; i < 5; i++)
            {
                upgradeButton[i].Color = Color.Gray;
                upgradeButton[i].Enabled = false;
            }
        }
    }

    public override void PreDraw(GraphicsDevice GraphicsDevice, SpriteBatch spriteBatch)
    {

        DrawColorMap(GraphicsDevice, spriteBatch);  // Draw the colors

        if (Options.GetValue<State>("role") != State.Doctor)
        {

            DrawLightMap(GraphicsDevice, spriteBatch, 0.0f); // Draw the lights
            BlurRenderTarget(GraphicsDevice, lightMap, 2.5f);// Blurr the shadows
            CombineAndDraw(GraphicsDevice); // Combine
        }
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        base.Draw(spriteBatch);
        player.Draw(spriteBatch);
        robot1.Draw(spriteBatch);
        if (Options.GetValue<State>("role") == State.Doctor)
            spriteBatch.DrawString(font, "EvilPoints: " + (int)evilPoints, new Vector2(10, 30), Color.Red);
        if (Options.GetValue<State>("role") != State.Doctor)
            spriteBatch.DrawString(font, "Energy Cells: " + (int)cellCount, new Vector2(10, 30), Color.Blue); //TODO:: mooier font?

        psUp.draw(spriteBatch);
       

    }

    public void LoadMap()
    {
        tile = content.Load<Texture2D>("Sprites/tiles");
        textureGrid = new Tile[90, 90];
        StreamReader Reader = new StreamReader("Content/Level1.txt");
        List<string> YLines = new List<string>();
        string XLines = Reader.ReadLine();
        int count = 0;

        while (XLines != null)
        {
            YLines.Add(XLines);
            XLines = Reader.ReadLine();
        }

        foreach (string line in YLines)
        {
            string[] lineArray = line.Split(' ');
            for(int i = 0; i < lineArray.Length;i++){
                switch(lineArray[i]){
                    case "H": textureGrid[i, count] = new Tile(tile, 1);
                             Pose2D newPose = new Pose2D(new Vector2(16 + (32 * i), 16 + (32 * count)), 0f, 1f);
                             newBlock = new Visual(blockTexture, newPose);
                             blocks.Add(newBlock); break;
                    case "O": textureGrid[i, count] = new Tile(tile, 0); break;
                    case "1": textureGrid[i, count] = new Tile(tile, 1); break;
                    case "2": textureGrid[i, count] = new Tile(tile, 2); break;
                    case "3": textureGrid[i, count] = new Tile(tile, 3); break;
                    case "4": textureGrid[i, count] = new Tile(tile, 4); break;
                    case "5": textureGrid[i, count] = new Tile(tile, 5); break;
                    case "6": textureGrid[i, count] = new Tile(tile, 6); break;
                    case "7": textureGrid[i, count] = new Tile(tile, 7); break;
                    case "-": textureGrid[i, count] = new Tile(tile, 12); break;
                    case "|": textureGrid[i, count] = new Tile(tile, 21); break;
                    case "X": textureGrid[i, count] = new Tile(tile, 20); break;
                    case "Z": textureGrid[i, count] = new Tile(tile, 19); break;
                    case "A": textureGrid[i, count] = new Tile(tile, 11); break;
                    case "S": textureGrid[i, count] = new Tile(tile, 13); break;

                }
            }
           count++;
        }

    }

    public void PlaySound(int soundID)
    {
        playingSoundEffect = true;
        soundEffectTimer = (float)beep.Duration.TotalMilliseconds;

        switch (soundID)
        {
            case 0: beep.Play(Math.Min(1.0f, Options.GetValue<float>("soundVolume")), -1.0f, 0.0f); break;
            case 1: beep.Play(Math.Min(1.0f, Options.GetValue<float>("soundVolume")), 0.0f, 0.0f); break;
            case 2: beep.Play(Math.Min(1.0f, Options.GetValue<float>("soundVolume")), 1.0f, 0.0f); break;
            case 3: beep.Play(Math.Min(1.0f, Options.GetValue<float>("soundVolume")), 0.0f, -1.0f); break;
            case 4: beep.Play(Math.Min(1.0f, Options.GetValue<float>("soundVolume")), 0.0f, 1.0f); break;
                
        }

    }
    #region events
    void b_clicked(object sender, TomShane.Neoforce.Controls.EventArgs e)
    {
        Color[] colorRange;
        colorRange = new Color[5]{Color.Blue, Color.Red, Color.Yellow, Color.Purple, Color.Green};
        Button b = (Button)sender;
        for(int i = 0; i < 5; i++)
            if (b.Name == abilityProgress[i].Name + i) //zoek id die overeenkomt met aangeklikte button
            {
                if (abilityProgress[i].Value >= abilityProgress[i].Range) //als ability gebruikt mag worden
                {
                    abilityProgress[i].Value = 0; //button cooldown start
                    b.Color = Color.White; //button weer white
                    usingAbility = true;
                    currentAbility = i;
                }
                
            }
        e.Handled = true;
    }

    void b_over(object sender, TomShane.Neoforce.Controls.EventArgs e)
    {
        Button b = (Button)sender;
        for (int i = 0; i < 5; i++)
            if (b.Name == abilityExpl[i].Name + i)
            {
                abilityExpl[i].Show();
                break;
            }
        e.Handled = true;
    }

    void b_out(object sender, TomShane.Neoforce.Controls.EventArgs e)
    {
        Button b = (Button)sender;
        for (int i = 0; i < 5; i++)
            if (b.Name == abilityExpl[i].Name + i)
            {
                abilityExpl[i].Hide();
                break;
            }
        e.Handled = true;
    }

    void s_clicked(object sender, TomShane.Neoforce.Controls.EventArgs e)
    {
        
        Button b = (Button)sender;
        if (!b.Pushed)
        {
            //PlaySound(int.Parse(b.Name));

            NetOutgoingMessage msg = NetworkManager.Instance.CreateMessage();
            msg.Write((byte)PacketTypes.SOUNDEFFECT);
            msg.Write((byte)int.Parse(b.Name));
            NetworkManager.Instance.SendMessage(msg);
            //TODO:: stuur command naar robot zodat die geluid te horen krijgt.
            b.Pushed = true;
        }
        e.Handled = true;
    }

    void u_clicked(object sender, TomShane.Neoforce.Controls.EventArgs e)
    {
        if (cellCount >= 3)
        {
            Button b = (Button)sender;
            for (int i = 0; i < 5; i++)
                if (b.Name == "u" + i && isUpgraded[i] == false) //zoek id die overeenkomt met aangeklikte upgade button
                {
                    //TODO:: check/reset etcetera
                    isUpgraded[i] = true;
                    cellCount -= 3;
                    upgradeButton[i].Color = Color.Gray;
                    upgradeButton[i].Enabled = false;
                    upgradeButton[i].Text = "Bought";
                    break;
                }
        }
        e.Handled = false;
    }
    #endregion

    #region Abilities

    #region system abilities
    public void SysAbZero() //creating light
    {
        if (inputManager.MouseLeftButtonReleased())
        {
            lights.Add(new PointLight(lightEffect,new Vector2(Mouse.GetState().X / ScreenManager.Instance.DrawScale().M11, Mouse.GetState().Y / ScreenManager.Instance.DrawScale().M22), 110, Color.Red, 1.0f));
            usingAbility = false;

            NetOutgoingMessage msg = NetworkManager.Instance.CreateMessage();
            msg.Write((byte)PacketTypes.ABILITIES);
            msg.Write((byte)Options.GetValue<State>("role"));
            msg.Write((byte)0);
            msg.Write((string)(inputManager.MousePosClean().X + " " + inputManager.MousePosClean().Y));
            NetworkManager.Instance.SendMessage(msg);
           
        }
        //upgrade, light follows mouse
    }

    public void SysAbOne() //creating light at robot
    {
        //TODO:: stuur command zodat er licht komt bij de speler die robot is.
        //upgrade, meer zicht
    }
    public void SysAbTwo() 
    {
        //TODO:: remove trap.
        //upgrade, remove ook enemies
    }
    public void SysAbThree() 
    {
        
        //upgrade, meer paint
        if (inputManager.MouseLeftButtonDown() && paintStartPos == Vector2.Zero)
        {
            paintStartPos = new Vector2(Mouse.GetState().X / ScreenManager.Instance.DrawScale().M11, Mouse.GetState().Y / ScreenManager.Instance.DrawScale().M22);
        }
        if(inputManager.MouseLeftButtonReleased())
        {
            paintEndPos = new Vector2(Mouse.GetState().X / ScreenManager.Instance.DrawScale().M11, Mouse.GetState().Y / ScreenManager.Instance.DrawScale().M22);
        }
        if (paintStartPos != Vector2.Zero && paintEndPos != Vector2.Zero)
        {
            psUp.CreateTrail(100, paintStartPos, paintEndPos, Color.Red, true,0.05f);
            //TODO stuur hier info naar robot if possible ;]]]]]
            paintStartPos = Vector2.Zero;
            paintEndPos = Vector2.Zero;
        }

    }
    public void SysAbFour() 
    {
        //TODO:: think of something
    }
    #endregion
        #region doctor abilities
    private void DocAbFour()
    {
        throw new NotImplementedException();
    }

    private void DocAbThree()
    {
        throw new NotImplementedException();
    }

    private void DocAbTwo()
    {
        throw new NotImplementedException();
    }

    private void DocAbOne()
    {
        if (inputManager.MouseLeftButtonReleased())
        {
            Trap tmp = new Trap(new Vector2((int)(((Mouse.GetState().X / ScreenManager.Instance.DrawScale().M11) - Camera.CameraPosition.X) / 16) * 16, (int)(((Mouse.GetState().Y / ScreenManager.Instance.DrawScale().M22) - Camera.CameraPosition.Y) / 16) * 16));
            traps.Add(tmp);
            usingAbility = false;
        }
    }

    private void DocAbZero()
    {
        throw new NotImplementedException();
    }
        #endregion
        #region robot abilities
    private void RobAbFour()
    {
        throw new NotImplementedException();
    }

    private void RobAbThree()
    {
        throw new NotImplementedException();
    }

    private void RobAbTwo()
    {
        throw new NotImplementedException();
    }

    private void RobAbOne()
    {
        throw new NotImplementedException();

    }

    private void RobAbZero()
    {
        if (inputManager.MouseLeftButtonReleased())
        {
            Projectile rocket = new Projectile("rocket", player.PlayerPosition, new Vector2(Mouse.GetState().X / ScreenManager.Instance.DrawScale().M11, Mouse.GetState().Y / ScreenManager.Instance.DrawScale().M22));
            projectiles.Add(rocket);
            usingAbility = false;
        }
    }
    #endregion

    #endregion

    public void CreateHud()
    {
        string[] abilityNames;
        string[] abilityDiscription;
        string[] abilityUpgradeName;
        int[] abilityCooldowns;
        abilityNames = new string[5];
        abilityCooldowns = new int[5];
        abilityDiscription = new string[5];
        abilityUpgradeName = new string[5];
        switch (Options.GetValue<State>("role"))
        {
            case State.Doctor: abilityNames = new string[5] { "Unknown", "Trap", "Monster", "Unknown", "Scramble" }; //namen van de abilities
                abilityCooldowns = new int[5] { 5000, 10000, 20000, 40000, 80000 }; //cooldown van de abilities
                abilityDiscription = new string[5] { "Unknown", "Create trap at mouse position", "Create monster at mouse position", "unknown", "Scramble the sounds of the system" };

                break;
            case State.Robot: abilityNames = new string[5] { "lol", "nee", "echt", "zo", "fucked" }; //namen van de abilities
                abilityCooldowns = new int[5] { 5000, 10000, 20000, 40000, 80000 }; //cooldown van de abilities
                abilityDiscription = new string[5] { "Dit is de eerste ability, het doet niks...", "oh waaait hoooo wat lalala", "ik heb te weinig geslapen", "dit is nummer 4 right", "kijk mij nou random shit bedenken." };
                break;
            case State.System: abilityNames = new string[5] { "Add light", "Vision Surge", "Destroy", "Paint", "hehe" }; //namen van de abilities
                abilityCooldowns = new int[5] { 5000, 10000, 20000, 4000, 80000 }; //cooldown van de abilities
                abilityDiscription = new string[5] { "Create a small temporary light at the position of your mouse.", "Restore minimal vision for the Robot", "Destroy stuff from Doctor", "Paint at mouse for Robot", "kijk mij nou random shit bedenken." };
                abilityUpgradeName = new string[5] { "Upgrade", "Upgrade", "Upgrade", "Upgrade", "Upgrade" };
                #region Soundbar
                int[] soundButtonPositions;
                soundButtonPositions = new int[10] { 10, 90, 170, 10, 170, 10, 90, 10, 170, 170 };
                Window soundBar = new Window(Wamo.manager);
                soundBar.Init();
                soundBar.SetPosition(Options.GetValue<int>("screenWidth")  - 260, Options.GetValue<int>("screenHeight") - 260);
                soundBar.SetSize(240, 240);
                soundBar.Suspended = true; //geen events
                soundBar.Visible = true;
                soundBar.Resizable = false;
                soundBar.Passive = true; //geen user input
                soundBar.BorderVisible = false;

                for (int i = 0; i < 5; i++)
                {
                    soundButton[i] = new Button(Wamo.manager);
                    soundButton[i].Init();
                    soundButton[i].Name = i.ToString();
                    soundButton[i].SetPosition(soundButtonPositions[i], soundButtonPositions[i + 5]);
                    soundButton[i].SetSize(60, 60);
                    soundButton[i].Text = "sound";
                    soundButton[i].Parent = soundBar;
                    soundButton[i].Anchor = Anchors.None;
                }
                Wamo.manager.Add(soundBar);
#endregion
                    break;
        }

        healhBar = new ProgressBar(Wamo.manager);
        healhBar.Init();
        healhBar.Color = Color.Blue;
        healhBar.SetPosition(30, 30);
        healhBar.SetSize(200,20);
        healhBar.Value = 100;
        healhBar.Range = 100; //dit is de health van de speler;
        healhBar.Text = "50/100";
        healhBar.TextColor = Color.Black;
        
        Wamo.manager.Add(healhBar);

        #region abilityBar
        Window abilityBar = new Window(Wamo.manager);
        abilityBar.Init();
        abilityBar.SetPosition(Options.GetValue<int>("screenWidth")/2 - 180, Options.GetValue<int>("screenHeight") - 160);
        abilityBar.SetSize(380, 110);
        abilityBar.Suspended = true; //geen events
        abilityBar.Visible = true;
        abilityBar.Resizable = false;
        abilityBar.Passive = true; //geen user input
        abilityBar.BorderVisible = false;

        for (int i = 0; i < 5; i++)
        {
            abilityButton[i] = new Button(Wamo.manager);
            abilityButton[i].Init();
            abilityButton[i].Name = "a" + i;
            abilityButton[i].SetPosition(10 + (70 * i), 10);
            abilityButton[i].SetSize(60, 60);
            abilityButton[i].Text = abilityNames[i];
            abilityButton[i].Parent = abilityBar;
            abilityButton[i].Anchor = Anchors.None;
        }

        for (int i = 0; i < 5; i++)
        {
            abilityProgress[i] = new ProgressBar(Wamo.manager);
            abilityProgress[i].Init();
            abilityProgress[i].SetPosition(10 + (70 * i), 72);
            abilityProgress[i].SetSize(60, 6);
            abilityProgress[i].Range = abilityCooldowns[i];
            abilityProgress[i].Name = "a";
            abilityProgress[i].Value = 0;
            abilityProgress[i].Parent = abilityBar;
            abilityProgress[i].Anchor = Anchors.None;
        }
        
        for (int i = 0; i < 5; i++)
        {
            upgradeButton[i] = new Button(Wamo.manager);
            upgradeButton[i].Init();
            upgradeButton[i].Name = "u" + i;
            upgradeButton[i].SetPosition(10 + (70 * i), 83);
            upgradeButton[i].SetSize(60, 20);
            upgradeButton[i].Text = abilityUpgradeName[i];
            upgradeButton[i].Color = Color.Gray;
            upgradeButton[i].Enabled = false;
            upgradeButton[i].Parent = abilityBar;
            upgradeButton[i].Anchor = Anchors.None;

        }
        
            for (int i = 0; i < 5; i++)
            {
                abilityExpl[i] = new TextBox(Wamo.manager);
                abilityExpl[i].Init();
                abilityExpl[i].SetPosition(Options.GetValue<int>("screenWidth") / 2 - 180, Options.GetValue<int>("screenHeight") - 205);
                abilityExpl[i].SetSize(360, 40);
                abilityExpl[i].Name = "a";
                abilityExpl[i].Resizable = false;
                abilityExpl[i].ReadOnly = true;
                abilityExpl[i].Text = abilityDiscription[i];
                abilityExpl[i].Hide();
                Wamo.manager.Add(abilityExpl[i]);

            }
            Wamo.manager.Add(abilityBar);
        #endregion abilityBar

    }

    private void CombineAndDraw(GraphicsDevice GraphicsDevice)
    {
        GraphicsDevice.SetRenderTarget(null);

        GraphicsDevice.Clear(Color.Black);

        GraphicsDevice.BlendState = BlendState.Opaque;
        // Samplers states are set by the shader itself            
        GraphicsDevice.DepthStencilState = DepthStencilState.Default;
        GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

        combineEffect.Parameters["colorMap"].SetValue(colorMap);
        combineEffect.Parameters["lightMap"].SetValue(lightMap);

        combineEffect.Techniques[0].Passes[0].Apply();
        quad.Render(GraphicsDevice, Vector2.One * -1.0f, Vector2.One);
    }

    /// <summary>
    /// Blurs the target render target
    /// </summary>        
    private void BlurRenderTarget(GraphicsDevice GraphicsDevice, RenderTarget2D target, float strength)
    {
        Vector2 renderTargetSize = new Vector2
            (
                target.Width,
                target.Height
            );

        blurEffect.Parameters["renderTargetSize"].SetValue(renderTargetSize);            
        blurEffect.Parameters["blur"].SetValue(strength);

        // Pass one
        GraphicsDevice.SetRenderTarget(blurMap);
        GraphicsDevice.Clear(Color.Black);

        blurEffect.Parameters["InputTexture"].SetValue(target);
           
        blurEffect.CurrentTechnique = blurEffect.Techniques["BlurHorizontally"];
        blurEffect.CurrentTechnique.Passes[0].Apply();
        quad.Render(GraphicsDevice, Vector2.One * -1, Vector2.One);

        // Pass two
        GraphicsDevice.SetRenderTarget(target);
        GraphicsDevice.Clear(Color.Black);
            
        blurEffect.Parameters["InputTexture"].SetValue(blurMap);

        blurEffect.CurrentTechnique = blurEffect.Techniques["BlurVertically"];
        blurEffect.CurrentTechnique.Passes[0].Apply();
        quad.Render(GraphicsDevice, Vector2.One * -1, Vector2.One);
    }

    /// <summary>
    /// Draws everything that emits light to a seperate render target
    /// </summary>
    private void DrawLightMap(GraphicsDevice GraphicsDevice, SpriteBatch spriteBatch, float ambientLightStrength)
    {
        GraphicsDevice.SetRenderTarget(lightMap);
        GraphicsDevice.Clear(Color.White * ambientLightStrength);

        // Draw normal object that emit light
        spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.Default, RasterizerState.CullCounterClockwise, null,ScreenManager.Instance.DrawScale());

        foreach(Visual v in blocks)
        {
            if(v.Glow != null)
            {
                Vector2 origin = new Vector2(v.Glow.Width / 2.0f, v.Glow.Height / 2.0f);
                spriteBatch.Draw(v.Glow, v.Pose.Position + Camera.CameraPosition, null, Color.White, v.Pose.Rotation, origin, v.Pose.Scale, SpriteEffects.None, 0.1f);
            }
        }

        spriteBatch.End();

        GraphicsDevice.BlendState = BlendState.Additive;
        // Samplers states are set by the shader itself            
        GraphicsDevice.DepthStencilState = DepthStencilState.None;
        GraphicsDevice.RasterizerState = RasterizerState.CullNone;

        if (Mouse.GetState().RightButton == ButtonState.Pressed)
        {
            //lights[0].Position = new Vector2(Mouse.GetState().X / ScreenManager.Instance.DrawScale().M11, Mouse.GetState().Y / ScreenManager.Instance.DrawScale().M22);
            psDown.CreateCannon(null, 10, 300, player.PlayerPosition + Camera.CameraPosition, new Vector2(Mouse.GetState().X / ScreenManager.Instance.DrawScale().M11, Mouse.GetState().Y / ScreenManager.Instance.DrawScale().M22),Color.Red,Color.Yellow);
           
        }
        List<Visual> inrange = new List<Visual>();

        Rectangle tmp = new Rectangle((int)(-Camera.CameraPosition.X), (int)(-Camera.CameraPosition.Y), 1400, 900);
        foreach (Visual v in blocks)
        {


            if (tmp.Contains(new Rectangle((int)(v.Pose.Position.X) - (int)(Camera.CameraPosition.X), (int)(v.Pose.Position.Y) - (int)(Camera.CameraPosition.Y), -(int)v.Pose.Scale.X * 64, -(int)v.Pose.Scale.Y * 64)))
            {
                inrange.Add(v);
            }
        }

        try
        {
            foreach (PointLight l in lights) l.Render(GraphicsDevice, inrange);
        }
        catch (System.InvalidOperationException e) { }
        
        inrange.Clear();
    }

    /// <summary>
    /// Draws all normal objects
    /// </summary>
    private void DrawColorMap(GraphicsDevice GraphicsDevice, SpriteBatch spriteBatch)
    {
        if (Options.GetValue<State>("role") != State.Doctor)
        GraphicsDevice.SetRenderTarget(colorMap);
        GraphicsDevice.Clear(Color.White);

        spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.Default, RasterizerState.CullCounterClockwise, null,ScreenManager.Instance.DrawScale());

        
        int size = 32;
        int minX = -(int)(Camera.CameraPosition.X/size);
        int minY = -(int)(Camera.CameraPosition.Y/size);
        int maxX = 2 + minX + 800 / size; //TODO: change to correct length
        int maxY = 2 + minY + 600 / size;
        for (int i = minX; i < maxX; i++)
            for (int j = minY; j < maxY; j++)
               if(textureGrid[i,j] != null)
                textureGrid[i, j].Draw(spriteBatch, new Vector2(i * size, j * size));
        
        foreach (Visual v in blocks)
        {
            Vector2 origin = new Vector2(v.Texture.Width / 2.0f, v.Texture.Height / 2.0f);

            spriteBatch.Draw(v.Texture, v.Pose.Position, null, Color.White, v.Pose.Rotation, origin, v.Pose.Scale, SpriteEffects.None, 0.1f);
        }

        foreach (EnergyCell ec in energyCells)
        {
            ec.Draw(spriteBatch); //TODO deze moeten achter shadow
        }
        foreach (Trap t in traps)
        {
            t.Draw(spriteBatch);
        }

        psDown.draw(spriteBatch);

        spriteBatch.End();
    }

    public static List<Visual> allBlocks
    {
        get { return blocks; }
    }

    public static int CellCount
    {
        get { return cellCount; }
        set { cellCount = value; }
    }

    public static Player Player
    {
        get { return player; }
    }
}