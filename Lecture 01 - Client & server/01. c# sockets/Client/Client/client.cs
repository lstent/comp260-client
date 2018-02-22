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
        static void Main(string[] args)
        {
            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            IPEndPoint ipLocal = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8221);

			bool connected = false;

			while (connected == false) 
			{
				try 
				{
					s.Connect (ipLocal);
					connected = true;
				} 
				catch (Exception) 
				{
					Thread.Sleep (1000);
				}
			}

            int ID = 0;

            while (true)
            {
                String msg = Console.ReadLine();
                String Msg = ID.ToString() + msg;
                ID++;
                ASCIIEncoding encoder = new ASCIIEncoding();
                byte[] buffer = encoder.GetBytes(msg);

                try
                {
                    Console.WriteLine("Writing to server: " + msg);
                    int bytesSent = s.Send(buffer);

                    buffer = new byte[4096];
                    int receiver = s.Receive(buffer);
                    if (receiver > 0)
                    {
                        String carrot = encoder.GetString(buffer, 0, receiver);
                        Console.WriteLine(carrot);
                    }
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine(ex);	
                }
                

                Thread.Sleep(1000);
            }
        }
    }
}
