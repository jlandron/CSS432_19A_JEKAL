using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jekal.Objects;
using System.Threading.Tasks;

namespace Jekal.Servers.GameServerClasses
{
    class HandleGameData
    {
        public static int identifierLength = 4; //size of an Integer

        public delegate void Packet(int connectionID, byte[] data);
        public Dictionary<int, Packet> packets = new Dictionary<int, Packet>();
        private GameServer _gameServer;

        public HandleGameData(GameServer gameServer)
        {
            _gameServer = gameServer;
        }

        //add all ways packets that might be handled here so that the async calls direct to the correct method
        public void InitPackets()
        {
            packets.Add((int)ClientPacketType.ClientTransformMessage, _gameServer.dataReciever.HandleTransformMessage);
        }

        public void HandleData(int connectionID, byte[] data)
        {
            int pLength = 0;
            //if buffer is not made, make one
            if (GameClientManager.clients[connectionID].buffer == null)
            {
                GameClientManager.clients[connectionID].buffer = new ByteBuffer();
            }
            //write passed in data to client connection
            GameClientManager.clients[connectionID].buffer.Write(data);
            //check if empty package
            if (GameClientManager.clients[connectionID].buffer.Count() == 0)
            {
                GameClientManager.clients[connectionID].buffer.Clear();
                return;
            }
            //make sure packet has at least a length
            if (GameClientManager.clients[connectionID].buffer.Length() >= identifierLength)
            {
                pLength = GameClientManager.clients[connectionID].buffer.ReadInt(false);
                //make sure packet exists
                if (pLength <= 0)
                {
                    GameClientManager.clients[connectionID].buffer.Clear();
                    return;
                }
            }
            //now read byte[] data in packet
            while (pLength > 0 & pLength <= GameClientManager.clients[connectionID].buffer.Length() - identifierLength)
            {
                if (pLength <= GameClientManager.clients[connectionID].buffer.Length() - identifierLength)
                {
                    GameClientManager.clients[connectionID].buffer.ReadInt();
                    data = GameClientManager.clients[connectionID].buffer.ReadBytes(pLength);
                    HandleDataPackets(connectionID, data);
                }
                pLength = 0;
                if (GameClientManager.clients[connectionID].buffer.Length() >= identifierLength)
                {
                    pLength = GameClientManager.clients[connectionID].buffer.ReadInt(false);
                    if (pLength <= 0)
                    {
                        GameClientManager.clients[connectionID].buffer.Clear();
                        return;
                    }
                }
            }
            //clear buffer because it is empty
            if (pLength <= 1)
            {
                GameClientManager.clients[connectionID].buffer.Clear();
            }

        }
        //method to handle each packet and dispose of the buffer
        private void HandleDataPackets(int connectionID, byte[] data)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.Write(data);
            int packetID = buffer.ReadInt();
            buffer.Dispose();
            //invoke the execution of the correct method to handle data
            if (packets.TryGetValue(packetID, out Packet packet))
            {
                packet.Invoke(connectionID, data);
            }
        }
    }
}
