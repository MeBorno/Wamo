using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

public class Player : Entity
{
    private InputManager inputManager;

    public override void LoadContent(ContentManager content, InputManager inputManager)
    {
        base.LoadContent(content, inputManager);

        fileManager = new FileManager();
        moveAnimation = new SpriteSheetAnimation();
        fileManager.LoadContent("Load/Player.cme", attributes, contents);

        Vector2 tmpFrames = Vector2.Zero;

        for (int i = 0; i < attributes.Count; i++)
            for (int j = 0; j < attributes[i].Count; j++)
            {
                switch(attributes[i][j])
                {
                    case "Image": image = content.Load<Texture2D>(contents[i][j]);
                        break;
                    case "Position":
                    {
                        string[] pos = contents[i][j].Split(' ');
                        position = new Vector2(int.Parse(pos[0]), int.Parse(pos[1]));
                        break;
                    }
                    case "Frames":
                    {
                        string[] frames = contents[i][j].Split(' ');
                        tmpFrames = new Vector2(int.Parse(frames[0]), int.Parse(frames[1]));
                        break;
                    }
                }
            }
        
        moveAnimation.LoadContent(content, image, "", position);

    }

    public override void UnloadContent()
    {
        base.UnloadContent();
        moveAnimation.UnloadContent();
    }

    public override void Update(GameTime gameTime, InputManager inputManager)
    {
        moveAnimation.IsActive = true;

        if (inputManager.KeyDown(Keys.Right, Keys.D)) moveAnimation.CurFrame = new Vector2(moveAnimation.CurFrame.X, 2);
        else if (inputManager.KeyDown(Keys.Left, Keys.A)) moveAnimation.CurFrame = new Vector2(moveAnimation.CurFrame.X, 1);
        else moveAnimation.IsActive = false;

        moveAnimation.Update(gameTime);

    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        moveAnimation.Draw(spriteBatch);
    }
}
