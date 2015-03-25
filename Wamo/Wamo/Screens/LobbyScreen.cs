using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Lidgren.Network;

public class LobbyScreen : GameScreen
{
    SpriteFont font;
    string text;
    bool gotRole = false;
    float delay = 0.0f;

    public override void LoadContent(ContentManager Content, InputManager inputManager)
    {
        base.LoadContent(Content, inputManager);
        font = content.Load<SpriteFont>("GUI/Fonts/debug");
        text = "";

        NetOutgoingMessage msg = NetworkManager.Instance.CreateMessage();
        msg.Write((byte)PacketTypes.STATEUPDATE);
        msg.Write((byte)State.Lobby);
        NetworkManager.Instance.SendMessage(msg);
    }

    public override void UnloadContent()
    {
        base.UnloadContent();
    }

    public override void Update(GameTime gameTime)
    {
        inputManager.Update();

        if (!NetworkManager.Instance.IsRunning)
            text = "You are not connected to the server... Trying to connect...";
        else if(!gotRole) text = "Waiting for other players to join...";
        if(gotRole)
        {
            if (delay < 5000f) delay += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            else ScreenManager.Instance.AddScreen(new GameplayScreen(), inputManager);
        }
    }

    public override void NetworkMessage(NetIncomingMessage message) 
    {
        message.Position = 0;
        PacketTypes type = (PacketTypes)message.ReadByte();
        if(type == PacketTypes.ROLESELECT)
        {
            State role = (State)message.ReadByte();
            text = "Your new role is " + role.ToString();
            Options.SetValue("role", role);
            gotRole = true;
        }
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        base.Draw(spriteBatch);
        Vector2 pos = (ScreenManager.Instance.BaseDimensions * new Vector2(0.5f, 0.5f)) - font.MeasureString(text) * new Vector2(0.5f, 2.0f);
        spriteBatch.DrawString(font, text, pos, Color.White);
    }
}