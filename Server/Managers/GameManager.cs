using Jekal.Objects;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace Jekal.Managers
{
    public class GameManager
    {
        private readonly object _gameManagerLock = new object();
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
                _game.GameEndMethod = EndGame;
                _games.Add(_game);
            }
            return _game;
        }

        private Task EndGame(int gameId)
        {
            lock (_gameManagerLock)
            {
                var game = GetGame(gameId);
                _games.Remove(game);
            }
            return Task.FromResult(0);
        }

        public Game GetGame(int id)
        {
            return _games.First(g => g.GameId == id);
        }

        public List<Game> GetAllGames()
        {
            return _games;
        }

        public void AddGame(Task gameTask)
        {
            _gameTasks.Add(gameTask);
        }
    }
}
