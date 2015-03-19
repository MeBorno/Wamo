using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using TomShane.Neoforce.Controls;

public class GameplayScreen : GameScreen
{
    GraphicsDevice GraphicsDevice;
    Button button; 
    Player player;
    int timer = 0;
    roll isRoll = roll.Robot;
    SoundEffect beep;
    Texture2D testTile;
    PointLight playerFOV;
    Visual testBlock;
    //InputManager inputManager;
    SpriteFont font;
    Vector2 oldCameraPosition;

    TextBox[] abilityExpl;
    ProgressBar[] abilityProgress;
    Button[] abilityButton;
    
    List<Visual> blocks;
    List<PointLight> lights;

    RenderTarget2D colorMap;
    RenderTarget2D lightMap;
    RenderTarget2D blurMap;

    Effect lightEffect;
    Effect combineEffect;
    Effect blurEffect;

    Quad quad;
    
    public override void LoadContent(ContentManager Content, InputManager inputManager)
    {
        base.LoadContent(Content, inputManager);
        GraphicsDevice = Options.GetValue<GraphicsDevice>("GraphicsDevice");
        font = content.Load<SpriteFont>("GUI/Fonts/debug");
        Texture2D blockTexture = Content.Load<Texture2D>("Block");
        Texture2D blockGlow = Content.Load<Texture2D>("BlockGlow");
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
        testTile = content.Load<Texture2D>("Sprites/tile1");
        lights.Clear();
        if (isRoll == roll.System)
        {
            playerFOV = new PointLight(lightEffect, Vector2.Zero, 500, Color.White, 1.0f);
            lights.Add(playerFOV);
            
        }
        else if(isRoll == roll.Robot)
        {
            playerFOV = new PointLight(lightEffect, Vector2.Zero, 100, Color.White, 1.0f);
            lights.Add(playerFOV);
            
        }
        else
        {
            
        }

        Options.SetValue("lightEngine", true);

        for (int i = 0; i < 20; i++)
        {
            Pose2D newPose = new Pose2D(new Vector2(250 + (25 * i), 100), 0f, 0.10f);
            testBlock = new Visual(blockTexture, newPose);
            blocks.Add(testBlock);
        }

        button = new Button(Wamo.manager);
        button.Init();
        button.SetPosition(30, 30);
        button.Text = "hoi";
        Wamo.manager.Add(button);

        abilityProgress = new ProgressBar[5];
        abilityButton = new Button[5];
        abilityExpl = new TextBox[5];
        CreateHud();

        //lights.Add(new PointLight(lightEffect,new Vector2(300,300), 300, Color.Red));
       // lights.Add(new PointLight(lightEffect, new Vector2(150, 450), 300, Color.Green));
       // lights.Add(new PointLight(lightEffect, new Vector2(450, 450), 300, Color.Blue));
    }

    public override void UnloadContent()
    {
        base.UnloadContent();
        player.UnloadContent();
    }

    public void PlaySound(int soundID)
    {
        switch (soundID)
        {
            case 1: beep.Play(1.0f, -0.5f, 0.0f); break;
            case 2: beep.Play(1.0f, 0.0f, 0.0f); break;
            case 3: beep.Play(1.0f, 0.5f, 0.0f); break;
            case 4: beep.Play(1.0f, 0.0f, -1.0f); break;
            case 5: beep.Play(1.0f, 0.0f, 1.0f); break;

        }
    }

    public override void Update(GameTime gameTime)
    {
            inputManager.Update();
            if(isRoll == roll.Robot || isRoll == roll.System)
            playerFOV.Position = player.PlayerPosition + Camera.CameraPosition;

            if (oldCameraPosition != Camera.CameraPosition)
            {
                foreach (Visual v in blocks)
                    v.Pose.Position = v.Pose.Position - (oldCameraPosition - Camera.CameraPosition);
                oldCameraPosition = Camera.CameraPosition;
            }

        if(isRoll == roll.Robot || isRoll == roll.System) //TODO:: uiteindelijk alleen robot???
        player.Update(gameTime, inputManager);
       //  else
       // {
        if (inputManager.KeyDown(Keys.Down, Keys.H))
        {
            Camera.CameraPosition = new Vector2(Camera.CameraPosition.X, Camera.CameraPosition.Y - 10); PlaySound(1);
        }
        if (inputManager.KeyDown(Keys.Up, Keys.Y))
        {
            Camera.CameraPosition = new Vector2(Camera.CameraPosition.X, Camera.CameraPosition.Y + 10); PlaySound(2);
        }
        if (inputManager.KeyDown(Keys.Left, Keys.G))
        {
            Camera.CameraPosition = new Vector2(Camera.CameraPosition.X + 10, Camera.CameraPosition.Y); PlaySound(3);
        }
        if (inputManager.KeyDown(Keys.Right, Keys.J))
        {
            Camera.CameraPosition = new Vector2(Camera.CameraPosition.X - 10, Camera.CameraPosition.Y); PlaySound(4);
        }
      //  }
        timer += gameTime.ElapsedGameTime.Milliseconds;
        
            for (int i = 0; i < 5; i++) {
                if (abilityProgress[i].Value < abilityProgress[i].Range)
                    abilityProgress[i].Value += gameTime.ElapsedGameTime.Milliseconds;
                else
                {
                    abilityButton[i].Color = Color.Red;
                }
            
        }

            button.MousePress += button_MousePress;

            for (int i = 0; i < 5; i++)
            {
                    abilityButton[i].MousePress += b_clicked;
                    abilityButton[i].MouseOver += b_over;
                    abilityButton[i].MouseOut += b_out;
            }
      
        
    }

    void b_clicked(object sender, TomShane.Neoforce.Controls.EventArgs e)
    {
        Color[] colorRange;
        colorRange = new Color[5]{Color.Blue, Color.Red, Color.Yellow, Color.Purple, Color.Green};
        Button b = (Button)sender;
        for(int i = 0; i < 5; i++)
            if (b.Name == abilityProgress[i].Name + i)
            {
                if (abilityProgress[i].Value >= abilityProgress[i].Range)
                {
                    abilityProgress[i].Value = 0;
                    b.Color = Color.White;
                    lights[0].Color = colorRange[i];
                    lights[0].Radius = i * 100;
                }
                break;
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

    void button_MousePress(object sender, TomShane.Neoforce.Controls.EventArgs e)
    {
        if (!button.Pushed)
        {
            //hier kan iets komen?
            button.Pushed = true;
        }
        
        e.Handled = true;
    }

    public void CreateHud()
    {
        string[] abilityNames;
        string[] abilityDiscription;
        int[] abilityCooldowns;
        abilityNames = new string[5];
        abilityCooldowns = new int[5];
        abilityDiscription = new string[5];
        switch (isRoll)
        {
            case roll.Doctor: abilityNames = new string[5] { "damn", "wij", "zijn", "zo", "fucked" }; //namen van de abilities
                abilityCooldowns = new int[5] { 5000, 10000, 20000, 40000, 80000 }; //cooldown van de abilities
                abilityDiscription = new string[5] { "Dit is de eerste ability, het doet niks...", "oh waaait hoooo wat lalala", "ik heb te weinig geslapen", "dit is nummer 4 right", "kijk mij nou random shit bedenken." };
                break;
            case roll.Robot: abilityNames = new string[5] { "lol", "nee", "echt", "zo", "fucked" }; //namen van de abilities
                abilityCooldowns = new int[5] { 5000, 10000, 20000, 40000, 80000 }; //cooldown van de abilities
                abilityDiscription = new string[5] { "Dit is de eerste ability, het doet niks...", "oh waaait hoooo wat lalala", "ik heb te weinig geslapen", "dit is nummer 4 right", "kijk mij nou random shit bedenken." };
                break;
            case roll.System: abilityNames = new string[5] { "libraries", "helpen", "wel", "though", "hehe" }; //namen van de abilities
                abilityCooldowns = new int[5] { 5000, 10000, 20000, 40000, 80000 }; //cooldown van de abilities
                abilityDiscription = new string[5] { "Dit is de eerste ability, het doet niks...", "oh waaait hoooo wat lalala", "ik heb te weinig geslapen", "dit is nummer 4 right", "kijk mij nou random shit bedenken." };
                break;
        }

        #region abilityBar
        Window abilityBar = new Window(Wamo.manager);
        abilityBar.Init();
        abilityBar.SetPosition(Options.GetValue<int>("screenWidth")/2 - 180, Options.GetValue<int>("screenHeight") - 160);
        abilityBar.SetSize(360, 80);
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

         for (int i = 0; i < 5; i++ )
        {
            abilityExpl[i] = new TextBox(Wamo.manager);
            abilityExpl[i].Init();
            abilityExpl[i].SetPosition(Options.GetValue<int>("screenWidth") / 2 - 180, Options.GetValue<int>("screenHeight") - 205);
            abilityExpl[i].SetSize(360,40);
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

    public override void PreDraw(GraphicsDevice GraphicsDevice, SpriteBatch spriteBatch)
    {
        DrawColorMap(GraphicsDevice, spriteBatch);  // Draw the colors
    
        if (isRoll != roll.Doctor)
        {
            
            DrawLightMap(GraphicsDevice, spriteBatch, 0.0f); // Draw the lights
            BlurRenderTarget(GraphicsDevice, lightMap, 2.5f);// Blurr the shadows
            CombineAndDraw(GraphicsDevice); // Combine
        }
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        //spriteBatch.Draw(testTile, new Vector2(Camera.CameraPosition.X + 20, Camera.CameraPosition.Y + 20), Color.White);
        base.Draw(spriteBatch);
        player.Draw(spriteBatch);

        if(isRoll == roll.Robot || isRoll == roll.System)
        spriteBatch.DrawString(font, playerFOV.Position.X + "," + playerFOV.Position.Y, Camera.CameraPosition + new Vector2(100, 160), Color.Black);
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
                spriteBatch.Draw(v.Glow, v.Pose.Position + Camera.CameraPosition, null, Color.White, v.Pose.Rotation, origin, v.Pose.Scale, SpriteEffects.None, 0);
            }
        }

        spriteBatch.End();

        GraphicsDevice.BlendState = BlendState.Additive;
        // Samplers states are set by the shader itself            
        GraphicsDevice.DepthStencilState = DepthStencilState.None;
        GraphicsDevice.RasterizerState = RasterizerState.CullNone;

        if (Mouse.GetState().RightButton == ButtonState.Pressed)
            lights[0].Position = new Vector2(Mouse.GetState().X / ScreenManager.Instance.DrawScale().M11, Mouse.GetState().Y / ScreenManager.Instance.DrawScale().M22);
        
            
        foreach (PointLight l in lights)
            l.Render(GraphicsDevice, blocks);
    }

    /// <summary>
    /// Draws all normal objects
    /// </summary>
    private void DrawColorMap(GraphicsDevice GraphicsDevice, SpriteBatch spriteBatch)
    {
        if(isRoll != roll.Doctor)
        GraphicsDevice.SetRenderTarget(colorMap);
        GraphicsDevice.Clear(Color.White);

        spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.Default, RasterizerState.CullCounterClockwise, null,ScreenManager.Instance.DrawScale());

        foreach (Visual v in blocks)
        {
            Vector2 origin = new Vector2(v.Texture.Width / 2.0f, v.Texture.Height / 2.0f);

            spriteBatch.Draw(v.Texture, v.Pose.Position, null, Color.White, v.Pose.Rotation, origin, v.Pose.Scale, SpriteEffects.None, 0);
        }

        spriteBatch.End();
    }



    enum roll { Robot, Doctor, System };
}