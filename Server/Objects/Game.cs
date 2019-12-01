using Jekal.Protocols;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private List<Team> _teams;
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
            _teams = new List<Team>();
            _closedConnections = new List<Player>();
            _scoreBoard = new ScoreBoard();
            _maxPlayers = Convert.ToInt32(_jekal.Settings["maxPlayersPerGame"]);
            ReadyToStart = false;
            GameId = id;

            int teamCount = Convert.ToInt32(_jekal.Settings["maxTeamsPerGame"]);
            for (int i = 0; i < teamCount; i++)
            {
                _teams.Add(new Team(i));
            }
        }

        public Task Start()
        {
            // Sart Game Timers
            _gameTime = Convert.ToInt32(_jekal.Settings["gameTime"]) * 60;  // Game time in seconds
            _gameTimer = new Timer(GameTimeCheck, null, 0, 1000);
            _updateTimer = new Timer(SendUpdate, null, 0, 100); // Update 10 times a second
            _gameStarted = true;

            while (_gameStarted)
            {

            }

            return Task.FromResult(0);
        }

        public bool HasPlayerSpace()
        {
            return _players.Count < _maxPlayers;
        }

        public bool AddPlayer(Player player)
        {
            // If game full, return false
            if (_players.Count >= _maxPlayers)
            {
                return false;
            }

            // Find team with lowest count
            Team team = null;
            foreach (var t in _teams)
            {
                if (team == null || t.Count() < team.Count())
                {
                    team = t;
                }
            }

            // Assign player game and team ids
            player.TeamID = team.TeamId;
            player.GameID = GameId;

            // Add player to team and game
            team.AddPlayer(player);
            _players.Add(player);

            // Check if game is ready to start
            if (_players.Count == _maxPlayers)
            {
                ReadyToStart = true;
            }

            return true;
        }

        private void SendUpdate(object stateInfo)
        {
            var buffer = new ByteBuffer();
            buffer.Write((int)GameMessage.Messages.STATUS);
            buffer.Write(_gameTime);
            foreach(var p in _players)
            {
                buffer.Write(p.SessionID);
                buffer.Write(p.PosX);
                buffer.Write(p.PosY);
                buffer.Write(p.PosZ);
                buffer.Write(p.RotX);
                buffer.Write(p.RotY);
                buffer.Write(p.RotZ);
                buffer.Write(p.RotW);
            }

            foreach(var p in _players)
            {
                if (!p.SendGameMessage(buffer))
                {
                    CloseConnection(p);
                }
            }
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
            // TODO: What else in GAMEEND?

            foreach (var p in _players)
            {
                try
                {
                    if (!p.SendGameMessage(byteBuffer))
                    {
                        CloseConnection(p);
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine($"GAME: Error communicating with player {p.Name}, closing connection.");
                }
            }

            _gameStarted = false;
        }

        public void StopGame()
        {
            _gameTime = 0;
            GameTimeCheck(null);
        }

        public void HandleMessage(IAsyncResult ar)
        {
            object gameLock = new object();
            var player = (Player)ar.AsyncState;
            lock(gameLock)
            {
                try
                {
                    int length = 0;
                    length = player.EndReadGame(ar);

                    var gameMsg = new GameMessage();
                    byte[] temp = new byte[length];

                    Array.Copy(player.GetGameBuffer(), temp, length);
                    gameMsg.Buffer.Write(temp);

                    // Continue reading if more data
                    while (player.GameHasData())
                    {
                        player.BeginReadGame(new AsyncCallback(HandleMessage));
                    }

                    if (!gameMsg.Parse())
                    {
                        // Invalid Message, drop it
                        return;
                    }

                    // Handle messages from client
                    switch (gameMsg.MessageType)
                    {
                        case GameMessage.Messages.TAG:
                            break;
                        case GameMessage.Messages.UPDATE:
                            UpdatePlayer(gameMsg);
                            break;
                        default:
                            // Drop, invalid message
                            return;
                    }

                    player.BeginReadGame(new AsyncCallback(HandleMessage));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"GAME ERROR: {ex.Message}");
                    CloseConnection(player);
                }
            } // End Lock
        }

        public void UpdatePlayer(GameMessage msg)
        {

        }

        private void CloseConnection(Player player)
        {
            Console.WriteLine($"GAME: Error communicating to {player.Name}.  Closing game connection.");
            _players.Remove(player);
            _jekal.Players.RemovePlayer(player);
            _closedConnections.Add(player);
        }

        public Team GetTeam(int id)
        {
            return _teams.First(t => t.TeamId == id);
        }
    }
}
