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
        public ChatServer Chat { get; private set; }

        private LoginServer loginServer;

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
            Chat = new ChatServer(this);
            Games = new GameManager(this);
            Players = new PlayerManager();
        }

        public void StartGame()
        {
            var token = source.Token;
            var loginTask = loginServer.StartServer(token);
            var chatTask = Chat.StartServer(token);

            Task.WaitAll(loginTask, chatTask);
        }

        public void StopGame()
        {
            source.Cancel();
        }
    }
}
