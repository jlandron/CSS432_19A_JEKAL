using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Jekal.Servers
{
    public class ChatServer : IServer
    {
        private readonly JekalGame _game;
        private readonly IPAddress _ipAddress;
        bool stopServer = false;
        int nPort = 0;

        public ChatServer(JekalGame game)
        {
            _game = game;
            nPort = Convert.ToInt32(_game.Settings["chatServerPort"]);
            var serverName = Dns.GetHostName();
            var hostEntry = Dns.GetHostEntry(serverName);
            _ipAddress = Array.FindAll(hostEntry.AddressList, a => a.AddressFamily == AddressFamily.InterNetwork)[0];
        }

        public int GetPort()
        {
            return nPort;
        }

        public string GetIP()
        {
            
            return _ipAddress.ToString();
        }

        public void StopServer()
        {
            Console.WriteLine("Stopping Chat Server...");
            stopServer = true;
            return;
        }

        public async Task<int> StartServer(CancellationToken token)
        {
            Console.WriteLine("Starting Chat Server...");
            return await StartListening();
        }

        private async Task<int> StartListening()
        {
            while (!stopServer)
            {
                Console.WriteLine("ChatServer...");
                await Task.Delay(1000);
            }

            return 0;
        }

    }
}
