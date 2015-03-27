using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public class Tile : Entity
{
    /*
     * Field Region
     */
    protected int spriteID;
    protected bool solid;

    public static Point tile_size = new Point(32, 32);

    /*
     * Constructor
     */
    public Tile(Texture2D image, int spriteID, bool isSolid = false)
    {
        this.image = image;
        this.spriteID = spriteID;
        this.solid = isSolid;
    }

    public Tile(Texture2D image, int spriteX, int spriteY, bool isSolid = false)
    {
        this.image = image;
        this.spriteID = (spriteY * (image.Width / tile_size.X)) + spriteX;
        this.solid = isSolid;
    }

    /* 
     * Methods
     */
    public void Draw(SpriteBatch spriteBatch,Vector2 position)
    {
        int x, y;
        getGridLocation(spriteID, out x, out y);

        spriteBatch.Draw(image, new Rectangle((int)(position.X + Camera.CameraPosition.X), (int)(position.Y + Camera.CameraPosition.Y), tile_size.X, tile_size.Y),
                                 new Rectangle((int)x, (int)y, tile_size.X, tile_size.Y),
                                 Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.2f);
    }


    /// <summary>
    /// Returns if the tile is solid or not
    /// </summary>
    /// <returns></returns>
    public bool isSolid()
    {
        if (solid) return true;
        else { return false; }
    }


    /// <summary>
    /// returns the x and y location of the spriteID in the spritegrid
    /// </summary>
    /// <param name="id"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public void getGridLocation(int id, out int x, out int y)
    {
        int columns = image.Width / tile_size.X;
        int nx = 0, ny = 0;
        nx = (int)Math.Floor((double)(id % columns));// id % columns;
        ny = (int)Math.Floor((double)(id / columns));
        x = nx * tile_size.X;
        y = ny * tile_size.Y;
    }

    /*
     * Properties
     */
    public int SpriteID
    {
        get { return spriteID; }
        set { spriteID = Math.Max(0, Math.Min(256 * 256, value)); }
    }

    public bool Solid
    {
        get { return solid; }
        set { solid = value; }
    }

    public TileType GetType()
    {
        switch (spriteID)
        {
            case 0x7A:
            case 0x7B:
            case 0x8A:
            case 0x8B:
                return TileType.grass;
            case 0x03:
            case 0x04:
            case 0x05:
            case 0x13:
            case 0x14:
            case 0x15:
            case 0x23:
            case 0x24:
            case 0x25:
            case 0x33:
            case 0x34:
            case 0x43:
            case 0x44:
            case 0x53:
            case 0x54:
            case 0x55:
            case 0x63:
            case 0x64:
            case 0x65:
            case 0x73:
            case 0x74:
            case 0x75:
                return TileType.road;
            default: return TileType.generic;
        }
    }
}

public enum TileType
{
    generic = 0x00,
    grass = 0x01,
    water = 0x02,
    road = 0x03
}