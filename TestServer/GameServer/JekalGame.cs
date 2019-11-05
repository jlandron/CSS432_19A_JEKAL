using Jekal.Servers;
using Jekal.Managers;
using System.Collections.Specialized;

namespace Jekal
{
    public class JekalGame
    {
        public GameManager Games { get; set; }
        public PlayerManager Players { get; set; }

        private LoginServer loginServer;
        private ChatServer chatServer;

        public NameValueCollection Settings { get; }

        public JekalGame(NameValueCollection settings)
        {
            Settings = settings;
        }

        public void Initialize()
        {
            loginServer = new LoginServer(this);
            chatServer = new ChatServer(this);
            Games = new GameManager();
            Players = new PlayerManager();
        }

        public void StartGame()
        {
            _ = loginServer.StartServer();
            _ = chatServer.StartServer();
        }

        public void StopGame()
        {
            loginServer.StopServer();
            chatServer.StopServer();
        }
    }
}
