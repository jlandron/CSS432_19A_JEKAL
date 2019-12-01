using Jekal.Servers;
using Jekal.Managers;
using System.Collections.Specialized;
using System.Threading;
using System.Threading.Tasks;

namespace Jekal
{
    public class JekalGame
    {
        public GameManager Games { get; set; }
        public PlayerManager Players { get; set; }

        private LoginServer loginServer;
        private ChatServer chatServer;
        private GameServer gameServer;

        private CancellationTokenSource source;

        public NameValueCollection Settings { get; }

        public JekalGame(NameValueCollection settings)
        {
            Settings = settings;
        }

        public void Initialize()
        {
            source = new CancellationTokenSource();
            loginServer = new LoginServer(this);
            chatServer = new ChatServer(this);
            gameServer = new GameServer(this);
            Players = new PlayerManager();
            Games = new GameManager(this);
        }

        public void StartGame()
        {
            var token = source.Token;
            var loginTask = loginServer.StartServer(token);
            var chatTask = chatServer.StartServer(token);
            var gameTask = gameServer.StartServer(token);

            Task.WaitAll(loginTask, chatTask, gameTask);
        }

        public string GetChatIP()
        {
            return chatServer.GetIP();
        }

        public int GetChatPort()
        {
            return chatServer.GetPort();
        }

        public string GetGameIP()
        {
            return gameServer.GetIP();
        }

        public int GetGamePort()
        {
            return gameServer.GetPort();
        }

        public void StopGame()
        {
            source.Cancel();
        }
    }
}
