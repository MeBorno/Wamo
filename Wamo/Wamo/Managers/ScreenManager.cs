using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

public class ScreenManager
{
    #region Field Region

    private static ScreenManager instance;
    private InputManager inputManager;

    Dictionary<string, GameScreen> screens = new Dictionary<string, GameScreen>();
    Stack<GameScreen> screenStack = new Stack<GameScreen>();
    GameScreen currentScreen, newScreen;
    ContentManager content;
    Vector2 dimensions;

    bool transition;
    FadeAnimation fade;
    Texture2D fadeTexture;
    Texture2D nullImage;

    #endregion

    #region Main Methods

    public void Initialize() 
    {
        currentScreen = new SplashScreen();
        fade = new FadeAnimation();
        inputManager = new InputManager();
    }

    public void LoadContent(ContentManager Content)
    {
        content = new ContentManager(Content.ServiceProvider, "Content");
        currentScreen.LoadContent(content, inputManager);

        fadeTexture = content.Load<Texture2D>("GUI/fade");
        fade.LoadContent(content, fadeTexture, "", Vector2.Zero);
        fade.Scale = Math.Max(dimensions.X, dimensions.Y);

        nullImage = content.Load<Texture2D>("GUI/null");
    }

    public void UnloadContent()
    {
    }
    
    public void Update(GameTime gameTime) 
    {
        if (!transition) currentScreen.Update(gameTime);
        else Transition(gameTime);
    }

    public void Draw(SpriteBatch spriteBatch) 
    {
        currentScreen.Draw(spriteBatch);
        if(transition) fade.Draw(spriteBatch);
    }

    #endregion

    #region Special Methods

    public void AddScreen(GameScreen screen, InputManager inputManager)
    {
        transition = true;

        newScreen = screen;
        fade.IsActive = true;
        fade.Alpha = 0.0f;
        fade.ActivateValue = 1.0f;
        this.inputManager = inputManager;
    }

    public void AddScreen(GameScreen screen, InputManager inputManager, float alpha)
    {
        transition = true;

        newScreen = screen;
        fade.IsActive = true;
        fade.Alpha = 0.0f;
        fade.ActivateValue = 1.0f;
        if(alpha != 1.0f) fade.Alpha = 1.0f - alpha;
        else fade.Alpha = alpha;

        this.inputManager = inputManager;

    }

    private void Transition(GameTime gameTime)
    {
        

        fade.Update(gameTime);
        if (fade.Alpha == 1.0f && fade.Timer.TotalSeconds == 1.0f)
        {
            screenStack.Push(newScreen);
            currentScreen.UnloadContent();
            currentScreen = newScreen;
            currentScreen.LoadContent(content, this.inputManager);
        }
        else if (fade.Alpha == 0.0f)
        {
            transition = false;
            fade.IsActive = false;
        }
    }

    #endregion

    #region Properties
    public static ScreenManager Instance
    {
        get
        {
            if (instance == null) instance = new ScreenManager();
            return instance;
        }
    }

    public Vector2 Dimensions
    {
        get { return dimensions; }
        set { dimensions = value; }
    }

    public Texture2D NullImage
    {
        get { return nullImage; }
    }
    #endregion
}