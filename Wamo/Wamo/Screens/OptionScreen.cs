using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

public class OptionScreen : GameScreen
{
    SpriteFont font;
    string text;

    public override void LoadContent(ContentManager Content, InputManager inputManager)
    {
        base.LoadContent(Content, inputManager);
        font = content.Load<SpriteFont>("GUI/Fonts/debug");
        text = "";
    }

    public override void UnloadContent()
    {
        base.UnloadContent();
    }

    public override void Update(GameTime gameTime)
    {
        inputManager.Update();

        text = "Update this shit!";
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        base.Draw(spriteBatch);
        Vector2 pos = (ScreenManager.Instance.BaseDimensions * new Vector2(0.5f, 0.5f)) - font.MeasureString(text) * new Vector2(0.5f, 2.0f);
        spriteBatch.DrawString(font, text, pos, Color.White);
    }
}