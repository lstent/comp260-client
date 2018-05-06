using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Client
{

    class client
    {
        //set variables for the client
        static bool firstMessage = true;
        static string loginMessage;
        static LinkedList<String> incommingMessages = new LinkedList<string>();

        //function create a socket to receive messages from the server
        static void ReceiveMessages(Object obj)
        {
            ASCIIEncoding encoder = new ASCIIEncoding();
            byte[] receiveBuffer = new byte[8192];

            Socket s = obj as Socket;

            while (true)
            {
                try
                {
                    int reciever = s.Receive(receiveBuffer);
                    s.Receive(receiveBuffer);
                    if (reciever > 0)
                    {
                        String userCmd = encoder.GetString(receiveBuffer, 0, reciever);
                        Console.WriteLine(userCmd);
                    }
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }

        //function to clear the console messages (to make it less confusing for the player)
        public static void ClearLine()
        {
            Console.SetCursorPosition(0, Console.CursorTop - 1);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, Console.CursorTop - 1);
        }

        //function to allow the player to create a username or log in using existing username
        static string username()
        {
            String Msg;

            Console.WriteLine("press 1 to create unsername");
            Console.WriteLine("press 2 to login");

            Msg = Console.ReadLine();
            while(firstMessage == true)
            { 
                if (Msg == "1")
                {
                    Console.WriteLine("enter new username");
                    firstMessage = false;
                }
                else if (Msg == "2")
                {
                    Console.WriteLine("enter username");
                    firstMessage = false;
                }
                else
                {
                    Console.WriteLine("Error choice must be 1 or 2");
                }
            }

            Msg += " ";
            Msg += Console.ReadLine();
            Console.WriteLine(Msg);
            return Msg;
        }

        //main loop try to connect to server
        static void Main(string[] args)
        {
            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            IPEndPoint ipLocal = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 3445);

            bool connected = false;

            while (connected == false)
            {
                try
                {
                    s.Connect(ipLocal);
                    connected = true;
                }
                catch (Exception)
                {
                    Thread.Sleep(1000);
                }
            }

            //start the login function to send the chosen username or login username
            ASCIIEncoding encoder = new ASCIIEncoding();
            byte[] buffer = new byte[4096];
            int ID = 0;

            loginMessage = username();
            buffer = encoder.GetBytes(loginMessage);
            try
            {
                int bytesSent = s.Send(buffer);
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex);
            }
            
            // send the messages to navigate through the dungeon
            var myThread = new Thread(ReceiveMessages);
            myThread.Start(s);

            while (true)
            {
                {
                    String Msg = Console.ReadLine();
                    String lowerMsg = Msg.ToLower() + "  ";
                    if (lowerMsg.Substring(0, 2) == "go")
                    {
                        Console.Clear();
                    }
                    else
                    {
                        ClearLine();
                    }

                    ID++;

                    buffer = encoder.GetBytes(Msg);

                    try
                    {
                        int bytesSent = s.Send(buffer);
                    }
                    catch (System.Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }
            }
        }
    }
}
