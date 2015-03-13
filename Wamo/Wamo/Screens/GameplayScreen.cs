using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;


public class GameplayScreen : GameScreen
{
    GraphicsDevice GraphicsDevice;

    Player player;
    bool isPlayer = true;
    Texture2D testTile;
    PointLight playerFOV;
    Visual testBlock;
    //InputManager inputManager;
    SpriteFont font;
    Vector2 oldCameraPosition;


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
        playerFOV = new PointLight(lightEffect, Vector2.Zero, 500, Color.White, 1.0f);
        testBlock = new Visual(blockTexture, new Vector2(250,150),45f,blockGlow);
        lights.Add(playerFOV);
        blocks.Add(testBlock);

        Options.SetValue("lightEngine", true);
        
    }

    public override void UnloadContent()
    {
        base.UnloadContent();
        player.UnloadContent();
    }

    public override void Update(GameTime gameTime)
    {
            inputManager.Update();
            playerFOV.Position = player.PlayerPosition + Camera.CameraPosition;

            if (oldCameraPosition != Camera.CameraPosition)
            {
                testBlock.Pose.Position = testBlock.Pose.Position - (oldCameraPosition - Camera.CameraPosition);
                oldCameraPosition = Camera.CameraPosition;
            }

        if(isPlayer)
        player.Update(gameTime, inputManager);
       //  else
       // {
            if (inputManager.KeyDown(Keys.Down,Keys.H))
                Camera.CameraPosition = new Vector2(Camera.CameraPosition.X, Camera.CameraPosition.Y - 10);
            if (inputManager.KeyDown(Keys.Up, Keys.Y))
                Camera.CameraPosition = new Vector2(Camera.CameraPosition.X, Camera.CameraPosition.Y + 10);
            if (inputManager.KeyDown(Keys.Left, Keys.G))
                Camera.CameraPosition = new Vector2(Camera.CameraPosition.X + 10, Camera.CameraPosition.Y);
            if (inputManager.KeyDown(Keys.Right, Keys.J))
                Camera.CameraPosition = new Vector2(Camera.CameraPosition.X - 10, Camera.CameraPosition.Y);
      //  }

            
    }

    public override void PreDraw(GraphicsDevice GraphicsDevice, SpriteBatch spriteBatch)
    {
        DrawColorMap(GraphicsDevice, spriteBatch); // Draw the colors
        DrawLightMap(GraphicsDevice, spriteBatch, 0.0f); // Draw the lights
        BlurRenderTarget(GraphicsDevice, lightMap, 2.5f);// Blurr the shadows
        CombineAndDraw(GraphicsDevice); // Combine
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        //spriteBatch.Draw(testTile, new Vector2(Camera.CameraPosition.X + 20, Camera.CameraPosition.Y + 20), Color.White);
        base.Draw(spriteBatch);
        player.Draw(spriteBatch);

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
                spriteBatch.Draw(v.Glow, v.Pose.Position, null, Color.White, v.Pose.Rotation, origin, v.Pose.Scale, SpriteEffects.None, 0);
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