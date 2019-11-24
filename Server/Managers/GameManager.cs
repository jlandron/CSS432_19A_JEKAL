using Jekal.Objects;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace Jekal.Managers
{
    public class GameManager
    {
        private List<Game> games;
        private Game _game;
        private readonly JekalGame _jekal;
        private int _currentPort;
        private IPAddress _serverIp;

        public GameManager(JekalGame game)
        {
            _jekal = game;
            _currentPort = Convert.ToInt32(_jekal.Settings["gameServerPort"]);
            string serverName = Dns.GetHostName();
            IPHostEntry hostEntry = Dns.GetHostEntry(serverName);
            _serverIp = Array.FindAll(hostEntry.AddressList, a => a.AddressFamily == AddressFamily.InterNetwork)[0];
            games = new List<Game>();
            games.Add(new Game(_jekal));
        }
        public int GetGamePort()
        {
            return _currentPort;
        }

        public string GetGameIPAddress()
        {
            return _serverIp.ToString();
        }

        public Game GetWaitingGame()
        {
            return _game;
        }
    }
}
