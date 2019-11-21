using Jekal.Objects;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace Jekal.Servers.GameServerClasses
{
    class GameClientManager
    {
        //
        public Dictionary<int, GameClient> clients = new Dictionary<int, GameClient>();

        private GameServer _gameServer;

        public GameClientManager(GameServer gameServer)
        {
            _gameServer = gameServer;
        }
        public void CreateNewConnection(TcpClient GameClient)
        {
            GameClient newClient = new GameClient(_gameServer);
            newClient.socket = GameClient;
            newClient.connectionID = ((IPEndPoint)GameClient.Client.RemoteEndPoint).Port;
            newClient.Start();
            clients.Add(newClient.connectionID, newClient);
            _gameServer.dataSender.SendWelcomeMessage(newClient.connectionID);
            InstatiatePlayer(newClient.connectionID);
        }

        public void InstatiatePlayer(int newConnectionID)
        {
            foreach (KeyValuePair<int, GameClient> item in clients)
            {
                //no need to send this message to new GameClient
                if (item.Key != newConnectionID)
                {
                    //Send all online clients to new GameClient
                    _gameServer.dataSender.SendInstatiatePlayerMessage(item.Key, newConnectionID);
                    //send the new connection to all online clients
                    _gameServer.dataSender.SendInstatiatePlayerMessage(newConnectionID, item.Key);
                }
            }
        }

        public void SendData(int connectionID, byte[] data)
        {
            ByteBuffer byteBuffer = new ByteBuffer();
            //write length
            byteBuffer.Write((data.GetUpperBound(0) - data.GetLowerBound(0)) + 1);
            //write data to buffer
            byteBuffer.Write(data);
            clients[connectionID].stream.BeginWrite(byteBuffer.ToArray(), 0, byteBuffer.ToArray().Length, null, null);
            byteBuffer.Dispose();
        }
    }
}
