//Based on the work of https://github.com/Phil-Sparkes/comp260-server-1617/blob/master/MUD2/Server/Server/server.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Server
{


    class program
    {
        static Dungeon dungeon = new Dungeon();
        static Dictionary<String, Socket> clientDictionary = new Dictionary<String, Socket>();
        //static int clientID = 1;
        static bool quit = false;
        public static List<Player> PlayerList = new List<Player>();
        static LinkedList<String> incommingMessages = new LinkedList<string>();
        static LinkedList<String> outgoingMessages = new LinkedList<string>();

        class ReceiveThreadLaunchInfo
        {
            public ReceiveThreadLaunchInfo(int ID, Socket socket)
            {
                this.ID = ID;
                this.socket = socket;
            }

            public int ID;
            public Socket socket;
        }

        static void acceptClientThread(Object obj)
        {
            Socket s = obj as Socket;

            int ID = 1;

            while (quit == false)
            {

                var newClientSocket = s.Accept();

                var myThread = new Thread(clientReceiveThread);
                myThread.Start(new ReceiveThreadLaunchInfo(ID, newClientSocket));

                lock (clientDictionary)
                {
                    String clientName = "client" + ID;
                    clientDictionary.Add(clientName, newClientSocket);
                    var player = new Player
                    {
                        dungeonRef = dungeon,
                        playerName = "Player" + ID,
                        clientName = clientName
                    };

                    player.Init();
                    PlayerList.Add(player);


                    var dungeonResult = dungeon.GiveInfo(player);

                    lock (outgoingMessages)
                    {
                        outgoingMessages.AddLast(clientName + ":" + dungeonResult);
                    }
                }
                ID++;
            }
        }

        static Socket GetSocketFromName(String name)
        {
            lock (clientDictionary)
            {
                return clientDictionary[name];
            }
        }

        static String GetNameFromSocket(Socket s)
        {
            lock (clientDictionary)
            {
                foreach (KeyValuePair<String, Socket> o in clientDictionary)
                {
                    if (o.Value == s)
                    {
                        return o.Key;
                    }
                }
            }
            return null;
        }

        static void localChatMessage(string message, Player player)
        {
            foreach (Player otherPlayer in PlayerList)
            {
                if (player.currentRoom == otherPlayer.currentRoom)
                {
                    lock (outgoingMessages)
                    {
                        outgoingMessages.AddLast(otherPlayer.clientName + ":" + message);
                    }
                }

            }

        }

        static void clientReceiveThread(Object obj)
        {
            ReceiveThreadLaunchInfo receiveInfo = obj as ReceiveThreadLaunchInfo;
            bool socketLost = false;

            while ((quit == false) && (socketLost == false))
            {
                byte[] buffer = new byte[4094];

                try
                {
                    int result = receiveInfo.socket.Receive(buffer);

                    if (result > 0)
                    {
                        ASCIIEncoding encoder = new ASCIIEncoding();

                        lock (incommingMessages)
                        {
                            incommingMessages.AddLast(receiveInfo.ID + ":" + encoder.GetString(buffer, 0, result));
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    socketLost = true;
                }
            }
        }

        static void Main(string[] args)
        {
            ASCIIEncoding encoder = new ASCIIEncoding();

            dungeon.Init();

            Socket serverClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            IPEndPoint ipLocal = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8221);

            serverClient.Bind(ipLocal);
            serverClient.Listen(4);

            Console.WriteLine("Waiting for client ...");

            var myThread = new Thread(acceptClientThread);
            myThread.Start(serverClient);

            byte[] buffer = new byte[4096];
            while (true)
            {
                String messageToSend = "";
                lock (outgoingMessages)
                {
                    if (outgoingMessages.First != null)
                    {
                        messageToSend = outgoingMessages.First.Value;

                        outgoingMessages.RemoveFirst();
                    }
                }
                if (messageToSend != "")
                {
                    try
                    {

                        String[] substrings = messageToSend.Split(':');

                        string theClient = substrings[0];
                        string dungeonResult = substrings[1];

                        byte[] sendBuffer = encoder.GetBytes(dungeonResult); // this is sending back to client
                        int bytesSent = GetSocketFromName(theClient).Send(sendBuffer);

                        bytesSent = GetSocketFromName(theClient).Send(sendBuffer); // DO NOT KNOW WHY I HAVE TO DO THIS TWICE BUT ONLY WAY IT WORKS
                        Console.WriteLine("sending message to " + theClient);
                    }
                    catch
                    {

                    }
                }


                String labelToPrint = "";
                lock (incommingMessages)
                {
                    if (incommingMessages.First != null)
                    {
                        labelToPrint = incommingMessages.First.Value;

                        incommingMessages.RemoveFirst();
                    }
                }

                if (labelToPrint != "")
                {

                    String[] substrings = labelToPrint.Split(':');

                    int PlayerID = Int32.Parse(substrings[0])-1;
                    String UserCmd = substrings[1];

                    var dungeonResult = dungeon.Process(UserCmd, PlayerList[PlayerID]);
                    Player player = PlayerList[PlayerID];
                    String theClient = "client" + substrings[0];


                    if (dungeonResult.Length > 7)
                    {
                        if (dungeonResult.Substring(0, 7) == "[LOCAL]")
                        {
                            localChatMessage(dungeonResult, player);
                        }
                        else
                        {
                            lock (outgoingMessages)
                            {
                                outgoingMessages.AddLast(theClient + ":" + dungeonResult);
                            }
                        }
                    }
                    else
                    {
                        lock (outgoingMessages)
                        {
                            outgoingMessages.AddLast(theClient + ":" + dungeonResult);
                        }
                    }
                }

                Thread.Sleep(1);
            }
        }
    }
}