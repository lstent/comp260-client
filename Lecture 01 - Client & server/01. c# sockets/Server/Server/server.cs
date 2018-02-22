using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Server
{
    class server
    {
        class program
        {
            static Dictionary<String, Socket> clientDictionary = new Dictionary<String, Socket>();
            static int clientID = 1;
            static bool quit = false;
            static LinkedList<String> incommingMessages = new LinkedList<string>();

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

                int ID = 0;

                while (quit == false)
                {
                    
                    var newClientSocket = s.Accept();
                    String clientName = "client" + ID;
                    Socket serverClient = s.Accept();

                    var myThread = new Thread(clientReceiveThread);
                    myThread.Start(new ReceiveThreadLaunchInfo(ID, newClientSocket));
                    clientDictionary.Add(clientName, serverClient);
                    
                    ID++;
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
                var dungeon = new Dungeon();

                dungeon.Init();

                Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                IPEndPoint ipLocal = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8221);

                s.Bind(ipLocal);
                s.Listen(4);

                Console.WriteLine("Waiting for client ...");

                Socket newConnection = s.Accept();

                var myThread = new Thread(acceptClientThread);
                myThread.Start(s);

                int tick = 0;
                int itemsProcessed = 0;
                while (true)
                {
                    Console.WriteLine(clientDictionary);
                    byte[] buffer = new byte[4096];
                    try
                    {
                        int result = newConnection.Receive(buffer);
                        if (result > 0)
                        {
                            //String recdMsg = encoder.GetString(buffer, 0, result);
                            ASCIIEncoding encoder = new ASCIIEncoding();

                            String msg = encoder.GetString(buffer, 0, result);
                            Console.WriteLine("Received: " + msg);

                            var dungeonResult = dungeon.Process(msg);
                            Console.WriteLine(dungeonResult);

                            //NetworkStream writer = new NetworkStream(newConnection.BeginSend)

                            byte[] sendBuffer = encoder.GetBytes(dungeonResult);

                            int bytesSent = newConnection.Send(sendBuffer);
                            //newConnection.Send(sendBuffer);
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Console.WriteLine(ex);
                    }


                    String labelToPrint = "";
                    lock (incommingMessages)
                    {
                        if (incommingMessages.First != null)
                        {
                            labelToPrint = incommingMessages.First.Value;

                            incommingMessages.RemoveFirst();

                            itemsProcessed++;

                        }
                    }
                        
                    /*if (labelToPrint != "")
                    {
                        Console.WriteLine(tick + ":" + itemsProcessed + " " + labelToPrint);

                        Console.WriteLine(incommingMessages);
                    }*/
                    tick++;

                    Thread.Sleep(1);
                }
            }
        }
    }
}

