using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jekal.Objects;
using System.Threading.Tasks;

namespace Jekal.Servers.GameServerClasses
{
    public enum ClientPacketType
    {
        ClientTransformMessage = 1,
        ClientChatMessage,
        
    }
    class DataReciever
    {
        private GameServer _gameServer;

        public DataReciever(GameServer gameServer)
        {
            _gameServer = gameServer;
        }
        public void HandleChatMessage(int connectionID, byte[] data)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.Write(data);
            int packetID = buffer.ReadInt();
            string msg = buffer.ReadString();
            buffer.Dispose();

            Console.Write(msg);
        }
        //TODO: gameUpdateMassage
        public void HandleTransformMessage(int connectionID, byte[] data)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.Write(data);
            _gameServer.dataSender.BroadcastUpdateTransform(connectionID, buffer.ToArray());
            buffer.Dispose();
        }
    }
}
