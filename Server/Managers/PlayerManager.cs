using Jekal.Objects;
using System.Collections.Generic;
using System.Linq;

namespace Jekal.Managers
{
    public class PlayerManager
    {
        private readonly object _playerManagerLock = new object();

        private readonly Dictionary<int, Player> _players;
        private readonly List<Player> _closedConnections;
        private int _nextSessionId = 0;

        public PlayerManager()
        {
            _players = new Dictionary<int, Player>();
            _closedConnections = new List<Player>();
        }

        public Player GetPlayer(string playerName)
        {
            Player player = null;

            lock (_playerManagerLock)
            {
                if (!PlayerExists(playerName))
                {
                    return null;
                }

                player = _players.First(p => p.Value.Name.Equals(playerName, System.StringComparison.InvariantCultureIgnoreCase)).Value;
            }

            return player;
        }

        public Player GetPlayer(int playerId)
        {
            Player player = null;

            lock (_playerManagerLock)
            {
                if (_players.ContainsKey(playerId))
                {
                    player = _players[playerId];
                }
            }

            return player;
        }

        public bool PlayerExists(string playerName)
        {
            List<KeyValuePair<int, Player>> players;

            lock (_playerManagerLock)
            {
                players = _players.Where(p => p.Value.Name.Equals(playerName, System.StringComparison.InvariantCultureIgnoreCase)).ToList();
            }

            return players.Count != 0;
        }

        public bool ValidateSession(string playerName, int sessionId)
        {
            if (!PlayerExists(playerName))
            {
                return false;
            }

            Player player = null;

            lock (_playerManagerLock)
            {
                player = _players[sessionId];
            }

            bool valid = false;
            if (player != null)
            {
                valid = player.Name.Equals(playerName, System.StringComparison.InvariantCultureIgnoreCase);
            }

            return valid;
        }

        public bool SessionExists(int sessionId)
        {
            List<KeyValuePair<int, Player>> sessions;

            lock (_playerManagerLock)
            {
                sessions = _players.Where(p => p.Value.Equals(sessionId)).ToList();
            }

            return sessions.Count != 0;
        }

        public Player CreateSession(string playerName)
        {
            if (PlayerExists(playerName))
            {
                return null;
            }

            if (_nextSessionId == int.MaxValue)
            {
                // Reset Session ID and find first one not in use
                _nextSessionId = 0;
                while (SessionExists(_nextSessionId))
                {
                    _nextSessionId++;
                }
            }

            var newPlayer = new Player()
            {
                Name = playerName,
                SessionID = _nextSessionId++
            };

            lock (_playerManagerLock)
            {
                _players.Add(newPlayer.SessionID, newPlayer);
            }

            return newPlayer;
        }

        public Dictionary<int, Player> GetAllPlayers()
        {
            return _players;
        }

        public void RemovePlayer(Player player)
        {
            lock (_playerManagerLock)
            {
                if (!player.ChatEnabled || !player.GameEnabled)
                {
                    _players.Remove(player.SessionID);
                }
            }
        }

        public void CleanupPlayers(object stateinfo)
        {
            lock (_playerManagerLock)
            {
                foreach (var p in _players)
                {
                    if (p.Value.PlayerCheck && (!p.Value.ChatEnabled || !p.Value.GameEnabled))
                    {
                        p.Value.CloseChat();
                        p.Value.CloseGame();
                        _closedConnections.Add(p.Value);
                    }
                }

                foreach (var p in _closedConnections)
                {
                    _players.Remove(p.SessionID);
                }

                _closedConnections.Clear();
            }
        }
    }
}
