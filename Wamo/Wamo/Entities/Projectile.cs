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
    Texture2D image;
    Rectangle placement;
    Vector2 position, direction, speed;
    Boolean visible = true;
    ParticleSystem ps;
    float angle;

    public Projectile(string imagelink, Vector2 pos, Vector2 dir, float angl)
    {
        this.image = Wamo.manager.Content.Load<Texture2D>("content/" + imagelink);
        this.position = pos;
        this.direction = dir;
        placement = new Rectangle((int)position.X, (int)position.Y, this.image.Width, this.image.Height);
        ps = new ParticleSystem();
        angle = angl;
        //projec moet van pos naar direction gaan, via speed. angle gebruiken?
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
    }

    public Boolean CheckCollision(Rectangle enemy)
    {
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
    }


    public override void Draw(SpriteBatch spriteBatch)
    {
        if (visible)
            spriteBatch.Draw(image, placement, null, Color.White, 0.0f, new Vector2(75f, 12.5f), SpriteEffects.None, 0f);
    }
}

