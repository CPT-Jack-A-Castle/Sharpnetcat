using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;


namespace Sharpnetcat
{
    internal class Server
    {
        public Server(int port)
        {
            _sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _sock.Bind(new IPEndPoint(IPAddress.Any, port));
            _sock.Listen(10);

            _clients = new List<Client>();

            _sock.BeginAccept(ClienConnected, _sock);
        }


        private readonly Socket _sock;
        private readonly List<Client> _clients;


        private void ClienConnected(IAsyncResult ar)
        {
            Console.WriteLine("客户端已连接");
            var c = new Client(_sock.EndAccept(ar));
            c.Disconnected += ClientQuit;
            c.MessageReceived += MessageReceived;
            c.TransfertProgress += FileProgress;
            _clients.Add(c);
            _sock.BeginAccept(ClienConnected, _sock);
        }

        private void ClientQuit(object sender, EventArgs e)
        {
            Console.WriteLine("客户端已断开");
            var c = (Client)sender;
            c.Disconnected -= ClientQuit;
            c.MessageReceived -= MessageReceived;
            c.TransfertProgress -= FileProgress;
            c.Disconnect();
            _clients.Remove(c);
        }

        private void MessageReceived(object sender, MessageEvent msg)
        {
            Console.WriteLine(msg.Content);
        }

        private void FileProgress(object sender, TransfertEvent tr)
        {
            Console.WriteLine("Receving file {0}: {1}%", tr.Name, tr.Progress * 100);
        }


        public void SendMessage(string msg)
        {
            foreach (var client in _clients)
                client.SendMessage(msg);
        }

        public void Disconnect()
        {
            foreach (var client in _clients)
            {
                client.Disconnected -= ClientQuit;
                client.MessageReceived -= MessageReceived;
                client.Disconnect();
            }
        }

    }
}