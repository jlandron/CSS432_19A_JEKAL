using Jekal.Objects;
using System;
using System.Threading.Tasks;

namespace Jekal.Servers
{
    class ChatServer : IServer
    { 
        bool stopServer = false;

        public void StopServer()
        {
            Console.WriteLine("Stopping Chat Server...");
            stopServer = true;
            return;
        }

        public async Task<int> StartServer()
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
