using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Lidgren.Network;

public class NetworkManager
{

    #region Field Region

    private static NetworkManager instance;

    private NetClient client;
    private System.Timers.Timer update;
    private bool isRunning = false;

    #endregion

    #region Main Methods

    public void Initialize()
    {
        NetPeerConfiguration Config = new NetPeerConfiguration("game");
        client = new NetClient(Config);
        NetOutgoingMessage outmsg = client.CreateMessage();

        client.Start();

        outmsg.Write((byte)PacketTypes.LOGIN);
        outmsg.Write(Options.GetValue<string>("name"));
        client.Connect(Options.GetValue<string>("defaultIP"), 14242, outmsg);


        Console.WriteLine("Client Started");
        update = new System.Timers.Timer(50);

        update.Elapsed += new System.Timers.ElapsedEventHandler(update_Elapsed);
        WaitForStartingInfo();

        update.Start();
    }

    public void Update()
    {
        if (!isRunning) WaitForStartingInfo();
        else
        {
            if (client.ServerConnection == null) isRunning = false;
        }
        //GetInputAndSendItToServer();
    }

    #endregion

    #region Special Methods

    private void update_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
        if (!isRunning) return;
        CheckServerMessages();
    }

    private void WaitForStartingInfo()
    {
        NetIncomingMessage inc;

       // while (!CanStart)
        {
            if ((inc = client.ReadMessage()) != null)
            {
                switch (inc.MessageType)
                {
                    case NetIncomingMessageType.Data:
                        if (inc.ReadByte() == (byte)PacketTypes.WORLDSTATE)
                        {
                            isRunning = true;
                            Console.WriteLine("CONNECTED FUCKER!");
                        }
                        break;

                    default: Console.WriteLine(inc.ReadString() + " Strange message");
                        break;
                }
            }
        }
    }

    private void CheckServerMessages()
    {
        NetIncomingMessage inc;

        while ((inc = client.ReadMessage()) != null)
        {
            NetIncomingMessage ret = inc;
            if (inc.MessageType == NetIncomingMessageType.Data)
            {
                ScreenManager.Instance.CurrentScreen.NetworkMessage(ret);
                PacketTypes type = (PacketTypes)inc.ReadByte();
                if (type == PacketTypes.WORLDSTATE)
                {
                    Console.WriteLine("World State update");
                } else if (type == PacketTypes.ROLESELECT)
                {
                    
                }
            }
        }
    }

    public void SendMessage(NetOutgoingMessage message)
    {
        client.SendMessage(message, NetDeliveryMethod.ReliableOrdered);
    }

    public NetOutgoingMessage CreateMessage()
    {
        return client.CreateMessage();
    }

    #endregion

    #region Properties
    public static NetworkManager Instance
    {
        get
        {
            if (instance == null) instance = new NetworkManager();
            return instance;
        }
    }

    public bool IsRunning
    {
        get { return isRunning; }
    }
    #endregion

    class Character
    {
        public string name;
        public State role;
        public NetConnection connection;

        public Character(string name, State role, NetConnection conn)
        {
            this.name = name;
            this.role = role;
            this.connection = conn;
        }

        public Character()
        {
        }
    }

    public enum PacketTypes
    {
        LOGIN,
        MOVE,
        WORLDSTATE,
        ROLESELECT,
        STATEUPDATE
    }
    public enum State
    {
        None,
        Lobby,
        Waiting,
        Robot,
        System,
        Doctor
    }
}
