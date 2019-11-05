using Jekal.Objects;
using System;
using System.Threading.Tasks;

namespace Jekal.Servers
{
    class LoginServer : IServer
    {
        bool stopServer = false;

        public async Task<int> StartServer()
        {
            Console.WriteLine("Starting Login Server...");
            return await StartListening();
        }

        private async Task<int> StartListening()
        {
            while (!stopServer)
            {
                Console.WriteLine("LoginServer...");
                await Task.Delay(1000);
            }

            return 0;
        }

        public void StopServer()
        {
            Console.WriteLine("Stopping Login Server...");
            stopServer = true;
        }
    }
}
