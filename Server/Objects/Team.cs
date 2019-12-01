using System.Collections.Generic;

namespace Jekal.Objects
{
    public class Team
    {
        public int TeamId { get; set; }

        private List<Player> _players;
        

        public Team(int id)
        {
            _players = new List<Player>();
            TeamId = id;
        }

        public int Count()
        {
            return _players.Count;
        }

        public List<Player> GetPlayers()
        {
            return _players;
        }

        public void AddPlayer(Player player)
        {
            _players.Add(player);
        }

        public void SendMessage(ByteBuffer buffer)
        {
            foreach (var p in _players)
            {
                p.SendChatMessage(buffer);
            }

        }
    }
}
