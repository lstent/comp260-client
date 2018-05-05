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
        static bool firstMessage = true;
        static string loginMessage;
        static LinkedList<String> incommingMessages = new LinkedList<string>();

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

        public static void ClearLine()
        {
            Console.SetCursorPosition(0, Console.CursorTop - 1);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, Console.CursorTop - 1);
        }

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
            

            var myThread = new Thread(ReceiveMessages);
            myThread.Start(s);

            while (true)
            {
                {
                    String Msg = Console.ReadLine();
                    String lowerMsg = Msg.ToLower() + "  ";
                    //Console.WriteLine("/n Type help if your stuck");
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
