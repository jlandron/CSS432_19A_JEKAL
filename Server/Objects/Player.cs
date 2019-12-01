using System;
using System.Net.Sockets;

namespace Jekal.Objects
{
    public class Player
    {
        const int BUFFER_SIZE = 4096;

        // Player Data
        public string Name { get; set; }
        public int SessionID { get; set; }
        public int GameID { get; set; }
        public int TeamID { get; set; }
        public float PosX { get; set; }
        public float PosY { get; set; }
        public float PosZ { get; set; }
        public float RotX { get; set; }
        public float RotY { get; set; }
        public float RotZ { get; set; }
        public float RotW { get; set; }


        // Chat Networking
        private TcpClient _chatSocket;
        private NetworkStream _chatStream;
        private byte[] _chatBuffer;

        // Game Networking
        private TcpClient _gameSocket;
        private NetworkStream _gameStream;
        private byte[] _gameBuffer;

        public Player()
        {
            // Init Chat Networking
            _chatSocket = null;
            _chatStream = null;
            _chatBuffer = new byte[BUFFER_SIZE];

            // Init Game Networking
            _gameSocket = null;
            _gameStream = null;
            _gameBuffer = new byte[BUFFER_SIZE];

            // Init Player
            Name = string.Empty;
            SessionID = -1;
            
        }

        public bool IsChatConnected()
        {
            if (_chatSocket == null)
            {
                return false;
            }

            return _chatSocket.Connected;
        }

        public bool IsGameConnected()
        {
            if (_gameSocket == null)
            {
                return false;
            }

            return _gameSocket.Connected;
        }

        public void AssignChatConnection(TcpClient connection, AsyncCallback callback)
        {
            _chatSocket = connection;
            _chatStream = connection.GetStream();
            _chatStream.BeginRead(_chatBuffer, 0, BUFFER_SIZE, callback, this);
        }

        public void AssignGameConnection(TcpClient connection, AsyncCallback callback)
        {
            _gameSocket = connection;
            _gameStream = connection.GetStream();
            _gameStream.BeginRead(_gameBuffer, 0, BUFFER_SIZE, callback, this);
        }

        public int EndReadChat(IAsyncResult ar)
        {
            try
            {
                return _chatStream.EndRead(ar);
            }
            catch(Exception ex)
            {
                throw new Exception($"PLAYER {Name}: Error read chat stream.", ex);
            }
        }

        public int EndReadGame(IAsyncResult ar)
        {
            try
            {
                return _gameStream.EndRead(ar);
            }
            catch (Exception ex)
            {
                throw new Exception($"PLAYER {Name}: Error read game stream.", ex);
            }
        }

        public void BeginReadChat(AsyncCallback callback)
        {
            try
            {
                _chatStream.BeginRead(_chatBuffer, 0, BUFFER_SIZE, callback, this);
            }
            catch (Exception ex)
            {
                throw new Exception($"PLAYER {Name}: Error beginning chat read.", ex);
            }
        }

        public void BeginReadGame(AsyncCallback callback)
        {
            try
            {
                _gameStream.BeginRead(_gameBuffer, 0, BUFFER_SIZE, callback, this);
            }
            catch (Exception ex)
            {
                throw new Exception($"PLAYER {Name}: Error beginning game read.", ex);
            }
        }

        public bool ChatHasData()
        {
            try
            {
                return _chatStream.DataAvailable;
            }
            catch (Exception ex)
            {
                throw new Exception($"PLAYER {Name}: Error checking chat stream for data.", ex);
            }
        }

        public bool GameHasData()
        {
            try
            {
                return _gameStream.DataAvailable;
            }
            catch (Exception ex)
            {
                throw new Exception($"PLAYER {Name}: Error checking game stream for data.", ex);
            }
        }

        public byte[] GetChatBuffer()
        {
            return _chatBuffer;
        }

        public byte[] GetGameBuffer()
        {
            return _gameBuffer;
        }

        public bool SendChatMessage(ByteBuffer buffer)
        {
            var success = SendMessage(_chatSocket, _chatStream, buffer);
            if (!success)
            {
                Console.WriteLine($"PLAYER {Name}: Error sending chat message, closing connection.");
            }

            return success;
        }

        public bool SendGameMessage(ByteBuffer buffer)
        {
            var success = SendMessage(_gameSocket, _gameStream, buffer);
            if (!success)
            {
                Console.WriteLine($"PLAYER {Name}: Error sending game message, closing connection.");
            }

            return success;
        }

        private bool SendMessage(TcpClient client, NetworkStream stream, ByteBuffer buffer)
        {
            try
            {
                if (client == null || !client.Connected)
                {
                    return false;
                }

                if (stream == null || !stream.CanWrite)
                {
                    return false;
                }

                stream.Write(buffer.ToArray(), 0, buffer.Count());
                stream.Flush();
            }
            catch (Exception)
            {
                stream.Close();
                client.Close();
                return false;
            }
            return true;
        }
    }
}
