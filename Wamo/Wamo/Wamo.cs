using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Lidgren.Network;

public class Wamo : Microsoft.Xna.Framework.Game
{
    GraphicsDeviceManager graphics;
    SpriteBatch spriteBatch;

    public static List<Visual> blocks;

    public static List<PointLight> lights;

    RenderTarget2D colorMap;
    RenderTarget2D lightMap;
    RenderTarget2D blurMap;

    Effect lightEffect;
    Effect combineEffect;
    Effect blurEffect;

    Quad quad;
    
    public Wamo()
    {
        graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
    }

    protected override void Initialize()
    {
        Options.LoadOptions(Content.RootDirectory+"\\options.cme");
        NetworkManager.Instance.Initialize();
        ScreenManager.Instance.Initialize(this);
        ScreenManager.Instance.Dimensions = new Vector2(Options.GetValue<int>("screenWidth"), Options.GetValue<int>("screenHeight"));
        graphics.PreferredBackBufferWidth = (int)ScreenManager.Instance.Dimensions.X;
        graphics.PreferredBackBufferHeight = (int)ScreenManager.Instance.Dimensions.Y;
        graphics.ApplyChanges();

        this.IsMouseVisible = true;

        base.Initialize();
    }

    protected override void LoadContent()
    {
        spriteBatch = new SpriteBatch(GraphicsDevice);
        ScreenManager.Instance.LoadContent(Content);

       // int windowWidth = GraphicsDevice.Viewport.Width;
       // int windowHeight = GraphicsDevice.Viewport.Height;
        int windowWidth = 1920;
        int windowHeight = 1080;

        // Set up all render targets, the blur map doesn't need a depth buffer
        colorMap = new RenderTarget2D(GraphicsDevice, windowWidth, windowHeight, false, SurfaceFormat.Color, DepthFormat.Depth16, 16, RenderTargetUsage.DiscardContents);
        lightMap = new RenderTarget2D(GraphicsDevice, windowWidth, windowHeight, false, SurfaceFormat.Color, DepthFormat.Depth16, 16, RenderTargetUsage.DiscardContents);
        blurMap = new RenderTarget2D(GraphicsDevice, windowWidth, windowHeight, false, SurfaceFormat.Color, DepthFormat.None, 16, RenderTargetUsage.DiscardContents);

        combineEffect = Content.Load<Effect>("Combine");
        lightEffect = Content.Load<Effect>("Light");
        blurEffect = Content.Load<Effect>("Blur");

        quad = new Quad();

       // Texture2D blockTexture = Content.Load<Texture2D>("Block");
       // Texture2D blockGlow = Content.Load<Texture2D>("BlockGlow");

        blocks = new List<Visual>();
        lights = new List<PointLight>();
        lights.Add(new PointLight(lightEffect, new Vector2(300, 300), 500, Color.White, 1.0f));
        //hier nog niet light/blocks toevoegen right
    }

    protected override void UnloadContent()
    {
        ScreenManager.Instance.UnloadContent();
    }

    protected override void Update(GameTime gameTime)
    {
        if (Keyboard.GetState().IsKeyDown(Keys.Escape) || Options.GetValue<bool>("shutDown"))
            this.Exit();

        if (Options.GetValue<bool>("fullScreen") != graphics.IsFullScreen)
        {
            graphics.IsFullScreen = Options.GetValue<bool>("fullScreen");
            graphics.ApplyChanges();
        }

        NetworkManager.Instance.Update();
        ScreenManager.Instance.Update(gameTime);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.DarkGray);

        // Draw the colors
        DrawColorMap();

        // Draw the lights
        DrawLightMap(0.0f);

        // Blurr the shadows
        BlurRenderTarget(lightMap, 2.5f);

        // Combine
        CombineAndDraw();   

        spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, null, null, ScreenManager.Instance.DrawScale());
            ScreenManager.Instance.Draw(spriteBatch);
        spriteBatch.End();

        

        base.Draw(gameTime);
    }

    class Character
    {
        public int X { get; set; }
        public int Y { get; set; }
        public string Name { get; set; }
        public NetConnection Connection { get; set; }
        public Character(string name, int x, int y, NetConnection conn)
        {
            Name = name;
            X = x;
            Y = y;
            Connection = conn;
        }
        public Character()
        {
        }
    }

    enum PacketTypes
    {
        LOGIN,
        MOVE,
        WORLDSTATE
    }

    private void CombineAndDraw()
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
        private void BlurRenderTarget(RenderTarget2D target, float strength)
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
        private void DrawLightMap(float ambientLightStrength)
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
                    spriteBatch.Draw(v.Glow, v.Pose.Position, null, Color.White, v.Pose.Rotation, origin, v.Pose.Scale, SpriteEffects.None, 0);
                }
            }

            spriteBatch.End();

            GraphicsDevice.BlendState = BlendState.Additive;
            // Samplers states are set by the shader itself            
            GraphicsDevice.DepthStencilState = DepthStencilState.None;
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;

            if (Mouse.GetState().RightButton == ButtonState.Pressed)
            {
                lights[0].Position = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
            }
            
            foreach (PointLight l in lights)
            {                
                l.Render(GraphicsDevice, blocks);
            }
        }

        /// <summary>
        /// Draws all normal objects
        /// </summary>
        private void DrawColorMap()
        {
            GraphicsDevice.SetRenderTarget(colorMap);
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.Default, RasterizerState.CullCounterClockwise, null,ScreenManager.Instance.DrawScale());

            foreach (Visual v in blocks)
            {
                Vector2 origin = new Vector2(v.Texture.Width / 2.0f, v.Texture.Height / 2.0f);

                spriteBatch.Draw(v.Texture, v.Pose.Position, null, Color.White, v.Pose.Rotation, origin, v.Pose.Scale, SpriteEffects.None, 0);
            }

            spriteBatch.End();
        }       
    }
