﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;

class Program
{
    static NetServer Server;
    static NetPeerConfiguration Config;

    static void Main(string[] args)
    {
        Config = new NetPeerConfiguration("game");
        Config.Port = 14242;
        Config.MaximumConnections = 20;

        Config.EnableMessageType(NetIncomingMessageType.ConnectionApproval);

        Server = new NetServer(Config);
        Server.Start();
            
        Console.WriteLine("Server Started");

        List<Character> GameWorldState = new List<Character>();

        NetIncomingMessage inc;
        DateTime time = DateTime.Now;
        TimeSpan timetopass = new TimeSpan(0, 0, 0, 5, 0);
        Console.WriteLine("Waiting for new connections and updateing world state to current ones");

        // Main loop
        while (true)
        {
            if ((inc = Server.ReadMessage()) != null)
            {
                switch (inc.MessageType)
                {
                    case NetIncomingMessageType.ConnectionApproval:
                        {
                            if (inc.ReadByte() == (byte)PacketTypes.LOGIN)
                            {
                                Console.WriteLine("Incoming LOGIN");
                                inc.SenderConnection.Approve();

                                Random r = new Random();
                                GameWorldState.Add(new Character(inc.ReadString(), State.Lobby, inc.SenderConnection));

                                NetOutgoingMessage outmsg = Server.CreateMessage();
                                outmsg.Write((byte)PacketTypes.WORLDSTATE);
                                outmsg.Write(GameWorldState.Count);

                                foreach (Character ch in GameWorldState)
                                    outmsg.WriteAllProperties(ch);

                                Server.SendMessage(outmsg, inc.SenderConnection, NetDeliveryMethod.ReliableOrdered, 0);
                                Console.WriteLine("Approved new connection and updated the world status");
                            }

                            break;
                        }

                    case NetIncomingMessageType.Data:
                        {
                            if (inc.ReadByte() == (byte)PacketTypes.MOVE)
                            {
                                foreach (Character ch in GameWorldState)
                                {
                                    if (ch.connection != inc.SenderConnection)
                                        continue;

                                    byte b = inc.ReadByte();

                                    NetOutgoingMessage outmsg = Server.CreateMessage();

                                    outmsg.Write((byte)PacketTypes.WORLDSTATE);

                                    outmsg.Write(GameWorldState.Count);

                                    foreach (Character ch2 in GameWorldState)
                                        outmsg.WriteAllProperties(ch2);

                                    Server.SendMessage(outmsg, Server.Connections, NetDeliveryMethod.ReliableOrdered, 0);
                                    break;
                                }

                            }
                            break;
                        }

                    case NetIncomingMessageType.StatusChanged:
                        {
                            Console.WriteLine(inc.SenderConnection.ToString() + " status changed. " + (NetConnectionStatus)inc.SenderConnection.Status);
                            if (inc.SenderConnection.Status == NetConnectionStatus.Disconnected || inc.SenderConnection.Status == NetConnectionStatus.Disconnecting)
                            {
                                foreach (Character cha in GameWorldState)
                                {
                                    if (cha.connection == inc.SenderConnection)
                                    {
                                        GameWorldState.Remove(cha);
                                        break;
                                    }
                                }
                            }
                            break;
                        }
                    default:
                        // As i statet previously, theres few other kind of messages also, but i dont cover those in this example
                        // Uncommenting next line, informs you, when ever some other kind of message is received
                        //Console.WriteLine("Not Important Message");
                        break;
                }
            }

            // timestep (30ms)
            if ((time + timetopass) < DateTime.Now)
            {
                if (Server.ConnectionsCount != 0)
                {
                    NetOutgoingMessage outmsg = Server.CreateMessage();
                    outmsg.Write((byte)PacketTypes.WORLDSTATE);

                    outmsg.Write(GameWorldState.Count);

                    foreach (Character ch2 in GameWorldState)
                        outmsg.WriteAllProperties(ch2);

                    Server.SendMessage(outmsg, Server.Connections, NetDeliveryMethod.ReliableOrdered, 0);
                }
                time = DateTime.Now;
            }

            //System.Threading.Thread.Sleep(1);
        }
    }

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