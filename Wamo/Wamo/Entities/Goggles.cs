using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

public class Goggles : Entity
{
    private InputManager inputManager;
    SpriteFont font;
    Texture2D cellTexture;
    Rectangle placement;
    Vector2 position;
    Boolean visible = true;

    public Goggles(Vector2 position)
    {
        cellTexture = Wamo.manager.Content.Load<Texture2D>("content/goggles_24");
        placement = new Rectangle((int)position.X, (int)position.Y, cellTexture.Width, cellTexture.Height);
        this.position = position;
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
        placement = new Rectangle((int)this.position.X + (int)Camera.CameraPosition.X, (int)this.position.Y + (int)Camera.CameraPosition.Y, cellTexture.Width, cellTexture.Height);
    }

    public void CheckCollision(Rectangle player)
    {
        Rectangle tmp = new Rectangle(player.X + (int)Camera.CameraPosition.X, player.Y + (int)Camera.CameraPosition.Y, player.Width, player.Height);
        if (this.visible && this.placement.Intersects(tmp))
        {
            this.visible = false;

            // iets gebeurt met goggles.
            // verander draw van robot naar precies hetzelfde als dat van system


            // GameplayScreen.CellCount += 1;
        }
    }


    public override void Draw(SpriteBatch spriteBatch)
    {
        if (visible)
            spriteBatch.Draw(cellTexture, placement, null, Color.Pink, 0f, Vector2.Zero, SpriteEffects.None, 0f);
    }


}

