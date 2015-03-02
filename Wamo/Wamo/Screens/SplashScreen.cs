using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Input;

public class SplashScreen : GameScreen
{
    SpriteFont font;

    FileManager fileManager;
    List<FadeAnimation> fade;
    List<Texture2D> images;
    List<SoundEffect> sounds;

    int imageNumber;

    public override void LoadContent(ContentManager Content, InputManager inputManager)
    {
        base.LoadContent(Content, inputManager);

        if (font == null)
            font = content.Load<SpriteFont>("GUI/Fonts/debug");

        fileManager = new FileManager();
        fade = new List<FadeAnimation>();
        images = new List<Texture2D>();
        sounds = new List<SoundEffect>();

        fileManager.LoadContent("Load/splash.cme", attributes, contents);
        for(int i=0; i<attributes.Count; i++)
            for(int j=0; j<attributes[i].Count; j++)
            {
                switch(attributes[i][j])
                {
                    case "Image": 
                        images.Add(content.Load<Texture2D>(contents[i][j]));
                        fade.Add(new FadeAnimation());
                        break;
                    case "Sound": sounds.Add(content.Load<SoundEffect>(contents[i][j]));
                        break;
                }
            }

        for (int i = 0; i < fade.Count; i++)
        {
            string[] tmp = contents[i][1].Split(' ');
            fade[i].LoadContent(content, images[i], "", new Vector2(float.Parse(tmp[0]), float.Parse(tmp[1])));
            fade[i].Scale = float.Parse(contents[i][2]);
            fade[i].IsActive = true;
        }
    }

    public override void UnloadContent()
    {
        base.UnloadContent();
        fileManager = null;
    }

    public override void Update(GameTime gameTime)
    {
        inputManager.Update();

        fade[imageNumber].Update(gameTime);

        if (fade[imageNumber].Alpha == 0.0f)
            imageNumber++;

        if (imageNumber >= fade.Count - 1 || inputManager.KeyPressed(Keys.Space))
        {
            if (fade[imageNumber].Alpha != 1.0f)
                ScreenManager.Instance.AddScreen(new TitleScreen(), inputManager, fade[imageNumber].Alpha);
            else ScreenManager.Instance.AddScreen(new TitleScreen(), inputManager);
        }
        
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        fade[imageNumber].Draw(spriteBatch);
    }
}
