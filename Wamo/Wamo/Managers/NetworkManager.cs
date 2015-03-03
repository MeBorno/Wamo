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
            if (inc.MessageType == NetIncomingMessageType.Data)
            {
                if (inc.ReadByte() == (byte)PacketTypes.WORLDSTATE)
                {
                    Console.WriteLine("World State update");
                }
            }
        }
    }


    /* Get input from player and send it to server
    private void GetInputAndSendItToServer()
    {

        // Enum object
        MoveDirection MoveDir = new MoveDirection();

        // Default movement is none
        MoveDir = MoveDirection.NONE;

        // Readkey ( NOTE: This normally stops the code flow. Thats why we have timer running, that gets updates)
        // ( Timers run in different threads, so that can be run, even thou we sit here and wait for input )
        ConsoleKeyInfo kinfo = Console.ReadKey();

        // This is wsad controlling system
        if (kinfo.KeyChar == 'w')
            MoveDir = MoveDirection.UP;
        if (kinfo.KeyChar == 's')
            MoveDir = MoveDirection.DOWN;
        if (kinfo.KeyChar == 'a')
            MoveDir = MoveDirection.LEFT;
        if (kinfo.KeyChar == 'd')
            MoveDir = MoveDirection.RIGHT;

        if (kinfo.KeyChar == 'q')
        {

            // Disconnect and give the reason
            client.Disconnect("bye bye");

        }

        // If button was pressed and it was some of those movement keys
        if (MoveDir != MoveDirection.NONE)
        {
            // Create new message
            NetOutgoingMessage outmsg = Client.CreateMessage();

            // Write byte = Set "MOVE" as packet type
            outmsg.Write((byte)PacketTypes.MOVE);

            // Write byte = move direction
            outmsg.Write((byte)MoveDir);

            // Send it to server
            Client.SendMessage(outmsg, NetDeliveryMethod.ReliableOrdered);

            // Reset movedir
            MoveDir = MoveDirection.NONE;
        }

    }*/
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

    enum PacketTypes
    {
        LOGIN,
        MOVE,
        WORLDSTATE
    }
    enum State
    {
        None,
        Lobby,
        Waiting,
        Robot,
        System,
        Doctor
    }
}
