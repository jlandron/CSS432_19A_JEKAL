using Jekal.Servers;
using System.Collections.Generic;

namespace Jekal.Objects
{
    public class Game
    {
        private ScoreBoard scoreBoard;
        private GameServer gameServer;
        private List<Player> players;
        private JekalGame _jekal;

        public Game(JekalGame jekalGame)
        {
            _jekal = jekalGame;
            gameServer = new GameServer(_jekal);
        }
    }
}
