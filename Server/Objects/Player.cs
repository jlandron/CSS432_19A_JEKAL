using System;
using System.Net.Sockets;

namespace Jekal.Objects
{
    public class Player
    {
        const int BUFFER_SIZE = 4096;

        public byte[] ChatBuffer { get; set; }
        public string Name { get; set; }
        public int SessionID { get; set; }
        int score;
        public TcpClient ChatSocket { get; set; }
        public TcpClient GameSocket { get; set; }
        public NetworkStream NetStream { get; set; }

        Socket gameSocket;

        public Player()
        {
            ChatSocket = null;
            GameSocket = null;
            Name = string.Empty;
            SessionID = -1;
            ChatBuffer = new byte[BUFFER_SIZE];
        }
    }
}
