using Jekal.Objects;
using System.Collections.Generic;

namespace Jekal.Managers
{
    public class GameManager
    {
        private List<Game> Games;
        private Game _game;
        private int currentPort = 4444;
        private string serverIp = "127.0.0.1";

        public int GetGamePort()
        {
            return currentPort;
        }

        public string GetGameIPAddress()
        {
            return serverIp;
        }

        public Game GetWaitingGame()
        {
            return _game;
        }
    }
}
