using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public class Collision
{
    private static Vector2 CalculateIntersectionDepth(Rectangle rectA, Rectangle rectB)
    {
        Vector2 minDistance = new Vector2(rectA.Width + rectB.Width,
            rectA.Height + rectB.Height) / 2;
        Vector2 centerA = new Vector2(rectA.Center.X, rectA.Center.Y);
        Vector2 centerB = new Vector2(rectB.Center.X, rectB.Center.Y);
        Vector2 distance = centerA - centerB;
        Vector2 depth = Vector2.Zero;
        if (distance.X > 0)
            depth.X = minDistance.X - distance.X;
        else
            depth.X = -minDistance.X - distance.X;
        if (distance.Y > 0)
            depth.Y = minDistance.Y - distance.Y;
        else
            depth.Y = -minDistance.Y - distance.Y;
        return depth;
    }

    private static Rectangle Intersection(Rectangle rect1, Rectangle rect2)
    {
        int xmin = (int)MathHelper.Max(rect1.Left, rect2.Left);
        int xmax = (int)MathHelper.Min(rect1.Right, rect2.Right);
        int ymin = (int)MathHelper.Max(rect1.Top, rect2.Top);
        int ymax = (int)MathHelper.Min(rect1.Bottom, rect2.Bottom);
        return new Rectangle(xmin, ymin, xmax - xmin, ymax - ymin);
    }

    public static bool CollidesWith(Entity obj1, Entity obj2)
    {
        Rectangle obj1Rectangle = new Rectangle((int)obj1.Position.X, (int)obj1.Position.Y, (int)obj1.Image.Width, (int)obj1.Image.Height);
        Rectangle obj2Rectangle = new Rectangle((int)obj2.Position.X, (int)obj2.Position.Y, (int)obj2.Image.Width, (int)obj2.Image.Height);
        if (!obj1Rectangle.Intersects(obj2Rectangle))
            return false;
        Rectangle b = Collision.Intersection(obj1Rectangle, obj2Rectangle);
        for (int x = 0; x < b.Width; x++)
            for (int y = 0; y < b.Height; y++)
            {
                int thisx = b.X - (int)(obj1.Position.X) + x;
                int thisy = b.Y - (int)(obj1.Position.Y) + y;
                int objx = b.X - (int)(obj2.Position.X) + x;
                int objy = b.Y - (int)(obj2.Position.Y) + y;
                if (GetPixelColor(obj1.Image, thisx, thisy).A != 0
                    && GetPixelColor(obj2.Image, objx, objy).A != 0)
                    return true;
            }
        return false;
    }

    private static Color GetPixelColor(Texture2D sprite, int x, int y)
    {
        Rectangle sourceRectangle = new Rectangle(x, y, 1, 1);
        Color[] retrievedColor = new Color[1];
        sprite.GetData<Color>(0, sourceRectangle, retrievedColor, 0, 1);
        return retrievedColor[0];
    }


}

