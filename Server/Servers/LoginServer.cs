using Jekal;
using System;
using System.Threading.Tasks;

namespace Jekal.Servers
{
    public class LoginServer : IServer
    {
        bool stopServer = false;
        readonly JekalGame _game;
        readonly int nPort = 0;

        public LoginServer(JekalGame game)
        {
            _game = game;
            nPort = Convert.ToInt32(_game.Settings["loginServerPort"]);
        }

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

        private void OnSocketConnect()
        {
            object playerLock = new object();

            lock (playerLock)
            {
                _game.Players.AddPlayer(new Objects.Player());
            }
        }
    }
}
