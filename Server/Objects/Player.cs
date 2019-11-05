using System;
using System.Net.Sockets;

namespace Jekal.Objects
{
    public class Player
    {
        Guid playerId;
        string name;
        int score;
        Socket chatSocket;
        Socket gameSocket;
    }
}
