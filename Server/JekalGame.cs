using Jekal.Servers;
using Jekal.Managers;
using System;
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
            Console.WriteLine("JEKALGAME: Initializing Servers...");
            source = new CancellationTokenSource();
            loginServer = new LoginServer(this);
            chatServer = new ChatServer(this);
            gameServer = new GameServer(this);
            Players = new PlayerManager();
            Games = new GameManager(this);
        }

        public void StartGame()
        {
            Console.WriteLine("JEKALGAME: Starting Jekal servers...");
            var token = source.Token;
            var loginTask = loginServer.StartServer(token);
            var chatTask = chatServer.StartServer(token);
            var gameTask = gameServer.StartServer(token);

            Task.WaitAll(loginTask, chatTask, gameTask);

            Console.WriteLine("JEKALGAME: Stopping games in progress...");

            foreach (var g in Games.GetAllGames())
            {
                g.StopGame();
            }

            Console.WriteLine("JEKALGAME: Cleaning up player list...");
            foreach (var p in Players.GetAllPlayers())
            {
                p.Value.CloseChat(null);
                p.Value.CloseGame();
            }

            Console.WriteLine("JEKALGAME: Game Stopped.");
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
