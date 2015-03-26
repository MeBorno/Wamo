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
    //pixeldata en matrix zijn voor pixelcollision
    protected Color[,] pixeldata;
    protected Matrix matrix;

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

    public Color[,] TextureTo2DArray(Texture2D texture)
    {
        Color[] colors1D = new Color[texture.Width * texture.Height];
        texture.GetData(colors1D);

        Color[,] colors2D = new Color[texture.Width, texture.Height];
        for (int x = 0; x < texture.Width; x++)
            for (int y = 0; y < texture.Height; y++)
                colors2D[x, y] = colors1D[x + y * texture.Width];

        return colors2D;
    }

    public Color[,] Pixeldata
    {
        get { return pixeldata; }
    }

    public Matrix Matrix
    {
        get { return matrix; }
    }

    
}
