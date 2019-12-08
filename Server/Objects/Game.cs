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
        private readonly object _gameLock = new object();

        private Timer _gameTimer = null;
        private Timer _updateTimer = null;
        private List<Team> _teams;
        private JekalGame _jekal;
        private readonly int _maxPlayers;
        private int _gameTime;
        private bool _gameStarted = false;
        private List<Player> _closedConnections;

        public bool ReadyToStart { get; set; }
        public int GameId { get; set; }

        public List<Player> Players { get; }

        public Game(JekalGame jekalGame, int id)
        {
            _jekal = jekalGame;
            Players = new List<Player>();
            _teams = new List<Team>();
            _closedConnections = new List<Player>();
            _maxPlayers = Convert.ToInt32(_jekal.Settings["maxPlayersPerGame"]);
            ReadyToStart = false;
            GameId = id;

            int teamCount = Convert.ToInt32(_jekal.Settings["maxTeamsPerGame"]);
            for (int i = 0; i < teamCount; i++)
            {
                _teams.Add(new Team(i));
            }

            // Start update timer to allow free-roam
            _updateTimer = new Timer(SendUpdate, null, 0, 100); // Update 10 times a second
        }

        public Task Start()
        {
            var byteBuffer = new ByteBuffer();

            // Sart Game Timers
            _gameTime = Convert.ToInt32(_jekal.Settings["gameTime"]) * 60;  // Game time in seconds
            _gameTimer = new Timer(GameTimeCheck, null, 0, 1000);
            _gameStarted = true;

            byteBuffer.Write((int)GameMessage.Messages.GAMESTART);
            byteBuffer.Write(_gameTime);
            SendMessageToGame(byteBuffer);

            // While game has time on it.
            while (_gameStarted)
            {
                lock (_gameLock)
                {
                    // If one team is max or 0, someone has won
                    var team = _teams.Where(t => t.Count() == _maxPlayers);
                    if (team.Count() != 0)
                    {
                        _gameTime = 0;
                    }
                } // End lock

            }

            // Send Game Over message
            byteBuffer.Clear();
            byteBuffer.Write((int)GameMessage.Messages.GAMEEND);
            byteBuffer.Write((GetTeam(0).Count() == 0) ? 0 : 1);
            // TODO: What else in GAMEEND?
            SendMessageToGame(byteBuffer);

            return Task.FromResult(0);
        }

        public void SendMessageToGame(ByteBuffer buffer)
        {
            lock (_gameLock)
            {
                foreach (var p in Players)
                {
                    Console.WriteLine($" *** Sending to {p.Name} : {buffer.ReadInt(false)}");
                    try
                    {
                        if (!p.SendGameMessage(buffer))
                        {
                            CloseConnection(p);
                        }
                    }
                    catch (Exception)
                    {
                        Console.WriteLine($"GAME: Error communicating with player {p.Name}, closing connection.");
                    }
                }
            }  // End lock
        }

        public bool HasPlayerSpace()
        {
            return Players.Count < _maxPlayers;
        }

        public bool AddPlayer(Player player)
        {
            lock (_gameLock)
            {
                // If game full, return false
                if (Players.Count >= _maxPlayers)
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
                Players.Add(player);

                // Check if game is ready to start
                if (Players.Count == _maxPlayers)
                {
                    ReadyToStart = true;
                }
            }
            return true;
        }

        private void SendUpdate(object stateInfo)
        {
            lock (_gameLock)
            {
                var buffer = new ByteBuffer();
                buffer.Write((int)GameMessage.Messages.STATUS);
                buffer.Write(_gameTime);
                buffer.Write(Players.Count);

                foreach (var p in Players)
                {
                    buffer.Write(p.SessionID);
                    buffer.Write(p.PosX);
                    buffer.Write(p.PosY);
                    buffer.Write(p.PosZ);
                    buffer.Write(p.RotX);
                    buffer.Write(p.RotY);
                    buffer.Write(p.RotZ);
                    buffer.Write(p.RotW);
                    buffer.Write(p.Lerp);
                    buffer.Write(p.TeamID);
                    buffer.Write(p.Tags);
                    buffer.Write(p.Tagged);
                }

                foreach (var p in Players)
                {
                    if (!p.SendGameMessage(buffer))
                    {
                        CloseConnection(p);
                    }
                }

                CleanUpConnections();
            }  // End lock
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
                _gameStarted = false;
            }
        }

        public void StopGame()
        {
            _gameTime = 0;
            GameTimeCheck(null);
        }

        public void HandleMessage(IAsyncResult ar)
        {
            var player = (Player)ar.AsyncState;
            lock(_gameLock)
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
                            PlayerTag(gameMsg);
                            break;
                        case GameMessage.Messages.UPDATE:
                            UpdatePlayer(gameMsg);
                            break;
                        case GameMessage.Messages.GAMELEAVE:
                            PlayerLeaving(gameMsg);
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

        private void CleanUpConnections()
        {
            lock (_gameLock)
            {
                foreach (var p in _closedConnections)
                {
                    _jekal.Players.RemovePlayer(p);
                    Players.Remove(p);
                    _teams[p.TeamID].RemovePlayer(p);
                }

                var byteBuffer = new ByteBuffer();
                foreach (var p in _closedConnections)
                {
                    byteBuffer.Write((int)GameMessage.Messages.GAMELEAVE);
                    byteBuffer.Write(p.Name);
                    byteBuffer.Write(p.SessionID);
                    SendMessageToGame(byteBuffer);
                    byteBuffer.Clear();
                }
                byteBuffer.Dispose();
                _closedConnections.Clear();
            }
        }

        private void PlayerLeaving(GameMessage gameMessage)
        {
            var player = _jekal.Players.GetPlayer(gameMessage.SourceId);
            CloseConnection(player);
        }

        private void PlayerTag(GameMessage msg)
        {
            var player = _jekal.Players.GetPlayer(msg.SourceId);
            var target = _jekal.Players.GetPlayer(msg.TargetId);

            var buffer = new ByteBuffer();
            buffer.Write((int)GameMessage.Messages.TEAMSWITCH);
            buffer.Write(target.SessionID);
            buffer.Write(player.Name);
            buffer.Write(player.SessionID);
            buffer.Write(target.TeamID);
            buffer.Write(player.TeamID);

            var oldTeam = GetTeam(target.TeamID);
            var newTeam = GetTeam(player.TeamID);

            lock (_gameLock)
            {
                player.Tags++;
                target.Tagged++;
                oldTeam.RemovePlayer(target);
                target.TeamID = player.TeamID;
                newTeam.AddPlayer(target);
                target.SendGameMessage(buffer);
            } // End lock
        }

        public void UpdatePlayer(GameMessage msg)
        {
            var player = _jekal.Players.GetPlayer(msg.SourceId);
            player.PosX = msg.PosX;
            player.PosY = msg.PosY;
            player.PosZ = msg.PosZ;
            player.RotX = msg.RotX;
            player.RotY = msg.RotY;
            player.RotZ = msg.RotZ;
            player.RotW = msg.RotW;
            player.Lerp = msg.Lerp;
        }

        private void CloseConnection(Player player)
        {
            Console.WriteLine($"GAME: Error communicating to {player.Name}.  Closing game connection.");
            lock (_gameLock)
            {
               _closedConnections.Add(player);
            }
        }

        public Team GetTeam(int id)
        {
            return _teams.First(t => t.TeamId == id);
        }
    }
}
