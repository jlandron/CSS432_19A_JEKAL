using Jekal.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Jekal.Servers.GameServerClasses
{
    class GameClient
    {
        public const int BUFFER_SIZE = 4096;

        public int connectionID;
        public TcpClient socket;
        public NetworkStream stream;
        public ByteBuffer buffer;

        private byte[] _recieveBuffer;
        private GameServer _gameServer;

        public GameClient(GameServer gameServer)
        {
            _gameServer = gameServer;
        }
        public void Start()
        {
            _recieveBuffer = new byte[BUFFER_SIZE];
            socket.SendBufferSize = BUFFER_SIZE;
            socket.ReceiveBufferSize = BUFFER_SIZE;
            stream = socket.GetStream();
            stream.BeginRead(_recieveBuffer, 0, BUFFER_SIZE, new AsyncCallback(OnRecieveData), null);
            Console.WriteLine("Incoming Connection from'{0}'.", socket.Client.RemoteEndPoint.ToString());
        }

        private void OnRecieveData(IAsyncResult ar)
        {
            try
            {
                int length = stream.EndRead(ar);
                if (length <= 0)
                {
                    CloseConnection();
                    return;
                }
                byte[] newBytes = new byte[length];
                Array.Copy(_recieveBuffer, newBytes, length);
                //infinite loop
                _gameServer.gameData.HandleData(connectionID, newBytes);
                stream.BeginRead(_recieveBuffer, 0, BUFFER_SIZE, new AsyncCallback(OnRecieveData), null);
            }
            catch (Exception)
            {
                CloseConnection();
                return;
            }
        }

        private void CloseConnection()
        {
            Console.WriteLine("Connection from '{0}' has been terminated.", socket.Client.RemoteEndPoint.ToString());
            _gameServer.clientManager.clients.Remove(connectionID);
            socket.Close();
        }
    }
}
