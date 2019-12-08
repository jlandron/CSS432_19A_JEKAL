using System;
using System.Net.Sockets;
using System.Threading;

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
        public float Lerp { get; set; }
        public int Tags { get; set; }
        public int Tagged { get; set; }
        public bool PlayerCheck { get; set; }


        // Chat Networking
        private TcpClient _chatSocket;
        private NetworkStream _chatStream;
        private byte[] _chatBuffer;
        public bool ChatEnabled = false;
        private Timer _pingTimer = null;


        // Game Networking
        private TcpClient _gameSocket;
        private NetworkStream _gameStream;
        private byte[] _gameBuffer;
        public bool GameEnabled = false;

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
            Tags = 0;
            Tagged = 0;
            PlayerCheck = false;
        }

        public void AssignChatConnection(TcpClient connection, AsyncCallback callback)
        {
            _chatSocket = connection;
            _chatStream = connection.GetStream();
            _chatStream.BeginRead(_chatBuffer, 0, BUFFER_SIZE, callback, this);
            ChatEnabled = true;
            _pingTimer = new Timer(SendPing, null, 0, 250); // Ping every 250ms
        }

        public void AssignGameConnection(TcpClient connection, AsyncCallback callback)
        {
            _gameSocket = connection;
            _gameStream = connection.GetStream();
            _gameStream.BeginRead(_gameBuffer, 0, BUFFER_SIZE, callback, this);
            GameEnabled = true;
        }

        private void SendPing(object stateinfo)
        {
            var buffer = new ByteBuffer();
            buffer.Write(255);
            SendChatMessage(buffer);
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
            if (!ChatEnabled)
            {
                return false;
            }

            var success = SendMessage(_chatStream, buffer);
            if (!success)
            {
                Console.WriteLine($"PLAYER {Name}: Error sending chat message, closing connection.");
                CloseChat();
                _pingTimer.Dispose();
            }

            return success;
        }

        public bool SendGameMessage(ByteBuffer buffer)
        {
            if (!GameEnabled)
            {
                return false;
            }

            var success = SendMessage(_gameStream, buffer);
            if (!success)
            {
                Console.WriteLine($"PLAYER {Name}: Error sending game message, closing connection.");
                CloseGame();
            }

            return success;
        }

        private bool SendMessage(NetworkStream stream, ByteBuffer buffer)
        {
            bool success = true;

            try
            {
                stream.Write(buffer.ToArray(), 0, buffer.Count());
                stream.Flush();
            }
            catch (Exception)
            {
                success = false;
            }

            return success;
        }

        public void CloseChat(ByteBuffer buffer = null)
        {
            try
            {
                if (buffer != null)
                {
                    _chatStream.Write(buffer.ToArray(), 0, buffer.Count());
                    _chatStream.Flush();
                }
            }
            catch (Exception)
            {
                Console.WriteLine($"PLAYER {Name} : Error closing chat stream or socket on close request, ignoring...");
            }
            finally
            {
                _chatStream?.Close();
                _chatSocket?.Close();
                PlayerCheck = true;
                ChatEnabled = false;
            }
        }

        public void CloseGame(ByteBuffer buffer = null)
        {
            try
            {
                if (buffer != null)
                {
                    _gameStream.Write(buffer.ToArray(), 0, buffer.Count());
                    _gameStream.Flush();
                }
            }
            catch (Exception)
            {
                Console.WriteLine($"PLAYER {Name} : Error closing game stream or socket on close request, ignoring...");
            }
            finally
            {
                _gameStream?.Close();
                _gameSocket?.Close();
                PlayerCheck = true;
                GameEnabled = false;
            }
        }
    }
}
