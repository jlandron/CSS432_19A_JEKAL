using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Jekal.Objects
{
    public class Game
    {
        private ScoreBoard _scoreBoard;
        private List<Player> _players;
        private JekalGame _jekal;
        private readonly int _maxPlayers;

        public bool ReadyToStart { get; set; }


        public Game(JekalGame jekalGame)
        {
            _jekal = jekalGame;
            _players = new List<Player>();
            _scoreBoard = new ScoreBoard();
            _maxPlayers = Convert.ToInt32(_jekal.Settings["maxPlayersPerGame"]);
            ReadyToStart = false;
        }

        public Task Start(CancellationToken token)
        {
            return Task.FromResult(0);
        }

        public bool HasPlayerSpace()
        {
            return _players.Count < _maxPlayers;
        }

        public void AddPlayer(Player player)
        {
            _players.Add(player);
            if (_players.Count == _maxPlayers)
            {
                ReadyToStart = true;
            }
        }
    }
}
