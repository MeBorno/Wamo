using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

public class TitleScreen : GameScreen
{
    SpriteFont font;
    MenuManager menuManager;

    public override void LoadContent(ContentManager Content, InputManager inputManager)
    {
        base.LoadContent(Content, inputManager);

        if (font == null)
            font = content.Load<SpriteFont>("GUI/Fonts/debug");

        menuManager = new MenuManager();
        menuManager.LoadContent(Content);
    }

    public override void UnloadContent()
    {
        base.UnloadContent();
        menuManager.UnloadContent();
    }

    public override void Update(GameTime gameTime)
    {
        inputManager.Update();
        menuManager.Update(gameTime, inputManager);
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        menuManager.Draw(spriteBatch);
    }
}
