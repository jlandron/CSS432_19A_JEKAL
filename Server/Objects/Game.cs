using Jekal.Protocols;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Jekal.Objects
{
    public class Game
    {
        private Timer _gameTimer = null;
        private Timer _updateTimer = null;
        private ScoreBoard _scoreBoard;
        private List<Player> _players;
        private JekalGame _jekal;
        private readonly int _maxPlayers;
        private int _gameTime;
        private bool _gameStarted = false;
        private List<Player> _closedConnections;

        public bool ReadyToStart { get; set; }
        public int GameId { get; set; }


        public Game(JekalGame jekalGame, int id)
        {
            _jekal = jekalGame;
            _players = new List<Player>();
            _closedConnections = new List<Player>();
            _scoreBoard = new ScoreBoard();
            _maxPlayers = Convert.ToInt32(_jekal.Settings["maxPlayersPerGame"]);
            ReadyToStart = false;
            GameId = id;
        }

        public Task Start(CancellationToken token)
        {
            // Sart Game Timers
            _gameTime = Convert.ToInt32(_jekal.Settings["gameTime"]) * 60;  // Game time in seconds
            _gameTimer = new Timer(GameTimeCheck, null, 0, 1000);
            _updateTimer = new Timer(SendUpdate, null, 0, 100); // Update 10 times a second
            _gameStarted = true;

            while (!token.IsCancellationRequested)
            {
                try
                {

                }
                catch (ObjectDisposedException)
                {
                    Console.WriteLine("GAME: Stopping game...");
                    StopGame();
                }
            }

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

        private void SendUpdate(object stateInfo)
        {
            var gameMsg = new GameMessage();
            gameMsg.MessageType = GameMessage.Messages.STATUS;

        }

        private void GameTimeCheck(object stateInfo)
        {
            // Check game time
            if (_gameTime > 0)
            {
                _gameTime -= 1;
                return;
            }

            // Game Time Expired, stop update timer and send one last update
            if (_gameStarted)
            {
                _updateTimer.Dispose();
                _gameTimer.Dispose();
                SendUpdate(null);
            }

            // Send Game Over message
            var byteBuffer = new ByteBuffer();
            byteBuffer.Write((int)GameMessage.Messages.GAMEEND);

            foreach (var p in _players)
            {
                try
                {
                    if (p.IsGameConnected())
                    {
                        if (!p.SendGameMessage(byteBuffer))
                        {
                            CloseConnection(p);
                        }
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine($"GAME: Error communicating with player {p.Name}, closing connection.");
                }
            }
        }

        public void StopGame()
        {
            _gameTime = 0;
            GameTimeCheck(null);
        }

        public void HandleMessage(IAsyncResult ar)
        {
            Player player = (Player)ar.AsyncState;
        }

        private void CloseConnection(Player player)
        {
            Console.WriteLine($"GAME: Error communicating to {player.Name}.  Closing game connection.");
            _players.Remove(player);
            _jekal.Players.RemovePlayer(player);
            _closedConnections.Add(player);
        }
    }
}
