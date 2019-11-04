using Jekal.Servers;
using Jekal.Managers;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Jekal
{
    class JekalGame
    {
        public GameManager Games { get; set; }
        public PlayerManager Players { get; set; }

        private LoginServer loginServer;
        private ChatServer chatServer;

        public void Initialize()
        {
            loginServer = new LoginServer();
            chatServer = new ChatServer();
            Games = new GameManager();
            Players = new PlayerManager();
        }

        public void StartGame()
        {
            _ = loginServer.TestMethod();
            _ = chatServer.TestMethod();
        }

        public void StopGame()
        {
            loginServer.StopServer();
            chatServer.StopServer();
        }
    }
}
