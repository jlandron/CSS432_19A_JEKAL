using Jekal.Objects;
using System.Collections.Generic;
using System.Linq;

namespace Jekal.Managers
{
    public class PlayerManager
    {
        private readonly Dictionary<int, Player> _players;
        private int _nextSessionId = 0;

        public PlayerManager()
        {
            _players = new Dictionary<int, Player>();
        }

        public void AddPlayer(int sessionId, Player player)
        {
            _players.Add(sessionId, player);
        }

        public Player GetPlayer(string playerName)
        {
            if (!PlayerExists(playerName))
            {
                return null;
            }

            return _players.First(p => p.Value.Name.Equals(playerName, System.StringComparison.InvariantCultureIgnoreCase)).Value;
        }

        public Player GetPlayer(int playerId)
        {
            if (!_players.ContainsKey(playerId))
            {
                return null;
            }

            return _players[playerId];
        }

        public bool PlayerExists(string playerName)
        {
            var players = _players.Where(p => p.Value.Name.Equals(playerName, System.StringComparison.InvariantCultureIgnoreCase)).ToList();
            return players.Count != 0;
        }

        public bool ValidateSession(string playerName, int sessionId)
        {
            if (!PlayerExists(playerName))
            {
                return false;
            }

            return _players[sessionId].Name.Equals(playerName, System.StringComparison.InvariantCultureIgnoreCase);
        }

        public bool SessionExists(int sessionId)
        {
            var session = _players.Where(p => p.Value.Equals(sessionId)).ToList();
            return session.Count != 0;
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
            _players.Add(newPlayer.SessionID, newPlayer);
            return newPlayer;
        }

        public Dictionary<int, Player> GetAllPlayers()
        {
            return _players;
        }

        public void RemovePlayer(Player player)
        {
            if (!player.IsChatConnected() && !player.IsGameConnected())
            {
                _players.Remove(player.SessionID);
            }
        }
    }
}
