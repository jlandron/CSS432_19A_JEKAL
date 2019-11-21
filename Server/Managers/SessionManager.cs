using System.Collections.Generic;
using System.Linq;

namespace Jekal.Managers
{
    public class SessionManager
    {
        readonly Dictionary<string, int> sessions;
        private int _nextSessionId = 0;

        public SessionManager()
        {
            sessions = new Dictionary<string, int>();
        }

        public int CreateSession(string playerName)
        {
            if (PlayerExists(playerName))
            {
                return -1;
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
            return _nextSessionId++;
        }

        public bool PlayerExists(string playerName)
        {
            return sessions.ContainsKey(playerName);
        }

        public bool ValidateSession(string playerName, int sessionId)
        {
            if (!PlayerExists(playerName))
            {
                return false;
            }

            return sessions[playerName] == sessionId;
        }

        public bool SessionExists(int sessionId)
        {
            var session = sessions.Where(s => s.Value.Equals(sessionId)).ToList();
            return session.Count != 0;
        }
    }
}
