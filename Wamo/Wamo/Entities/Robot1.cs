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
    SpriteFont font;
    Vector2 velocity;
    Vector2 globalPos;
    float angle = 0.0f;
    Color testColor = Color.Blue;
    

    public override void LoadContent(ContentManager content, InputManager inputManager)
    {
        velocity = Vector2.Zero;
        font = content.Load<SpriteFont>("GUI/Fonts/debug");
        base.LoadContent(content, inputManager);

        fileManager = new FileManager();
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
                }
            }
        globalPos = position;
    }

    public override void UnloadContent()
    {
        base.UnloadContent();
        moveAnimation.UnloadContent();
    }

    public void Update(GameTime gameTime, InputManager inputManager, Player player, List<Visual> blocks)
    {
        if (DoPathFinding(player, blocks))
        {
            Vector2 positionDifference = player.PlayerPosition - globalPos;
            positionDifference.Normalize();
            positionDifference = Vector2.Multiply(positionDifference, 5.0f);
            velocity = positionDifference;
            angle = (float)Math.Atan2(positionDifference.Y, positionDifference.X);
        }
        Movement();

    }

    public void Movement()
    {
        Vector2 oldPos = globalPos;
        if (velocity != Vector2.Zero)
        {
            globalPos += velocity / 10;

            if (velocity.X < 0.2f && velocity.X > -0.2f) velocity.X = 0;
            else { velocity.X = velocity.X / 1.50f; }
            if (velocity.Y < 0.01f && velocity.Y > -0.01f) velocity.Y = 0;
            else { velocity.Y = velocity.Y / 1.50f; }

        }
        Rectangle tmp = new Rectangle((int)this.globalPos.X + (int)Camera.CameraPosition.X, (int)this.globalPos.Y + (int)Camera.CameraPosition.Y, 32, 32);
        foreach (Visual v in GameplayScreen.allInrangeBlocks)
        {
            if (tmp.Intersects(new Rectangle((int)(v.Pose.Position.X), (int)(v.Pose.Position.Y), 32, 32)))
            {
                globalPos = oldPos;
                velocity = -velocity;
                break;
            }

            testColor = Color.Blue;
        }
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(image, globalPos + Camera.CameraPosition, null, testColor, angle, new Vector2(16, 16), 1f, SpriteEffects.None, 0.3f);
    }

    public bool DoPathFinding(Player player, List<Visual> blocks)
    {
        //Checks which blocks are in camera view and puts it in a list
        List<Visual> blocksinrange = new List<Visual>();
        Rectangle tmp = new Rectangle((int)(-Camera.CameraPosition.X), (int)(-Camera.CameraPosition.Y), 1600, 1200);
        foreach (Visual v in blocks)
        {
            if (tmp.Intersects(new Rectangle((int)(v.Pose.Position.X), (int)(v.Pose.Position.Y), (int)v.Pose.Scale.X * 64, (int)v.Pose.Scale.Y * 64)))
                blocksinrange.Add(v);
        }
        //Logging values, measuring distances etc.
        Vector2 tmpPosition = globalPos;
        Vector2 positionDifference = player.PlayerPosition - globalPos;
        Vector2 tmpPositionDifference = positionDifference;
        positionDifference.Normalize();
        positionDifference = Vector2.Multiply(positionDifference, 5.0f);
        Vector2 tmpVelocity = positionDifference;

        //Checks step by step (velocity influence) if the path is free
        while (tmpPosition != player.PlayerPosition)
        {
            tmpPosition += tmpVelocity;
            Rectangle tmp2 = new Rectangle((int)tmpPosition.X, (int)tmpPosition.Y, 32, 32);
            foreach (Visual v in blocksinrange)
            {
                if (tmp2.Intersects(new Rectangle((int)(v.Pose.Position.X), (int)(v.Pose.Position.Y), 32, 32)))
                    return false;
                if (tmpPosition.X > -tmpPositionDifference.X && tmpPosition.Y > -tmpPositionDifference.Y)
                    return true;
            }
        }
        return true;
    }

    public Vector2 RobotPosition
    {
        get { return globalPos; }
    }
}

