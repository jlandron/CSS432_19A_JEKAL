using Jekal.Objects;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Jekal.Servers
{
    public class ChatServer : IServer
    {
        private readonly JekalGame _game;
        bool stopServer = false;
        int nPort = 0;
        string ipAddress = "127.0.0.1";

        public ChatServer(JekalGame game)
        {
            _game = game;
            nPort = Convert.ToInt32(_game.Settings["chatServerPort"]);
        }

        public int GetPort()
        {
            return nPort;
        }

        public string GetIP()
        {
            return ipAddress;
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
