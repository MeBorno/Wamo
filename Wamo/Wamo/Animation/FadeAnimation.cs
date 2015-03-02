using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

public class FadeAnimation : Animation
{
    bool increase, startTimer, stopUpdate;
    float fadeSpeed, activateValue, dAlpha;
    TimeSpan dTime, timer;

    public override void LoadContent(ContentManager content, Texture2D image, string text, Vector2 position)
    {
        base.LoadContent(content, image, text, position);
        increase = false;
        fadeSpeed = 0.5f;
        dTime = new TimeSpan(0, 0, 1);
        timer = dTime;
        activateValue = 0.0f;
        stopUpdate = false;
        dAlpha = alpha;
    }

    public override void Update(GameTime gameTime)
    {
        if (isActive)
        {
            if(!stopUpdate)
            { 
                alpha += ((increase)?1:-1) * fadeSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;

                if (alpha <= 0.0f) { alpha = 0.0f; increase = true; }
                else if (alpha >= 1.0f) { alpha = 1.0f; increase = false; }
            }

            if (alpha == activateValue)
            {
                stopUpdate = true;
                timer -= gameTime.ElapsedGameTime;
                if (timer.TotalSeconds <= 0)
                {
                    timer = dTime;
                    stopUpdate = false;
                }
            }

        } else
        {
            alpha = dAlpha;
            stopUpdate = false;
        }
    }

    public override float Alpha
    {
        get { return alpha; }
        set 
        {
            alpha = value;
            if (alpha == 1.0f) increase = false;
            else if (alpha == 0.0f) increase = true;
        }
    }

    public float DefaultAlpha
    {
        get { return dAlpha; }
        set { dAlpha = value; }
    }

    public float ActivateValue
    {
        get { return activateValue; }
        set { activateValue = value; }
    }

    public float FadeSpeed
    {
        get { return fadeSpeed; }
        set { fadeSpeed = value; }
    }

    public TimeSpan Timer
    {
        get { return timer; }
        set { dTime = value; timer = dTime; }
    }

    public bool Increase
    {
        get { return increase; }
        set { increase = value; }
    }
}
