using System.Net.Sockets;

namespace Jekal.Objects
{
    public class Player
    {
        const int BUFFER_SIZE = 4096;

        public byte[] ChatBuffer { get; set; }
        public string Name { get; set; }
        public int SessionID { get; set; }
        public TcpClient ChatSocket { get; set; }
        public TcpClient GameSocket { get; set; }
        public NetworkStream ChatStream { get; set; }
        public NetworkStream GameStream { get; set; }

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
