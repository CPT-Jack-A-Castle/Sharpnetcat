using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;


namespace Sharpnetcat
{
    public class Client
    {
        private readonly Socket _sock;

        private byte[] _messageType;

        private byte[] _content;

        public event EventHandler Disconnected;

        public event EventHandler<MessageEvent> MessageReceived;

        public event EventHandler PongEvent;

        public event EventHandler<TransfertEvent> TransfertProgress;


        private void Initialize()
        {
            _messageType = new byte[sizeof (ushort)];

            _sock.BeginReceive(_messageType, 0, _messageType.Length, SocketFlags.None, ReceiveDatas, _sock);
        }

        public Client(Socket sock)
        {
            _sock = sock;
            Initialize();
        }

        public Client(IPAddress ip, int port)
        {
            _sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _sock.Connect(new IPEndPoint(ip, port));
            Initialize();
        }


        private void ReceiveDatas(IAsyncResult ar)
        {
            if (!ar.IsCompleted || !_sock.Connected)
                return;
            try
            {
                _sock.EndReceive(ar);
                switch ((Instructions) BitConverter.ToUInt16(_messageType, 0))
                {
                    case Instructions.BasicMessage:
                        var msgSize = new byte[sizeof (long)];
                        _sock.Receive(msgSize, 0, msgSize.Length, SocketFlags.None);
                        _content = new byte[BitConverter.ToInt64(msgSize, 0)];
                        _sock.BeginReceive(_content, 0, _content.Length, SocketFlags.None, ReceiveBasicMessage, _sock);
                        return;
                    case Instructions.Disconnect:
                        Disconnect();
                        return;
                }
                _sock.BeginReceive(_messageType, 0, _messageType.Length, SocketFlags.None, ReceiveDatas, _sock);
            }
            catch (SocketException e)
            {
                if (e.NativeErrorCode != 10054)
                    throw e;
                if (Disconnected != null)
                    Disconnected(this, EventArgs.Empty);
            }
            catch (ObjectDisposedException)
            {
            }
        }
        

        private void ReceiveBasicMessage(IAsyncResult ar)
        {
            if (!ar.IsCompleted)
                return;
            _sock.EndReceive(ar);
            if (MessageReceived != null)
                MessageReceived(this, new MessageEvent(Encoding.UTF8.GetString(_content)));
            _sock.BeginReceive(_messageType, 0, _messageType.Length, SocketFlags.None, ReceiveDatas, _sock);
        }

        private void SendData(byte[] buf)
        {
            AsyncCallback end = delegate(IAsyncResult ar) { _sock.EndSend(ar); };
            _sock.BeginSend(buf, 0, buf.Length, SocketFlags.None, end, _sock);
        }
                

        public void Disconnect()
        {
            if (_sock.Connected)
            {
                _sock.Send(BitConverter.GetBytes((ushort) Instructions.Disconnect));
                _sock.Shutdown(SocketShutdown.Both);
            }
            _sock.Close(1);
            if (Disconnected != null)
                Disconnected(this, EventArgs.Empty);
        }

        public void SendMessage(string msg)
        {
            if (msg == null)
                return;
            SendData(BitConverter.GetBytes((ushort) Instructions.BasicMessage));
            SendData(BitConverter.GetBytes((long) msg.Length));
            SendData(Encoding.UTF8.GetBytes(msg));
        }

    }
}