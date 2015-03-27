using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TomShane.Neoforce.Controls;

public class Robot1 : Entity
{
    SpriteFont font;
    Vector2 velocity, knockbackVelocity;
    float angle = 0.0f;
    Color testColor = Color.White;


    public void LoadContent(ContentManager content, InputManager inputManager, Vector2 positionz)
    {
        base.LoadContent(content, inputManager);       
        velocity = Vector2.Zero;
        font = content.Load<SpriteFont>("GUI/Fonts/debug");
        image = content.Load<Texture2D>("Sprites/test");
        this.position = positionz;        
        pixeldata = TextureTo2DArray(image);
    }

    public override void UnloadContent()
    {
        base.UnloadContent();
        moveAnimation.UnloadContent();
    }

    public void Update(GameTime gameTime, InputManager inputManager, Player player, List<Visual> blocks, ProgressBar healthBar)
    {
        if (DoPathFinding(player, blocks))
        {
            Vector2 positionDifference = player.Position - position;
            positionDifference.Normalize();
            positionDifference = Vector2.Multiply(positionDifference, 5.0f);
            velocity = positionDifference;
            angle = (float)Math.Atan2(positionDifference.Y, positionDifference.X);
        }

        Movement();
        CheckPlayerCollision(player, healthBar);

        matrix =
            Matrix.CreateTranslation(16, 16, 0) *
            Matrix.CreateRotationZ(angle) *
            Matrix.CreateScale(1f) *
            Matrix.CreateTranslation(position.X, position.Y, 0);

    }

    public void Movement()
    {
        Vector2 oldPos = position;
        if (velocity != Vector2.Zero)
        {
            position += velocity / 10;

            if (velocity.X < 0.2f && velocity.X > -0.2f) velocity.X = 0;
            else { velocity.X = velocity.X / 1.50f; }
            if (velocity.Y < 0.01f && velocity.Y > -0.01f) velocity.Y = 0;
            else { velocity.Y = velocity.Y / 1.50f; }

        }

        if (knockbackVelocity != Vector2.Zero)
        {
            position += knockbackVelocity;

            if (knockbackVelocity.X < 0.2f && knockbackVelocity.X > -0.2f) knockbackVelocity.X = 0;
            else { knockbackVelocity.X = knockbackVelocity.X / 1.20f; }
            if (knockbackVelocity.Y < 0.01f && knockbackVelocity.Y > -0.01f) knockbackVelocity.Y = 0;
            else { knockbackVelocity.Y = knockbackVelocity.Y / 1.20f; }

        }

        Rectangle tmp = new Rectangle((int)this.position.X + (int)Camera.CameraPosition.X, (int)this.position.Y + (int)Camera.CameraPosition.Y, 32, 32);
        foreach (Visual v in GameplayScreen.allInrangeBlocks)
        {
            if (tmp.Intersects(new Rectangle((int)(v.Pose.Position.X), (int)(v.Pose.Position.Y), 32, 32)))
            {
                position = oldPos;
                velocity = -velocity;
                break;
            }

            //testColor = Color.Blue;
        }
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(image, position + Camera.CameraPosition, null, testColor, angle, new Vector2(16, 16), 1f, SpriteEffects.None, 0.1f);
    }

    public bool DoPathFinding(Player player, List<Visual> blocks)
    {
        //Checks which blocks are in camera view and puts it in a list
        List<Visual> blocksinrange = GameplayScreen.allInrangeBlocks;
        
        //Logging values, measuring distances etc.
        Vector2 tmpPosition = position;
        Vector2 positionDifference = player.Position - position;
        Vector2 tmpPositionDifference = positionDifference;
        positionDifference.Normalize();
        positionDifference = Vector2.Multiply(positionDifference, 5.0f);
        Vector2 tmpVelocity = positionDifference;

        //Checks step by step (velocity influence) if the path is free
        while (tmpPosition != player.Position)
        {
            tmpPosition += tmpVelocity; //dit is te weinig om een pad te kunnen berekenen volgens mij. game blijft te lang in de game loop
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

    public void CheckPlayerCollision(Player player, ProgressBar healthBar)
    {
        Vector2 positionDifference = player.Position - position;
        if (Collision.CollidesWith(this, player))
        {
            if (!Options.GetValue<bool>("immune")) healthBar.Value -= 10;
            if (positionDifference.X > 0)
            {
                player.Velocity = new Vector2(this.velocity.X * 15, player.Velocity.Y);
                knockbackVelocity.X = -this.velocity.X * 2;
            }
            if (positionDifference.X < 0)
            {
                player.Velocity = new Vector2(this.velocity.X * 15, player.Velocity.Y);
                knockbackVelocity.X = -this.velocity.X * 2;
            }
            if (positionDifference.Y > 0)
            {
                player.Velocity = new Vector2(player.Velocity.X, this.velocity.Y * 15);
                knockbackVelocity.Y = -this.velocity.Y * 2;
            }
            if (positionDifference.Y < 0)
            {
                player.Velocity = new Vector2(player.Velocity.X, this.velocity.Y * 15);
                knockbackVelocity.Y = -this.velocity.Y * 2;
            }
            player.TestColor = Color.Red;
        }
        else
            player.TestColor = Color.Blue;
    }

    public Color TestColor
    {
        get { return testColor; }
        set { testColor = value; }
    }
}

