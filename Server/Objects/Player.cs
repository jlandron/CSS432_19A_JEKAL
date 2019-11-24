using System;
using System.Net.Sockets;

namespace Jekal.Objects
{
    public class Player
    {
        public string Name { get; set; }
        public int SessionID { get; set; }
        int score;
        Socket chatSocket;
        Socket gameSocket;
    }
}
