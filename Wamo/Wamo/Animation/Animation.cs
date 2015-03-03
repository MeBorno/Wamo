using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

public class Animation
{
    private const string fontname = "GUI/Fonts/debug";

    protected Texture2D image;
    protected string text;
    protected SpriteFont font;
    protected Color color;
    protected Rectangle sourceRect;
    protected float rotation, scale, alpha;
    protected Vector2 origin, position;
    protected ContentManager content;
    protected bool isActive;
    protected Rectangle hitbox;

    public virtual void LoadContent(ContentManager content, Texture2D image, string text, Vector2 position)
    {
        this.content = new ContentManager(content.ServiceProvider, "Content");
        this.image = image;
        this.text = text;
        this.position = position;
        if (text != string.Empty)
        {
            font = content.Load<SpriteFont>(fontname);
            color = Color.White;
        }
        if (image != null)
            sourceRect = new Rectangle(0, 0, image.Width, image.Height);
        rotation = 0.0f;
        scale = 1.0f;
        alpha = 1.0f;
        if (text != string.Empty)
            hitbox = new Rectangle((int)position.X, (int)position.Y, Math.Max(image.Width, (int)font.MeasureString(text).X), Math.Max(image.Height, (int)font.MeasureString(text).Y));
        else hitbox = new Rectangle((int)position.X, (int)position.Y, 0, 0);
    }

    public virtual void UnloadContent()
    {
        this.content.Unload();
    }

    public virtual void Update(GameTime gameTime)
    {
        //hitbox = new Rectangle((int)position.X, (int)position.Y, hitbox.Width, hitbox.Height);
    }

    public virtual void Draw(SpriteBatch spriteBatch)
    {
        if (image != null)
        {
            origin = new Vector2(sourceRect.Width / 2, sourceRect.Height / 2);
            spriteBatch.Draw(image, (position + origin * scale), sourceRect, Color.White * alpha, rotation, origin, scale, SpriteEffects.None, 0.0f);
        }

        if (text != string.Empty)
        {
            origin = font.MeasureString(text) / 2;
            spriteBatch.DrawString(font, text, position + origin * scale, color * alpha, rotation, origin, scale, SpriteEffects.None, 0.0f);
        }
    }

    public bool IsActive
    {
        get { return isActive; }
        set { isActive = value; }
    }

    public virtual float Alpha
    {
        get { return alpha; }
        set { alpha = value; }
    }

    public float Scale
    {
        get { return scale; }
        set { scale = value; }
    }

    public SpriteFont Font
    {
        get { return font; }
        set { font = value; }
    }

    public Color Color
    {
        get { return color; }
        set { color = value; }
    }

    public Rectangle Hitbox
    {
        get { return hitbox; }
    }
}
