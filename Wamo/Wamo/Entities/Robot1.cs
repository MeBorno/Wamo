using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

public class Robot1 : Entity
{
    private InputManager inputManager;
    SpriteFont font;
    Vector2 velocity;

    public override void LoadContent(ContentManager content, InputManager inputManager)
    {
        velocity = Vector2.Zero;
        font = content.Load<SpriteFont>("GUI/Fonts/debug");
        base.LoadContent(content, inputManager);

        fileManager = new FileManager();
        moveAnimation = new SpriteSheetAnimation();
        fileManager.LoadContent("Load/robot1.cme", attributes, contents);

        Vector2 tmpFrames = Vector2.Zero;

        for (int i = 0; i < attributes.Count; i++)
            for (int j = 0; j < attributes[i].Count; j++)
            {
                switch (attributes[i][j])
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

    public void Update(GameTime gameTime, InputManager inputManager, Player player, PointLight playerFOV)
    {
        moveAnimation.IsActive = true;
        Vector2 positionDifference = this.position - player.PlayerPosition;
        
        

        moveAnimation.Update(gameTime);
        Movement();

    }

    public void Movement()
    {
        Rectangle playercollider = new Rectangle((int)RobotPosition.X, (int)RobotPosition.Y, 32, 32);
        foreach (Visual v in GameplayScreen.allBlocks)
        {
            if (playercollider.Intersects(new Rectangle((int)(v.Pose.Position.X / ScreenManager.Instance.DrawScale().M11), (int)(v.Pose.Position.Y / ScreenManager.Instance.DrawScale().M22), 32, 32)))
            {
                //  velocity = -velocity;
            }
        }

        if (velocity != Vector2.Zero)
        {
            moveAnimation.GlobalPos += velocity / 10;
            if (velocity.X < 0.2f && velocity.X > -0.2f) velocity.X = 0;
            else { velocity.X = velocity.X / 1.50f; }
            if (velocity.Y < 0.01f && velocity.Y > -0.01f) velocity.Y = 0;
            else { velocity.Y = velocity.Y / 1.50f; }

        }

    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        moveAnimation.Draw(spriteBatch);
    }

    public Vector2 RobotPosition
    {
        get { return moveAnimation.GlobalPos + new Vector2(moveAnimation.FrameWidth / 2, moveAnimation.FrameHeight / 2); }
    }
}

