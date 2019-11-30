using Jekal.Objects;
using Jekal.Servers;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Jekal.Managers
{
    public class GameManager
    {
        private List<Game> games;
        private List<Task> gameTasks;
        private Game _game;
        private readonly JekalGame _jekal;
        private bool _gameAvailable;
        private GameServer _gameServer;

        public GameManager(JekalGame game)
        {
            _jekal = game;
            _gameAvailable = false;
            games = new List<Game>();
            gameTasks = new List<Task>();
            _gameServer = new GameServer(_jekal);
        }
        public int GetGamePort()
        {
            return _gameServer.GetPort();
        }

        public string GetGameIPAddress()
        {
            return _gameServer.GetIP();
        }

        public Game GetWaitingGame()
        {
            return _game;
        }

        public async Task<int> StartServer(CancellationToken token)
        {
            Console.WriteLine($"GAMEMANAGER: Starting game management...");
            
            var gameServerTask = _gameServer.StartServer(token);

            while (!token.IsCancellationRequested)
            {
                try
                {
                    // Check if game is available
                    if (!_gameAvailable)
                    {
                        _game = new Game(_jekal);
                        _gameAvailable = true;
                    }

                    if (_game.ReadyToStart)
                    {
                        var gameTask = _game.Start(token);
                        gameTasks.Add(gameTask);
                        _gameAvailable = false;
                    }

                }
                catch (ObjectDisposedException) when (token.IsCancellationRequested)
                {
                    Console.WriteLine("GAMEMANAGER: Stopping games...");
                    _gameAvailable = false;
                    if (!_game.ReadyToStart)
                    {
                        // TODO: Kill waiting game
                    }

                    Task.WaitAll(gameServerTask);
                    Task.WaitAll(gameTasks.ToArray());
                }
                catch (Exception ex)
                {
                }
            }

            Console.WriteLine("GAMEMANAGER: Stopped game manager...");
            return 0;
        }

        public void AddPlayerToGame(Player player)
        {
            _game.AddPlayer(player);
        }
    }
}
