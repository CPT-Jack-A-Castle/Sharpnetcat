using System;
using System.Linq;
using System.Net;

namespace Sharpnetcat
{
    class Program
    {
        public static int port;
        public static string ip;

        static void Main(string[] args)
        {
            Console.WriteLine(@"                                  _                  _                 
                                 | |                | |                
           __ ___      ____ _  __| | __ _ _ __   ___| |__  _   _ _ __  
          / _` \ \ /\ / / _` |/ _` |/ _` | '_ \ / __| '_ \| | | | '_ \ 
         | (_| |\ V  V / (_| | (_| | (_| | | | | (__| | | | |_| | | | |
          \__, | \_/\_/ \__, |\__,_|\__,_|_| |_|\___|_| |_|\__,_|_| |_|
             | |           | |                                         
             |_|           |_|                                         

          ");
            if (args.Count() > 0)
            {
                if (args[0] == "-p")
                {
                    int port;
                    port = int.Parse(args[1]);
                    var serv = new Server(port);
                    Console.WriteLine("Server已启动.");
                    Console.CancelKeyPress += delegate { serv.Disconnect(); };
                    while (true)
                        serv.SendMessage(Console.ReadLine());
                }
                else if (args[0] == "-n") 
                {
                    ip = args[1];
                    port = int.Parse(args[2]);
                    var client = new Client(IPAddress.Parse(ip), port);
                    Console.CancelKeyPress += delegate { client.Disconnect(); };
                    client.MessageReceived += delegate (object sender, MessageEvent e) { Console.WriteLine(e.Content); };
                    client.Disconnected += delegate
                    {
                        Console.WriteLine("服务器已断开连接");
                        Environment.Exit(0);
                    };
                    while (true)
                        client.SendMessage(Console.ReadLine());
                }                
            }
        }
    }
}
