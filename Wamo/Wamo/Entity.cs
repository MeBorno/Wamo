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
    //protected FileManager fileManager;

    public virtual void LoadContent(ContentManager content, InputManager inputManager)
    {
        this.content = new ContentManager(content.ServiceProvider, "Content");
    }

    public virtual void UnloadContent()
    {
        content.Unload();
    }

    public virtual void Update(GameTime gameTime)
    {

    }

    public virtual void Draw(SpriteBatch spriteBatch)
    {

    }
}
