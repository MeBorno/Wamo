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
    SpriteFont font;
    Vector2 velocity;
    Vector2 globalPos;
    Vector2 oldPos;
    float angle;
    Color testColor = Color.Blue;
    ParticleSystem ps;

    public override void LoadContent(ContentManager content, InputManager inputManager)
    {
        velocity = Vector2.Zero;
        font = content.Load<SpriteFont>("GUI/Fonts/debug");
        base.LoadContent(content, inputManager);

        fileManager = new FileManager();
      
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
                    
                }
            }
        pixeldata = TextureTo2DArray(image);
        globalPos = position;

        ps = new ParticleSystem();
      

    }

    public override void UnloadContent()
    {
        base.UnloadContent();
        moveAnimation.UnloadContent();
    }

    public override void Update(GameTime gameTime, InputManager inputManager)
    {
        ps.update(gameTime);

        if (inputManager.KeyDown(Keys.Right, Keys.D))
        {
            if (inputManager.KeyDown(Keys.Down, Keys.S))
            {
               // moveAnimation.CurFrame = new Vector2(moveAnimation.CurFrame.X, 4);
                velocity.X += 7.07f;
                velocity.Y += 7.07f;
            }
            else if (inputManager.KeyDown(Keys.Up, Keys.W))
            {
               // moveAnimation.CurFrame = new Vector2(moveAnimation.CurFrame.X, 5);
                velocity.X += 7.07f;
                velocity.Y -= 7.07f;
            }
            else
            {
                //moveAnimation.CurFrame = new Vector2(moveAnimation.CurFrame.X, 2);
                velocity.X += 10;
            }
        }
        else if (inputManager.KeyDown(Keys.Left, Keys.A))
        {
            if (inputManager.KeyDown(Keys.Down, Keys.S))
            {
               // moveAnimation.CurFrame = new Vector2(moveAnimation.CurFrame.X, 6);
                velocity.X -= 7.07f;
                velocity.Y += 7.07f;
            }
            else if (inputManager.KeyDown(Keys.Up, Keys.W))
            {
                //moveAnimation.CurFrame = new Vector2(moveAnimation.CurFrame.X, 7);
                velocity.X -= 7.07f;
                velocity.Y -= 7.07f;
            }
            else
            {
               // moveAnimation.CurFrame = new Vector2(moveAnimation.CurFrame.X, 1);
                velocity.X -= 10;
            }
        }
        else if (inputManager.KeyDown(Keys.Up, Keys.W))
        {
           // moveAnimation.CurFrame = new Vector2(moveAnimation.CurFrame.X, 3);
            velocity.Y -= 10;
        }
        else if (inputManager.KeyDown(Keys.Down, Keys.S))
        {
            //moveAnimation.CurFrame = new Vector2(moveAnimation.CurFrame.X, 0);
            velocity.Y += 10;
        }
        

        double a = (inputManager.MousePos().Y - Camera.CameraPosition.Y) - globalPos.Y;
        double b = (inputManager.MousePos().X - Camera.CameraPosition.X) - globalPos.X;
        angle = (float)Math.Atan2(a, b);
        Movement();
        matrix =
            Matrix.CreateTranslation(16, 16, 0) *
            Matrix.CreateRotationZ(angle) *
            Matrix.CreateScale(1f) *
            Matrix.CreateTranslation(globalPos.X, globalPos.Y, 0);


    }

    public void Movement()
    {
        oldPos = globalPos;
        if (velocity != Vector2.Zero)
        {
            globalPos += velocity / 10;
            
            if (velocity.X < 0.2f && velocity.X > -0.2f) velocity.X = 0;
            else { velocity.X = velocity.X / 1.50f; }
            if (velocity.Y < 0.01f && velocity.Y > -0.01f) velocity.Y = 0;
            else { velocity.Y = velocity.Y / 1.50f; }

        }

        //Rectangle playercollider = new Rectangle((int)(PlayerPosition.X / ScreenManager.Instance.DrawScale().M11), (int)(PlayerPosition.Y / ScreenManager.Instance.DrawScale().M22), 32, 32);
        Rectangle tmp = new Rectangle((int)this.globalPos.X + (int)Camera.CameraPosition.X, (int)this.globalPos.Y + (int)Camera.CameraPosition.Y, 32, 32);
        foreach (Visual v in GameplayScreen.allInrangeBlocks)
        {
            if (tmp.Intersects(new Rectangle((int)(v.Pose.Position.X), (int)(v.Pose.Position.Y), 32, 32)))
            {
                //testColor = Color.Red;
                globalPos = oldPos;
                velocity = -velocity;
                break;
            }
            
            testColor = Color.Blue;
        }

        
       //check
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(image, globalPos + Camera.CameraPosition, null, Color.White, angle, new Vector2(16, 16), 1f, SpriteEffects.None, 0.3f);
        ps.draw(spriteBatch);
    }

    public Vector2 PlayerPosition
    {
        get { return globalPos; }
        set { globalPos = value; }
    }

    public float FacingAngle
    {
        get { return angle; }
        set { angle = value; }
    }

    public Vector2 Velocity
    {
        get { return velocity; }
        set { velocity = value; }
    }
}

