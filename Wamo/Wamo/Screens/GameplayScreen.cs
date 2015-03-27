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
    #region Field Region
    GraphicsDevice GraphicsDevice;
    ParticleSystem psUp; //Voor particles die boven shadow liggen
    ParticleSystem psDown; //Voor particles die onder shadow liggen
    static Player player;
    Projectile rocket;
    double evilPoints = 30;
    SoundEffect beep;
    Tile[,] textureGrid;
    Texture2D tile;
    Texture2D blockTexture, longBlockTexture;
    Texture2D paralyze;

    public PointLight playerFOV;
    Visual newBlock;
    SpriteFont font;
    Vector2 oldCameraPosition;

    TextBox[] abilityExpl;
    ProgressBar[] abilityProgress;
    ProgressBar healthBar;
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
    List<Robot1> robots;
    List<Trap> traps;
    List<Projectile> projectiles;
    public static int cellCount = 0;

    Vector2 paintStartPos = Vector2.Zero;
    Vector2 paintEndPos = Vector2.Zero;

    RobotItem[] robotItems;
    static List<Visual> inrange;
    List<Visual> sonarBlocks;

    int[] timer;

    Texture2D winScreen, winDoctorScreen, lossScreen, lossDoctorScreen;
    #endregion

    public override void LoadContent(ContentManager Content, InputManager inputManager)
    {
        base.LoadContent(Content, inputManager);
        GraphicsDevice = Options.GetValue<GraphicsDevice>("GraphicsDevice");
        font = content.Load<SpriteFont>("GUI/Fonts/debug");
        blockTexture = Content.Load<Texture2D>("Block");
        longBlockTexture = Content.Load<Texture2D>("LongBlock");
        paralyze = Content.Load<Texture2D>("paralyse");
        beep = Content.Load<SoundEffect>("beep");
        lightEffect = Content.Load<Effect>("Light");
        timer = new int[10] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0};
        int windowWidth = Options.GetValue<int>("screenWidth");
        int windowHeight = Options.GetValue<int>("screenHeight");

        winScreen = Content.Load<Texture2D>("GUI/win");
        winDoctorScreen = Content.Load<Texture2D>("GUI/winDoctor");
        lossScreen = Content.Load<Texture2D>("GUI/loss");
        lossDoctorScreen = Content.Load<Texture2D>("GUI/lossDoctor");

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
        rocket = new Projectile("Sprites/rocket", new Vector2(-100, -100), Vector2.Zero, 0f);

        player.LoadContent(content, inputManager);


        Vector2 hjdkPosition = new Vector2(220, 250);

       
        lights.Clear();

        if ((Options.GetValue<State>("role") == State.None))
            Options.SetValue("role", State.Robot);

        if (Options.GetValue<State>("role") == State.System)
        {
            playerFOV = new PointLight(lightEffect, Vector2.Zero, 1500, Color.White, 1.0f);
            lights.Add(playerFOV);
            
        }

        if (Options.GetValue<State>("role") == State.Robot)
        {
            PointLight tmp = new PointLight(lightEffect, player.Position - Camera.CameraPosition, 200, Color.White, 0.0f);
            lights.Add(tmp);
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

        inrange = new List<Visual>();
        sonarBlocks = new List<Visual>();

        psUp = new ParticleSystem();
        psDown = new ParticleSystem();

        energyCells = new EnergyCell[9]{
            new EnergyCell(new Vector2(64,320)), new EnergyCell(new Vector2(64,690)), new EnergyCell(new Vector2(520,320)),
            new EnergyCell(new Vector2(290,864)), new EnergyCell(new Vector2(768,140)), new EnergyCell(new Vector2(600,416)),
            new EnergyCell(new Vector2(1120,224)), new EnergyCell(new Vector2(1216,288)), new EnergyCell(new Vector2(1120,640))
        };
        traps = new List<Trap>();
        robots = new List<Robot1>();
        projectiles = new List<Projectile>();
        projectiles.Add(rocket);
        robotItems = new RobotItem[6]{
            new RobotItem(new Vector2(128,128),0),new RobotItem(new Vector2(400,400),1),new RobotItem(new Vector2(800,80),2),
            new RobotItem(new Vector2(800,550),3),new RobotItem(new Vector2(512,1000),4),new RobotItem(new Vector2(700,700),5)
        };
        Robot1 testRobot = new Robot1();
        testRobot.LoadContent(content, inputManager, new Vector2(220,250));        
        robots.Add(testRobot);
    }

    public override void UnloadContent()
    {
        base.UnloadContent();
        player.UnloadContent();
        Wamo.manager.Dispose();
    }

    public override void NetworkMessage(NetIncomingMessage message)
    {
        message.Position = 0;
        PacketTypes type = (PacketTypes)message.ReadByte();
        if (type == PacketTypes.SOUNDEFFECT && Options.GetValue<bool>("scramble")) PlaySound(5- (int)message.ReadByte());
        else if (type == PacketTypes.SOUNDEFFECT) PlaySound((int)message.ReadByte());
        else if (type == PacketTypes.ABILITIES)
        {
            State state = (State)message.ReadByte();
            if (state == State.System)
            {
                int abil = (int)message.ReadByte();
                if (abil == 0) { }
                else if (abil == 1)  Options.SetValue("robotLight", true);
                else if (abil == 3 && Options.GetValue<State>("role") == State.Robot)
                {
                    string[] data = message.ReadString().Split(' ');
                    psUp.CreateTrail(100, new Vector2(float.Parse(data[0]), float.Parse(data[1])), new Vector2(float.Parse(data[2]), float.Parse(data[3])), Color.Red, true, 0.05f);
                }
                else if (abil == 4) Options.SetValue("paralyze", true);
            }
            else if(state == State.Doctor)
            {
                int abil = (int)message.ReadByte();
                if (abil == 0) { }
                else if (abil == 1 && Options.GetValue<State>("role") != State.Doctor)
                {
                    string[] data = message.ReadString().Split(' ');
                    traps.Add(new Trap(new Vector2(int.Parse(data[0]), int.Parse(data[1]))));
                }
                else if (abil == 3) Options.SetValue("fog", true);
                else if (abil == 4) Options.SetValue("scramble", true);
            }
            else if (state == State.Robot)
            {
                int abil = (int)message.ReadByte();
                if (abil == 0 && Options.GetValue<State>("role") != State.Robot) 
                {
                    string[] data = message.ReadString().Split(' ');
                    projectiles.Add(new Projectile("Sprites/rocket", new Vector2(float.Parse(data[0]), float.Parse(data[1])), Vector2.Zero, float.Parse(data[2])));
                }
                else if (abil == 1) { }
                else if (abil == 3) { }
                else if (abil == 4) Options.SetValue("immune", true);
            }
        }
        else if (type == PacketTypes.MOVE)
        {
            if (Options.GetValue<State>("role") != State.Robot)
            {
                string[] data = message.ReadString().Split(' ');
                player.Position = new Vector2(float.Parse(data[0]), float.Parse(data[1]));
                player.FacingAngle = float.Parse(data[2]);
                player.Velocity = new Vector2(float.Parse(data[3]), float.Parse(data[4]));
            }
        }
    }

    public override void Update(GameTime gameTime)
    {
        inputManager.Update();
        if (Options.GetValue<State>("role") == State.System)
            playerFOV.Position = player.Position + Camera.CameraPosition; //wat is dit? TODO

        if (Options.GetValue<State>("role") != State.Doctor)
        {
            Vector2 offset = Vector2.Zero;
            if (player.Position.Y > 300)
                offset.Y += (player.Position.Y - 300);
            if (player.Position.X > 400)
                offset.X += (player.Position.X - 400);
            if (player.Position.Y > 300 || player.Position.X > 400)
                Camera.CameraPosition = -offset;
        }

        if (oldCameraPosition != Camera.CameraPosition)
        {
            foreach (Visual v in blocks)
                v.Pose.Position = v.Pose.Position - (oldCameraPosition - Camera.CameraPosition);
            oldCameraPosition = Camera.CameraPosition;
        }

        //if (Options.GetValue<State>("role") == State.System ||
        //    Options.GetValue<State>("role") == State.Robot)
        
        CameraMovement(); //alles met camera movement


        if (Options.GetValue<State>("role") == State.Robot && !Options.GetValue<bool>("robotDead"))
        {
            player.Update(gameTime, inputManager);
            NetOutgoingMessage msg = NetworkManager.Instance.CreateMessage();
            msg.Write((byte)PacketTypes.MOVE);
            msg.Write((string)(player.Position.X + " " + player.Position.Y + " " + player.FacingAngle + " " + player.Velocity.X + " " + player.Velocity.Y));
            NetworkManager.Instance.SendMessage(msg);
        }

        evilPoints += 0.010;
        CheckAbilities(gameTime); //checks bij abilities
        psUp.update(gameTime); //update particlesystem
        psDown.update(gameTime); //update particlesystem
        CheckSound(gameTime);
        CheckItems(gameTime); //check traps,cells,projectiles
        FindNearbyBlocks(); //checkt alle blocks of ze dichtbij zijn

        foreach (Robot1 r in robots)
        {
            r.Update(gameTime, inputManager, player, blocks, healthBar);
        }

        if (healthBar.Value <= 0)
            Options.SetValue("robotDead", true);

        if (Options.GetValue<bool>("robotDead"))
        {
            if(inputManager.KeyPressed(Keys.Space))
                ScreenManager.Instance.AddScreen(new TitleScreen(), inputManager);

            if(Options.GetValue<State>("role") != State.Doctor)
            {
                //"You lose" - screen TODO::
            }
            else
            {
                //"You win" - screen TODO::
            }
        }
        



            Timers();
        }

    private static void FindNearbyBlocks()
    {
        inrange.Clear();
        Rectangle tmp = new Rectangle((int)(-Camera.CameraPosition.X), (int)(-Camera.CameraPosition.Y), 1400, 900);
        foreach (Visual v in blocks)
        {


            if (tmp.Contains(new Rectangle((int)(v.Pose.Position.X) - (int)(Camera.CameraPosition.X), (int)(v.Pose.Position.Y) - (int)(Camera.CameraPosition.Y), -(int)v.Pose.Scale.X * 64, -(int)v.Pose.Scale.Y * 64)))
            {
                inrange.Add(v);
            }
        }
    }

    private void CheckSound(GameTime gameTime)
    {
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
    }

    private void CheckItems(GameTime gameTime)
    {
        foreach (EnergyCell ev in energyCells)
        {
            ev.Update(gameTime, inputManager);
            ev.CheckCollision(new Rectangle((int)player.Position.X, (int)player.Position.Y, 32, 32));
        }
        foreach (Trap t in traps)
        {
            t.Update(gameTime, inputManager);
            if (t.CheckCollision(new Rectangle((int)player.Position.X, (int)player.Position.Y, 32, 32)))
            {
                psDown.CreateExplosion(40, new Vector2(player.Position.X - 16, player.Position.Y - 16), Color.Orange, true, 0.15f, 200f, 0.50f, 10f);
                psDown.CreateExplosion(30, new Vector2(player.Position.X - 16, player.Position.Y - 16), Color.Red, true, 0.15f, 300f, 0.50f, 10f);
                psDown.CreateExplosion(90, new Vector2(player.Position.X - 16, player.Position.Y - 16), Color.Gray, true, 0.05f, 500f, 0.60f, 1f);
                if (!Options.GetValue<bool>("immune")) healthBar.Value -= 10;
            }
        }
        foreach (Projectile p in projectiles)
        {
            p.Update(gameTime, inputManager);
            //if (p.CheckCollision()) collision met enemies vd
            //if (TexturesCollide(p.Pixeldata, p.Matrix, robot1.Pixeldata, robot1.Matrix) != new Vector2(-1,-1)) 
            foreach (Robot1 r in robots)
            {
                if (Collision.CollidesWith(p, r))
                {
                    Vector2 collpos = r.Position;
                    psUp.CreateExplosion(40, collpos + Camera.CameraPosition + new Vector2(16, 16), Color.Orange, true, 0.15f, 200f, 0.50f, 10f);
                    psUp.CreateExplosion(30, collpos + Camera.CameraPosition + new Vector2(16, 16), Color.Red, true, 0.15f, 300f, 0.50f, 10f);
                    psUp.CreateExplosion(90, collpos + Camera.CameraPosition + new Vector2(16, 16), Color.Gray, true, 0.05f, 500f, 0.60f, 1f);
                    r.UnloadContent();
                    rocket.StopUse();
                }
            }
        }

        for (int i = 0; i < 6; i++)
        {
            robotItems[i].Update(gameTime, inputManager);
            if (robotItems[i].CheckCollision(new Rectangle((int)player.Position.X, (int)player.Position.Y, 32, 32)))
            {
                if (Options.GetValue<State>("role") == State.Robot && i < 5)
                {
                    isUpgraded[i] = true;
                    upgradeButton[i].Text = "Found";
                    upgradeButton[i].BackColor = Color.Green;
                }
                else if (i == 6)
                {
                    Options.SetValue("allFound", true);
                    if (Options.GetValue<State>("role") != State.Doctor)
                    {
                        //"You win" - screen TODO::
                    }
                    else
                    {
                        //"You lose" - screen TODO::
                    }
                }
            }
        }
        if (cellCount >= 3 && Options.GetValue<State>("role") == State.System)
        
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

        if (evilPoints > 30 && Options.GetValue<State>("role") == State.Doctor)
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
    }

    private void CheckAbilities(GameTime gameTime)
    {
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

        if (Options.GetValue<State>("role") == State.System && Options.GetValue<bool>("fog")) //dit met global option thingy zodat er op system scherm fog ontstaat
        {
            psUp.CreateStorm(50, Camera.CameraPosition - new Vector2(200, 200), Camera.CameraPosition + new Vector2(1000, 800), Color.Gray); //dit moet eigenlijk resolution achtig iets zijn
            Options.SetValue("fog", false);
        }

        if (Options.GetValue<State>("role") == State.Doctor && Options.GetValue<bool>("paralyze"))
        {
            for (int i = 0; i < 5; i++)
                abilityButton[i].Enabled = false;
        }

        if (Options.GetValue<State>("role") == State.Robot && Options.GetValue<bool>("robotLight"))
        {
            try
            {
                lights[0].Radius = 200;
                lights[0].Power = 1.0f;
            } catch(Exception e) { }
        }

        if (Options.GetValue<bool>("paralyze"))
            timer[0] += gameTime.ElapsedGameTime.Milliseconds;

        if (Options.GetValue<bool>("robotLight"))
            timer[2] += gameTime.ElapsedGameTime.Milliseconds;

        if (Options.GetValue<bool>("scramble"))
            timer[3] += gameTime.ElapsedGameTime.Milliseconds;

        if (Options.GetValue<bool>("painting"))
            timer[4] += gameTime.ElapsedGameTime.Milliseconds;

        if (Options.GetValue<bool>("immune"))
            timer[1] += gameTime.ElapsedGameTime.Milliseconds;
        if (Options.GetValue<bool>("boost"))
            timer[5] += gameTime.ElapsedGameTime.Milliseconds;
        

    }

    private void CameraMovement()
    {
        if (Options.GetValue<State>("role") == State.System)
            playerFOV.Position = player.Position + Camera.CameraPosition; //wat is dit? TODO

        if (Options.GetValue<State>("role") != State.Doctor)
        {
            Vector2 offset = Vector2.Zero;
            if (player.Position.Y > 300)
                offset.Y += (player.Position.Y - 300);
            if (player.Position.X > 400)
                offset.X += (player.Position.X - 400);
            if (player.Position.Y > 300 || player.Position.X > 400)
                Camera.CameraPosition = -offset;
        }

        if (oldCameraPosition != Camera.CameraPosition)
        {
            foreach (Visual v in blocks)
                v.Pose.Position = v.Pose.Position - (oldCameraPosition - Camera.CameraPosition);
            oldCameraPosition = Camera.CameraPosition;
        }

        if (Options.GetValue<State>("role") == State.Doctor && !Options.GetValue<bool>("paralyze"))
        {
            if (inputManager.KeyDown(Keys.Down, Keys.S))
                Camera.CameraPosition = new Vector2(Camera.CameraPosition.X, Camera.CameraPosition.Y - 10);
            if (inputManager.KeyDown(Keys.Up, Keys.W))
                Camera.CameraPosition = new Vector2(Camera.CameraPosition.X, Camera.CameraPosition.Y + 10);
            if (inputManager.KeyDown(Keys.Left, Keys.A))
                Camera.CameraPosition = new Vector2(Camera.CameraPosition.X + 10, Camera.CameraPosition.Y);
            if (inputManager.KeyDown(Keys.Right, Keys.D))
                Camera.CameraPosition = new Vector2(Camera.CameraPosition.X - 10, Camera.CameraPosition.Y);
        }
    }

    public void Timers() //dit zijn alle ability timers
    {
        if (Options.GetValue<State>("role") == State.Doctor && timer[0] > 5000 && Options.GetValue<bool>("paralyze"))
        {
            timer[0] = 0;
            Options.SetValue("paralyze", false);
            for (int i = 0; i < 5; i++)
                abilityButton[i].Enabled = true;
        }

        if (timer[1] > 2500 && Options.GetValue<bool>("immune") && Options.GetValue<State>("role") == State.Robot && !isUpgraded[4])
        {
            timer[1] = 0;
            Options.SetValue("immune", false);
        }

        if (timer[1] > 4000 && Options.GetValue<bool>("immune") && Options.GetValue<State>("role") == State.Robot && isUpgraded[4])
        {
            timer[1] = 0;
            Options.SetValue("immune", false);
        }

        if (Options.GetValue<State>("role") == State.Robot && timer[2] > 5000 && Options.GetValue<bool>("robotLight"))
        {
            timer[2] = 0;
            Options.SetValue("robotLight", false);
            lights[0].Power = 0.0f;
            lights[0].Radius = 0;
        }
        if (Options.GetValue<State>("role") == State.System && timer[3] > 5000 && Options.GetValue<bool>("scramble"))
        {
            timer[3] = 0;
            Options.SetValue("scramble", false);
        }

        if (Options.GetValue<State>("role") == State.System && Options.GetValue<bool>("painting") && (timer[4] > 5000 || currentAbility != 3))
        {
            timer[4] = 0;
            Options.SetValue("painting", false);
            if (currentAbility == 3)
                usingAbility = false;
        }

        if (Options.GetValue<bool>("boost") && Options.GetValue<State>("role") == State.Robot && timer[5] > 5000)
        {
            timer[5] = 0;
            Options.SetValue("boost", false);
        }
      
        

    }
    

    public override void PreDraw(GraphicsDevice GraphicsDevice, SpriteBatch spriteBatch)
    {
        DrawColorMap(GraphicsDevice, spriteBatch);  // Draw the colors

        if (Options.GetValue<State>("role") != State.Doctor) // MOET DOCTOR ZIJN
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
        psUp.draw(spriteBatch);
        
        if (Options.GetValue<State>("role") == State.Doctor){
            spriteBatch.DrawString(font, "EvilPoints: " + (int)evilPoints, new Vector2(10, 30), Color.Red);
            if (Options.GetValue<bool>("paralyze"))
            {
                //spriteBatch.Draw(paralyze, new Vector2(Options.GetValue<int>("screenWidth") / 2 - 180, Options.GetValue<int>("screenHeight") - 160), null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.0f);
                spriteBatch.Draw(paralyze, new Vector2(210,245), null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.0f);
                
            }
        }
        if (Options.GetValue<State>("role") != State.Doctor)
            spriteBatch.DrawString(font, "Energy Cells: " + (int)cellCount, new Vector2(10, 30), Color.Blue); //TODO:: mooier font?

        if(Options.GetValue<State>("role") == State.Robot && currentAbility == 1)
            foreach (Visual v in sonarBlocks)
            {
                Vector2 origin = new Vector2(v.Texture.Width / 2.0f, v.Texture.Height / 2.0f);
                spriteBatch.Draw(v.Texture, v.Pose.Position, null, Color.White, v.Pose.Rotation, origin, v.Pose.Scale, SpriteEffects.None, 0.1f);
            }

        if (Options.GetValue<bool>("robotDead") && Options.GetValue<State>("role") != State.Doctor)
        {
            //"You lose" - screen TODO::
            spriteBatch.Draw(lossScreen, new Rectangle(0, 0, lossScreen.Width, lossScreen.Height), Color.White);
        }
        else if (Options.GetValue<bool>("robotDead"))
        {
            //"You win" - screen TODO::
            spriteBatch.Draw(winDoctorScreen, new Rectangle(0, 0, winScreen.Width, winScreen.Height), Color.White);
        }
        if (Options.GetValue<bool>("allFound") && Options.GetValue<State>("role") != State.Doctor)
        {
            //"You lose" - screen TODO::
            spriteBatch.Draw(winScreen, new Rectangle(0, 0, lossScreen.Width, lossScreen.Height), Color.White);
        }
        else if (Options.GetValue<bool>("allFound"))
        {
            //"You win" - screen TODO::
            spriteBatch.Draw(lossDoctorScreen, new Rectangle(0, 0, winScreen.Width, winScreen.Height), Color.White);
        }
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
                    case "3": textureGrid[i, count] = new Tile(tile, 3); break;//'NORMAL' TILES
                    case "4": textureGrid[i, count] = new Tile(tile, 4); break;
                    case "5": textureGrid[i, count] = new Tile(tile, 5); break;

                    case "6": textureGrid[i, count] = new Tile(tile, 6); break;
                    case "7": textureGrid[i, count] = new Tile(tile, 7); break; //DANGER TILES

                    case "-": textureGrid[i, count] = new Tile(tile, 12); break;
                    case "|": textureGrid[i, count] = new Tile(tile, 21); break;
                    case "X": textureGrid[i, count] = new Tile(tile, 20); break;//BLUE LINE TILES
                    case "Z": textureGrid[i, count] = new Tile(tile, 19); break;
                    case "A": textureGrid[i, count] = new Tile(tile, 11); break;
                    case "S": textureGrid[i, count] = new Tile(tile, 13); break;

                    case "Q": textureGrid[i, count] = new Tile(tile, 8); break;

                }
            }
           count++;
        }

    }

    public void PlaySound(int soundID)
    {
        //TODO scramble
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
        if (cellCount >= 3 && Options.GetValue<State>("role") == State.System)
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
        if (evilPoints >= 30 && Options.GetValue<State>("role") == State.Doctor)
        {
            Button b = (Button)sender;
            for (int i = 0; i < 5; i++)
                if (b.Name == "u" + i && isUpgraded[i] == false) //zoek id die overeenkomt met aangeklikte upgade button
                {
                    //TODO:: check/reset etcetera
                    isUpgraded[i] = true;
                    evilPoints -= 30;
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
            lights.Add(new PointLight(lightEffect, new Vector2(Mouse.GetState().X / ScreenManager.Instance.DrawScale().M11, Mouse.GetState().Y / ScreenManager.Instance.DrawScale().M22), 110, Color.Red, 1.0f));
            usingAbility = false;
        }
        
    }

    public void SysAbOne() //creating light at robot
    {
        //TODO:: stuur command zodat er licht komt bij de speler die robot is.
        //upgrade, meer zicht
        NetOutgoingMessage msg = NetworkManager.Instance.CreateMessage();
        msg.Write((byte)PacketTypes.ABILITIES);
        msg.Write((byte)Options.GetValue<State>("role"));
        msg.Write((byte)1);
        msg.Write((string)("robotLight true"));
        NetworkManager.Instance.SendMessage(msg);
        usingAbility = false;
    }
    public void SysAbTwo() 
    {
        //TODO:: remove trap.
        //upgrade, remove ook enemies
    }
    public void SysAbThree() 
    {

        Options.SetValue("painting", true);
        if (inputManager.MouseLeftButtonDown() && paintStartPos == Vector2.Zero)
            paintStartPos = new Vector2(Mouse.GetState().X / ScreenManager.Instance.DrawScale().M11, Mouse.GetState().Y / ScreenManager.Instance.DrawScale().M22);
        
        if(inputManager.MouseLeftButtonReleased())
            paintEndPos = new Vector2(Mouse.GetState().X / ScreenManager.Instance.DrawScale().M11, Mouse.GetState().Y / ScreenManager.Instance.DrawScale().M22);
        
        if (paintStartPos != Vector2.Zero && paintEndPos != Vector2.Zero)
        {
            psUp.CreateTrail(100, paintStartPos, paintEndPos, Color.Red, true,0.05f);

            NetOutgoingMessage msg = NetworkManager.Instance.CreateMessage();
            msg.Write((byte)PacketTypes.ABILITIES);
            msg.Write((byte)Options.GetValue<State>("role"));
            msg.Write((byte)3);
            msg.Write((string)(paintStartPos.X + " " + paintStartPos.Y + " " + paintEndPos.X + " " + paintEndPos.Y));
            NetworkManager.Instance.SendMessage(msg);

            //TODO stuur hier info naar robot if possible ;]]]]]
            paintStartPos = Vector2.Zero;
            paintEndPos = Vector2.Zero;
        }

    }
    public void SysAbFour() 
    {
        NetOutgoingMessage msg = NetworkManager.Instance.CreateMessage();
        msg.Write((byte)PacketTypes.ABILITIES);
        msg.Write((byte)Options.GetValue<State>("role"));
        msg.Write((byte)4);
        msg.Write((string)("paralyze true"));
        NetworkManager.Instance.SendMessage(msg);
        usingAbility = false;
    }
    #endregion
    #region doctor abilities
    private void DocAbFour()
    {
        NetOutgoingMessage msg = NetworkManager.Instance.CreateMessage();
        msg.Write((byte)PacketTypes.ABILITIES);
        msg.Write((byte)Options.GetValue<State>("role"));
        msg.Write((byte)4);
        msg.Write((string)("scramble true"));
        NetworkManager.Instance.SendMessage(msg);
        usingAbility = false;
    }

    private void DocAbThree()
    {
        NetOutgoingMessage msg = NetworkManager.Instance.CreateMessage();
        msg.Write((byte)PacketTypes.ABILITIES);
        msg.Write((byte)Options.GetValue<State>("role"));
        msg.Write((byte)3);
        msg.Write((string)("fog true"));
        NetworkManager.Instance.SendMessage(msg);
        usingAbility = false;
    }

    private void DocAbTwo()
    {
        if (inputManager.MouseLeftButtonReleased())
        {
           // Robot1 tmp = new Robot1(new Vector2((int)(((Mouse.GetState().X / ScreenManager.Instance.DrawScale().M11) - Camera.CameraPosition.X) / 16) * 16, (int)(((Mouse.GetState().Y / ScreenManager.Instance.DrawScale().M22) - Camera.CameraPosition.Y) / 16) * 16));
            
            Robot1 newRobot = new Robot1();
            newRobot.LoadContent(content, inputManager, (inputManager.MousePos() + Camera.CameraPosition));
            robots.Add(newRobot);
            usingAbility = false;
        }
    }

    private void DocAbOne()
    {
        if (inputManager.MouseLeftButtonReleased())
        {
            Trap tmp = new Trap(new Vector2((int)(((Mouse.GetState().X / ScreenManager.Instance.DrawScale().M11) - Camera.CameraPosition.X) / 16) * 16, (int)(((Mouse.GetState().Y / ScreenManager.Instance.DrawScale().M22) - Camera.CameraPosition.Y) / 16) * 16));

            NetOutgoingMessage msg = NetworkManager.Instance.CreateMessage();
            msg.Write((byte)PacketTypes.ABILITIES);
            msg.Write((byte)Options.GetValue<State>("role"));
            msg.Write((byte)1);
            msg.Write((string)(tmp.Position.X + " " + tmp.Position.Y));
            NetworkManager.Instance.SendMessage(msg);

            traps.Add(tmp);
            usingAbility = false;
        }
    }

    private void DocAbZero()
    {
        if (inputManager.MouseLeftButtonReleased())
        {
            Visual door = new Visual(blockTexture, new Pose2D(new Vector2(16 + (int)(((Mouse.GetState().X / ScreenManager.Instance.DrawScale().M11) - Camera.CameraPosition.X) / 32) * 32, 16 + (int)(((Mouse.GetState().Y / ScreenManager.Instance.DrawScale().M22) - Camera.CameraPosition.Y) /32) * 32),0f,1f));
            blocks.Add(door);
            usingAbility = false;
        }
    }
        #endregion
    #region robot abilities
    private void RobAbFour()
    {
        //invincibility
        NetOutgoingMessage msg = NetworkManager.Instance.CreateMessage();
        msg.Write((byte)PacketTypes.ABILITIES);
        msg.Write((byte)Options.GetValue<State>("role"));
        msg.Write((byte)4);
        msg.Write((string)("immune true"));
        NetworkManager.Instance.SendMessage(msg);
        usingAbility = false;
    }

    private void RobAbThree() 
    {

        //unlock door

    }

    private void RobAbTwo()
    {
        //speedboost
        Options.SetValue("boost", true);
        usingAbility = false;
    }

    private void RobAbOne()
    {
        sonarBlocks.Clear();
        //Rectangle tmp = new Rectangle((int)(-Camera.CameraPosition.X) - 150, (int)(-Camera.CameraPosition.Y) - 150, 300, 300);
        Rectangle tmp = new Rectangle((int)(player.Position.X) - 150, (int)(player.Position.Y) - 150, 300, 300);
        foreach (Visual v in blocks)
        {
            if (tmp.Contains(new Rectangle((int)(v.Pose.Position.X) - (int)(Camera.CameraPosition.X), (int)(v.Pose.Position.Y) - (int)(Camera.CameraPosition.Y), -(int)v.Pose.Scale.X * 64, -(int)v.Pose.Scale.Y * 64)))
            {
                sonarBlocks.Add(v);
            }
        }
        usingAbility = false;
    }

    private void RobAbZero()
    {
        
        if (inputManager.MouseLeftButtonReleased())
        {
            rocket.SetUse(player.Position, player.FacingAngle);
            NetOutgoingMessage msg = NetworkManager.Instance.CreateMessage();
            msg.Write((byte)PacketTypes.ABILITIES);
            msg.Write((byte)Options.GetValue<State>("role"));
            msg.Write((byte)0);
            msg.Write((string)(rocket.Position.X + " " + rocket.Position.Y + " " + rocket.Angle));
            NetworkManager.Instance.SendMessage(msg);
            usingAbility = false;
        }
         

        if (inputManager.MouseLeftButtonDown())
        {
            if(isUpgraded[0] == false)
                psUp.CreateCannon(null, 10, 300, player.Position + Camera.CameraPosition, new Vector2(Mouse.GetState().X / ScreenManager.Instance.DrawScale().M11, Mouse.GetState().Y / ScreenManager.Instance.DrawScale().M22), Color.Red, Color.Yellow);
            if(isUpgraded[0] == true)
                psUp.CreateCannon(null, 10, 300, player.Position + Camera.CameraPosition, new Vector2(Mouse.GetState().X / ScreenManager.Instance.DrawScale().M11, Mouse.GetState().Y / ScreenManager.Instance.DrawScale().M22), Color.Blue, Color.Yellow);

            //usingAbility = false;
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
            case State.Doctor: abilityNames = new string[5] { "Wall", "Trap", "Monster", "Fog", "Scramble" }; //namen van de abilities
                abilityCooldowns = new int[5] { 5000, 10000, 20000, 40000, 80000 }; //cooldown van de abilities
                abilityDiscription = new string[5] { "Place an impassable wall", "Create trap at mouse position", "Create monster at mouse position", "Fog the vision of the System", "Scramble the sounds of the System" };
                abilityUpgradeName = new string[5] { "Upgrade", "Upgrade", "Upgrade", "Upgrade", "Upgrade" };
                break;
            case State.Robot: abilityNames = new string[5] { "Shoot", "Sonar", "Boost", "Open Door", "Invincible" }; //namen van de abilities
                abilityCooldowns = new int[5] { 5000, 10000, 20000, 40000, 80000 }; //cooldown van de abilities
                abilityDiscription = new string[5] { "Shoots a rocket at the targeted location", "See walls in a short radius around you", "Move 50% faster", "Open a door", "Become immune to damage for 2.5 seconds" };
                break;
            case State.System: abilityNames = new string[5] { "Light", "Vision Surge", "Destroy", "Paint", "Paralyze" }; //namen van de abilities
                abilityCooldowns = new int[5] { 5000, 10000, 20000, 4000, 80000 }; //cooldown van de abilities
                abilityDiscription = new string[5] { "Create a small temporary light at the position of your mouse.", "Restore minimal vision for the Robot", "Destroy stuff from Doctor", "Paint at mouse for Robot", "Paralyze the doctor" };
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
                string[] soundNames;
                soundNames = new string[5]{"Low","Normal","High","Left","Right"};
                
                for (int i = 0; i < 5; i++)
                {
                    soundButton[i] = new Button(Wamo.manager);
                    soundButton[i].Init();
                    soundButton[i].Name = i.ToString();
                    soundButton[i].SetPosition(soundButtonPositions[i], soundButtonPositions[i + 5]);
                    soundButton[i].SetSize(60, 60);
                    soundButton[i].Text = soundNames[i];
                    soundButton[i].Parent = soundBar;
                    soundButton[i].Anchor = Anchors.None;
                }
                Wamo.manager.Add(soundBar);
#endregion
                    break;
        }

        healthBar = new ProgressBar(Wamo.manager);
        healthBar.Init();
        healthBar.Color = Color.Blue;
        healthBar.SetPosition(30, 30);
        healthBar.SetSize(200,20);
        healthBar.Value = 100;
        healthBar.Range = 100; //dit is de health van de speler;
        healthBar.Text = "50/100";
        healthBar.TextColor = Color.Black;
        
        Wamo.manager.Add(healthBar);

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

        try
        {
            foreach (PointLight l in lights) l.Render(GraphicsDevice, inrange);
        }
        catch (System.InvalidOperationException e) { } 
    }

    /// <summary>
    /// Draws all normal objects
    /// </summary>
    private void DrawColorMap(GraphicsDevice GraphicsDevice, SpriteBatch spriteBatch)
    {
        if (Options.GetValue<State>("role") != State.Doctor) //MOET DOCTOR ZIJN
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
       

        for (int i = 0; i < 6; i++)
            robotItems[i].Draw(spriteBatch);


        foreach (EnergyCell ec in energyCells)
        {
            ec.Draw(spriteBatch);
        }
        foreach (Trap t in traps)
        {
            t.Draw(spriteBatch);
        }

        foreach (Robot1 r in robots)
            r.Draw(spriteBatch);

        foreach (Projectile p in projectiles)
        {
            p.Draw(spriteBatch);
        }
        
            psDown.draw(spriteBatch);

        spriteBatch.End();
    }

    /*public Vector2 TexturesCollide(Color[,] tex1, Matrix mat1, Color[,] tex2, Matrix mat2)
    {
        Matrix mat1to2 = mat1 * Matrix.Invert(mat2);
        int width1 = tex1.GetLength(0);
        int height1 = tex1.GetLength(1);
        int width2 = tex2.GetLength(0);
        int height2 = tex2.GetLength(1);

        for (int x1 = 0; x1 < width1; x1++)
        {
            for (int y1 = 0; y1 < height1; y1++)
            {
                Vector2 pos1 = new Vector2(x1, y1);
                Vector2 pos2 = Vector2.Transform(pos1, mat1to2);

                int x2 = (int)pos2.X;
                int y2 = (int)pos2.Y;
                if ((x2 >= 0) && (x2 < width2))
                {
                    if ((y2 >= 0) && (y2 < height2))
                    {
                        if (tex1[x1, y1].A > 0)
                        {
                            if (tex2[x2, y2].A > 0)
                            {
                                Vector2 screenPos = Vector2.Transform(pos1, mat1);
                                return screenPos;
                            }
                        }
                    }
                }
            }
        }

        return new Vector2(-1, -1);
    }*/

    public static List<Visual> allInrangeBlocks
    {
        get { return inrange; }
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

    public ProgressBar HealthBar
    {
        get { return healthBar; }
    }
}