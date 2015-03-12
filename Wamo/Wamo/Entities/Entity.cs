using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

public class Entity
{
    protected ContentManager content;
    protected FileManager fileManager;
    protected List<List<string>> attributes, contents;

    protected Vector2 position;
    protected Texture2D image;
    protected SpriteSheetAnimation moveAnimation;

    public virtual void LoadContent(ContentManager content, InputManager inputManager)
    {
        this.content = new ContentManager(content.ServiceProvider, "Content");
        attributes = new List<List<string>>();
        contents = new List<List<string>>();
    }

    public virtual void UnloadContent()
    {
        content.Unload();
    }

    public virtual void Update(GameTime gameTime, InputManager inputManager)
    {
        
    }

    public virtual void Draw(SpriteBatch spriteBatch)
    {

    }
}
