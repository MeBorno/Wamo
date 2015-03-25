using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Lidgren.Network;
using TomShane.Neoforce.Controls;
using System.Media;

public class Wamo : Microsoft.Xna.Framework.Game
{
    GraphicsDeviceManager graphics;
    SpriteBatch spriteBatch;
    public static Manager manager;
    Song music;
    
    public Wamo()
    {
        graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
    }

    protected override void Initialize()
    {
        Options.LoadOptions(Content.RootDirectory+"\\options.cme");
        NetworkManager.Instance.Initialize();
        ScreenManager.Instance.Initialize(this);
        ScreenManager.Instance.Dimensions = new Vector2(Options.GetValue<int>("screenWidth"), Options.GetValue<int>("screenHeight"));
        graphics.PreferredBackBufferWidth = (int)ScreenManager.Instance.Dimensions.X;
        graphics.PreferredBackBufferHeight = (int)ScreenManager.Instance.Dimensions.Y;
        graphics.ApplyChanges();

        this.IsMouseVisible = true;
     
        base.Initialize();

        manager = new Manager(this, graphics, "Default");
        manager.Initialize();
        
        Window window = new Window(manager);
        window.Init();
        window.Text = "My First Neoforce Window";
        window.Top = 150; // this is in pixels, top-left is the origin
        window.Left = 250;
        window.Width = 350;
        window.Height = 350;
        //manager.Add(window);
        
        music = Content.Load<Song>("Music/BlastProcess");
        MediaPlayer.Play(music);
        MediaPlayer.Volume = Math.Min(1.0f, Options.GetValue<float>("musicVolume"));
        MediaPlayer.IsRepeating = true;                
    }

    protected override void LoadContent()
    {
        spriteBatch = new SpriteBatch(GraphicsDevice);
        ScreenManager.Instance.LoadContent(Content);
        Options.SetValue("GraphicsDevice", GraphicsDevice);
    }

    protected override void UnloadContent()
    {
        ScreenManager.Instance.UnloadContent();
    }

    protected override void Update(GameTime gameTime)
    {
        if (Keyboard.GetState().IsKeyDown(Keys.Escape) || Options.GetValue<bool>("shutDown"))
            this.Exit();

        if (Options.GetValue<bool>("fullScreen") != graphics.IsFullScreen)
        {
            graphics.IsFullScreen = Options.GetValue<bool>("fullScreen");
            graphics.ApplyChanges();
        }

        NetworkManager.Instance.Update();
        ScreenManager.Instance.Update(gameTime);

        base.Update(gameTime);
        manager.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        manager.Draw(gameTime);
        manager.BeginDraw(gameTime);
        GraphicsDevice.Clear(Color.DarkGray);
        ScreenManager.Instance.PreDraw(GraphicsDevice, spriteBatch);
       

        spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, null, null, ScreenManager.Instance.DrawScale());
            ScreenManager.Instance.Draw(spriteBatch);
        spriteBatch.End();

        
        
        base.Draw(gameTime);
        manager.EndDraw();
    }

    class Character
    {
        public int X { get; set; }
        public int Y { get; set; }
        public string Name { get; set; }
        public NetConnection Connection { get; set; }
        public Character(string name, int x, int y, NetConnection conn)
        {
            Name = name;
            X = x;
            Y = y;
            Connection = conn;
        }
        public Character()
        {
        }
    }

    enum PacketTypes
    {
        LOGIN,
        MOVE,
        WORLDSTATE
    }
}
