﻿using Jekal.Servers;
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

        public SessionManager Sessions { get; set; }

        public ChatServer Chat
        {
            get
            {
                return chatServer;
            }
        }

        private LoginServer loginServer;
        private ChatServer chatServer;
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
            Games = new GameManager();
            Players = new PlayerManager();
            Sessions = new SessionManager();
        }

        public void StartGame()
        {
            var token = source.Token;
            var loginTask = loginServer.StartServer(token);
            var chatTask = chatServer.StartServer(token);

            Task.WaitAll(loginTask, chatTask);
        }

        public void StopGame()
        {
            source.Cancel();
            //loginServer.StopServer();
            chatServer.StopServer();
        }
    }
}
