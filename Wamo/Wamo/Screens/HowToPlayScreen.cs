using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TomShane.Neoforce.Controls;

public class HowToPlayScreen : GameScreen
{
    SpriteFont font;
    Texture2D screen1, screen2, screen3, screen4, currentscreen;
    Button[] screenButton;

    public override void LoadContent(ContentManager Content, InputManager inputManager)
    {
        base.LoadContent(Content, inputManager);
        if (font == null)
            font = content.Load<SpriteFont>("GUI/Fonts/debug");
        screen1 = content.Load<Texture2D>("GUI/HowTo/1");
        screen2 = content.Load<Texture2D>("GUI/HowTo/2");
        screen3 = content.Load<Texture2D>("GUI/HowTo/3");
        screen4 = content.Load<Texture2D>("GUI/HowTo/4");
        currentscreen = screen1;
        screenButton = new Button[4];

        Window screenBar = new Window(Wamo.manager);
        screenBar.Init();
        screenBar.SetPosition(Options.GetValue<int>("screenWidth") / 2 - 180, Options.GetValue<int>("screenHeight") - 160);
        screenBar.SetSize(360, 90);
        screenBar.Suspended = true; //geen events
        screenBar.Visible = true;
        screenBar.Resizable = false;
        screenBar.Passive = true; //geen user input
        screenBar.BorderVisible = false;

        for (int i = 0; i < 4; i++)
        {
            screenButton[i] = new Button(Wamo.manager);
            screenButton[i].Init();
            screenButton[i].Name = "a" + i;
            screenButton[i].SetPosition(10 + (70 * i), 10);
            screenButton[i].SetSize(60, 60);
            screenButton[i].Text = i.ToString();
            screenButton[i].Parent = screenBar;
            screenButton[i].Anchor = Anchors.None;
        }
       // Wamo.manager.Add(screenBar);
    }

    public override void UnloadContent()
    {
        base.UnloadContent();
    }

    public override void Update(GameTime gameTime)
    {
        inputManager.Update();
        if (inputManager.KeyPressed(Keys.D1)) 
            currentscreen = screen1;
        if (inputManager.KeyPressed(Keys.D2)) 
            currentscreen = screen2;
        if (inputManager.KeyPressed(Keys.D3)) 
            currentscreen = screen3;
        if (inputManager.KeyPressed(Keys.D4)) 
            currentscreen = screen4;
        if (inputManager.KeyPressed(Keys.Enter)) { } //terugnaarmenu
        
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(currentscreen, new Vector2(0, 0), Color.White);
    }
}
