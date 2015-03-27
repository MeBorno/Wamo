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
    Vector2 oldPos;
    float angle;
    Color testColor = Color.White;
    Color color;
    ParticleSystem ps;
    float diagonalspeed, speed;

    public override void LoadContent(ContentManager content, InputManager inputManager)
    {
        diagonalspeed = 7.07f;
        speed = 5f;
        color = Color.White;
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

        if (Options.GetValue<bool>("boost"))
        {
            speed = 7.5f;
            diagonalspeed = 10.6f;
        }
        else
        {
            speed = 5f;
            diagonalspeed = 7.07f;
        }

        if (inputManager.KeyDown(Keys.Right, Keys.D))
        {
            if (inputManager.KeyDown(Keys.Down, Keys.S))
            {
               // moveAnimation.CurFrame = new Vector2(moveAnimation.CurFrame.X, 4);
                velocity.X += diagonalspeed;
                velocity.Y += diagonalspeed;
            }
            else if (inputManager.KeyDown(Keys.Up, Keys.W))
            {
               // moveAnimation.CurFrame = new Vector2(moveAnimation.CurFrame.X, 5);
                velocity.X += diagonalspeed;
                velocity.Y -= diagonalspeed;
            }
            else
            {
                //moveAnimation.CurFrame = new Vector2(moveAnimation.CurFrame.X, 2);
                velocity.X += speed;
            }
        }
        else if (inputManager.KeyDown(Keys.Left, Keys.A))
        {
            if (inputManager.KeyDown(Keys.Down, Keys.S))
            {
               // moveAnimation.CurFrame = new Vector2(moveAnimation.CurFrame.X, 6);
                velocity.X -= diagonalspeed;
                velocity.Y += diagonalspeed;
            }
            else if (inputManager.KeyDown(Keys.Up, Keys.W))
            {
                //moveAnimation.CurFrame = new Vector2(moveAnimation.CurFrame.X, 7);
                velocity.X -= diagonalspeed;
                velocity.Y -= diagonalspeed;
            }
            else
            {
               // moveAnimation.CurFrame = new Vector2(moveAnimation.CurFrame.X, 1);
                velocity.X -= speed;
            }
        }
        else if (inputManager.KeyDown(Keys.Up, Keys.W))
        {
           // moveAnimation.CurFrame = new Vector2(moveAnimation.CurFrame.X, 3);
            velocity.Y -= speed;
        }
        else if (inputManager.KeyDown(Keys.Down, Keys.S))
        {
            //moveAnimation.CurFrame = new Vector2(moveAnimation.CurFrame.X, 0);
            velocity.Y += speed;
        }
        

        double a = (inputManager.MousePos().Y - Camera.CameraPosition.Y) - position.Y;
        double b = (inputManager.MousePos().X - Camera.CameraPosition.X) - position.X;
        angle = (float)Math.Atan2(a, b);
        Movement();
        matrix =
            Matrix.CreateTranslation(16, 16, 0) *
            Matrix.CreateRotationZ(angle) *
            Matrix.CreateScale(1f) *
            Matrix.CreateTranslation(position.X, position.Y, 0);
        if (Options.GetValue<bool>("immune"))
            color = Color.Gold;
        else color = Color.White;

    }

    public void Movement()
    {
        oldPos = position;
        if (velocity != Vector2.Zero)
        {
            position += velocity / 10;
            
            if (velocity.X < 0.2f && velocity.X > -0.2f) velocity.X = 0;
            else { velocity.X = velocity.X / 1.50f; }
            if (velocity.Y < 0.01f && velocity.Y > -0.01f) velocity.Y = 0;
            else { velocity.Y = velocity.Y / 1.50f; }

        }

        //Rectangle playercollider = new Rectangle((int)(PlayerPosition.X / ScreenManager.Instance.DrawScale().M11), (int)(PlayerPosition.Y / ScreenManager.Instance.DrawScale().M22), 32, 32);
        Rectangle tmp = new Rectangle((int)this.position.X + (int)Camera.CameraPosition.X, (int)this.position.Y + (int)Camera.CameraPosition.Y, 32, 32);
        foreach (Visual v in GameplayScreen.allInrangeBlocks)
        {
            if (tmp.Intersects(new Rectangle((int)(v.Pose.Position.X), (int)(v.Pose.Position.Y), 32, 32)))
            {
                //testColor = Color.Red;
                position = oldPos;
                velocity = -velocity;
                break;
            }
            
            testColor = Color.Blue;
        }

        
       //check
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(image, position + Camera.CameraPosition, null, color, angle, new Vector2(16, 16), 1f, SpriteEffects.None, 0.3f);
        ps.draw(spriteBatch);
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

    public Color TestColor
    {
        get { return testColor; }
        set { testColor = value; }
    }
    
    public float Speed
    {
        get { return speed; }
        set { speed = value; }
    }

    public float Diagonalspeed
    {
        get { return diagonalspeed; }
        set { diagonalspeed = value; }
    }
}

