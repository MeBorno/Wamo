using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

public class Projectile : Entity
{
    private InputManager inputManager;
    SpriteFont font;
    Rectangle placement;
    Vector2 direction, speed;
    Boolean visible = true;
    ParticleSystem ps;
    float angle;

    public Projectile(string imagelink, Vector2 pos, Vector2 dir, float angl)
    {
        this.image = Wamo.manager.Content.Load<Texture2D>("content/" + imagelink);
        pixeldata = TextureTo2DArray(image);
        this.position = pos;
        this.direction = dir;
        placement = new Rectangle((int)position.X, (int)position.Y, this.image.Width, this.image.Height);
        ps = new ParticleSystem();
        angle = angl;
        //projec moet van pos naar direction gaan, via speed. angle gebruiken?\
        speed = new Vector2((float)Math.Cos(angl) * 7f, (float)Math.Sin(angl) * 7f);
    }

    public override void LoadContent(ContentManager content, InputManager inputManager)
    {
        font = content.Load<SpriteFont>("GUI/Fonts/debug");
        base.LoadContent(content, inputManager);
    }
    
    public override void UnloadContent()
    {
        base.UnloadContent();
        moveAnimation.UnloadContent();
    }

    public override void Update(GameTime gameTime, InputManager inputManager)
    {
        position += speed;
        placement = new Rectangle((int)this.position.X + (int)Camera.CameraPosition.X, (int)this.position.Y + (int)Camera.CameraPosition.Y, image.Width, image.Height);
        matrix =
            Matrix.CreateTranslation(0, image.Height / 2, 0) *
            Matrix.CreateRotationZ(angle) *
            Matrix.CreateScale(1f) *
            Matrix.CreateTranslation(position.X, position.Y, 0);
    }

    public Boolean CheckCollision(Rectangle enemy)
    {
        //WERKT NIET VOOR ROTATED SPRITES
        Rectangle tmp = new Rectangle(enemy.X + (int)Camera.CameraPosition.X, enemy.Y + (int)Camera.CameraPosition.Y, enemy.Width, enemy.Height);
        if (this.visible && this.placement.Intersects(tmp))
        {
            this.visible = false;
            return true;
        }
        else
        {
            return false;
        }
        //WERKT NIET VOOR ROTATED SPRITES
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        if (visible)
            spriteBatch.Draw(image, placement, null, Color.White, angle, new Vector2(0, image.Height/2), SpriteEffects.None, 0f);
    }

    public float Angle
    {
        get { return angle; }
    }
}

