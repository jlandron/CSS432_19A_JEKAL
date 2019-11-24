using System;
using System.Net.Sockets;

namespace Jekal.Objects
{
    public class Player
    {
        public string Name { get; set; }
        public int SessionID { get; set; }
        int score;
        public TcpClient ChatSocket { get; set; }
        public TcpClient GameSocket { get; set; }

        Socket gameSocket;

        public Player()
        {
            ChatSocket = null;
            GameSocket = null;
            Name = string.Empty;
            SessionID = -1;
        }
    }
}
