using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

public class RobotItem : Entity
{
    private InputManager inputManager;
    SpriteFont font;
    Texture2D cellTexture;
    Rectangle placement;
    Vector2 position;
    Boolean visible = true;
    int identifier;

    public RobotItem(Vector2 position,int id)
    {
        cellTexture = Wamo.manager.Content.Load<Texture2D>("content/goggles_24");
        placement = new Rectangle((int)position.X, (int)position.Y, cellTexture.Width, cellTexture.Height);
        this.position = position;
        identifier = id;
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

    public Boolean CheckCollision(Rectangle player)
    {
        Rectangle tmp = new Rectangle(player.X + (int)Camera.CameraPosition.X - 16, player.Y + (int)Camera.CameraPosition.Y - 16, player.Width, player.Height);
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
            spriteBatch.Draw(cellTexture, placement, null, Color.Pink, 0f, Vector2.Zero, SpriteEffects.None, 0f);
    }

    public int ItemIdentifier
    {
        get { return identifier; }
    }
}

