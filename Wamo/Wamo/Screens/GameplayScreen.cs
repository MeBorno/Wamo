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
    Player player;
    bool isPlayer = true;
    Texture2D testTile;
    Effect lightEffect;
    PointLight playerFOV;
    Visual testBlock;
    //InputManager inputManager;
    SpriteFont font;
    Vector2 oldCameraPosition;
    
    public override void LoadContent(ContentManager Content, InputManager inputManager)
    {
        base.LoadContent(Content, inputManager);
        font = content.Load<SpriteFont>("GUI/Fonts/debug");
        Texture2D blockTexture = Content.Load<Texture2D>("Block");
        Texture2D blockGlow = Content.Load<Texture2D>("BlockGlow");
        lightEffect = Content.Load<Effect>("Light");

        player = new Player();
        player.LoadContent(content, inputManager);
        testTile = content.Load<Texture2D>("Sprites/tile1");
        Wamo.lights.Clear();
        playerFOV = new PointLight(lightEffect, Vector2.Zero, 500, Color.White, 1.0f);
        testBlock = new Visual(blockTexture, new Vector2(250,150),45f,blockGlow);
        Wamo.lights.Add(playerFOV);
        Wamo.blocks.Add(testBlock);
        
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

    public override void Draw(SpriteBatch spriteBatch)
    {
       // spriteBatch.Draw(testTile, new Vector2(Camera.CameraPosition.X + 20, Camera.CameraPosition.Y + 20), Color.White);
        base.Draw(spriteBatch);
        player.Draw(spriteBatch);

        spriteBatch.DrawString(font, playerFOV.Position.X + "," + playerFOV.Position.Y, Camera.CameraPosition + new Vector2(100, 160), Color.Black);
        


    }
}