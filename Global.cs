using System;

namespace Sharpnetcat
{
    public class MessageEvent : EventArgs
    {
        public MessageEvent(string msg)
        {
            Content = msg;
        }

        public string Content { get; private set; }
    }

    public enum Instructions : ushort
    {
        Ping,
        BasicMessage,
        FileTransfert,
        Pong,
        Disconnect
    }

    public class TransfertEvent : EventArgs
    {
        public string Name { get; private set; }

        public double Progress { get; set; }

        public bool Done { get; set; }

        public bool Inbound { get; private set; }
    }
}