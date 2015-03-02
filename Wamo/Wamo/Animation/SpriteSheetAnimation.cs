using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

public class SpriteSheetAnimation : Animation
{
    float frameCounter;
    int switchFrame;

    Vector2 frames;
    Vector2 curFrame;

    public override void LoadContent(ContentManager content, Texture2D image, string text, Vector2 position)
    {
        base.LoadContent(content, image, text, position);
        frameCounter = 0;
        switchFrame = 100;
        frames = new Vector2(3, 4);
        curFrame = new Vector2(0, 0);
        
        sourceRect = new Rectangle((int)curFrame.X * FrameWidth, (int)curFrame.Y * FrameHeight, FrameWidth, FrameHeight);
    }

    public override void UnloadContent()
    {
        base.UnloadContent();
    }

    public override void Update(GameTime gameTime)
    {
        if (isActive)
        {
            frameCounter += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            if(frameCounter >= switchFrame)
            {
                frameCounter = 0;
                curFrame.X++;
                if (curFrame.X * FrameWidth >= image.Width) curFrame.X = 0;
            }
        }
        else frameCounter = 0;

        sourceRect = new Rectangle((int)curFrame.X * FrameWidth, (int)curFrame.Y * FrameHeight, FrameWidth, FrameHeight);
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        base.Draw(spriteBatch);
    }

    public Vector2 Frames
    {
        get { return frames; }
        set { frames = value; }
    }

    public Vector2 CurFrame
    {
        get { return curFrame; }
        set { curFrame = value; }
    }

    public int FrameWidth
    {
        get { return image.Width / (int)frames.X; }
    }
    public int FrameHeight
    {
        get { return image.Height / (int)frames.Y; }
    }

}
