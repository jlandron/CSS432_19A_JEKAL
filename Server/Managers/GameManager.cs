using Jekal.Objects;
using System.Collections.Generic;


namespace Jekal.Managers
{
    public class GameManager
    {
        private readonly List<Game> _games;
        private readonly JekalGame _jekal;
        private Game _game;
        private int _gameId = 0;

        public GameManager(JekalGame jekal)
        {
            _jekal = jekal;
            _games = new List<Game>();
        }

        public Game GetWaitingGame()
        {
            if (_game == null || _game.ReadyToStart)
            {
                _game = new Game(_jekal, _gameId++);
                _games.Add(_game);
            }
            return _game;
        }
    }
}
