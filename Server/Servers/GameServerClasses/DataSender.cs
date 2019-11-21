using Jekal.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Threading.Tasks;

namespace Jekal.Servers.GameServerClasses
{
    public enum GameServerPacketsType
    {
        ServerTransformUpdate = 1,
        ServerWelcomeMessage,
        ServerInstatiatePlayerData,

    }
    class DataSender
    {
        
        private GameServer _gameServer;

        public DataSender(GameServer gameServer)
        {
            _gameServer = gameServer;
        }
        public void SendWelcomeMessage(int connectionID)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.Write((int)GameServerPacketsType.ServerWelcomeMessage);
            buffer.Write("Welcome to Jekel Server!");
            buffer.Write(connectionID);
            _gameServer.clientManager.SendData(connectionID, buffer.ToArray());
            buffer.Dispose();
        }
       

        public void BroadcastUpdateTransform(int connectionID, byte[] data)
        {
            foreach (KeyValuePair<int, GameClient> item in GameClientManager.clients)
            {
                if (item.Key != connectionID)
                {
                    _gameServer.clientManager.SendData(item.Key, data);
                }
            }
        }

        public void SendInstatiatePlayerMessage(int index, int connectionID)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.Write((int)GameServerPacketsType.ServerInstatiatePlayerData);
            buffer.Write(index);
            _gameServer.clientManager.SendData(connectionID, buffer.ToArray());
            buffer.Dispose();
        }
    }
}
