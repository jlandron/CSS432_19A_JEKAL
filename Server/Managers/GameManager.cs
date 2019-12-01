using Jekal.Objects;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace Jekal.Managers
{
    public class GameManager
    {
        private readonly List<Game> _games;
        private readonly List<Task> _gameTasks;
        private readonly JekalGame _jekal;
        private Game _game;
        private int _gameId = 0;

        public GameManager(JekalGame jekal)
        {
            _jekal = jekal;
            _games = new List<Game>();
            _gameTasks = new List<Task>();
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

        public Game GetGame(int id)
        {
            return _games.First(g => g.GameId == id);
        }

        public void AddGame(Task gameTask)
        {
            _gameTasks.Add(gameTask);
        }
    }
}
