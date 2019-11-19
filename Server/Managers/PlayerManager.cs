using Jekal.Objects;
using System.Collections.Generic;
using System.Net.Sockets;

namespace Jekal.Managers
{
    public class PlayerManager
    {
        private List<Player> Players;

        public PlayerManager()
        {
            Players = new List<Player>();
        }
        public void AddPlayer(Player player)
        {
            Players.Add(player);
        }

        public bool PlayerExists(string playerName)
        {
            return false;
        }

        public static Player CreatePlayer(Socket clientSocket)
        {
            return new Player();
        }
    }
}
