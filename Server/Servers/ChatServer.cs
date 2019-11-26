using Common.Protocols;
using Jekal.Objects;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Jekal.Servers
{
    public class ChatServer : IServer
    {
        const int BUFFER_SIZE = 4096;
        private readonly JekalGame _game;
        private readonly IPAddress _ipAddress;
        int nPort = 0;
        List<Task> connections;
        List<Player> players;

        public ChatServer(JekalGame game)
        {
            _game = game;
            nPort = Convert.ToInt32(_game.Settings["chatServerPort"]);
            string serverName = Dns.GetHostName();
            IPHostEntry hostEntry = Dns.GetHostEntry(serverName);
            _ipAddress = Array.FindAll(hostEntry.AddressList, a => a.AddressFamily == AddressFamily.InterNetwork)[0];
            connections = new List<Task>();
            players = new List<Player>();
        }

        public int GetPort()
        {
            return nPort;
        }

        public string GetIP()
        {
            return _ipAddress.ToString();
        }

        public void StopServer()
        {
            // TODO: Remove this from classes and interface
            return;
        }

        public async Task<int> StartServer(CancellationToken token)
        {
            string serverName = Dns.GetHostName();
            IPHostEntry hostEntry = Dns.GetHostEntry(serverName);
            IPAddress ipAddress = Array.FindAll(hostEntry.AddressList, a => a.AddressFamily == AddressFamily.InterNetwork)[0];

            Console.WriteLine($"CHATSERVER: Starting on {ipAddress}:{nPort}");
            TcpListener chatListener = new TcpListener(ipAddress, nPort);
            token.Register(chatListener.Stop);

            chatListener.Start();
            while (!token.IsCancellationRequested)
            {
                try
                {
                    TcpClient playerConnection = await chatListener.AcceptTcpClientAsync();
                    Task playerTask = HandlePlayer(playerConnection);
                    connections.Add(playerTask);
                }
                catch (ObjectDisposedException) when (token.IsCancellationRequested)
                {
                    Console.WriteLine("CHATSERVER: Stopping Server...");
                    Task.WaitAll(connections.ToArray());
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"CHATSERVER: Error handling client connetion: {ex.Message}");
                }
            }

            // TODO: Send chat shutdown message to all players here.
            Console.WriteLine("CHATSERVER: Stopped Server...");
            return 0;
        }

        private Task HandlePlayer(TcpClient playerConnection)
        {
            Console.WriteLine("CHATSERVER: Incoming Connection");
            NetworkStream netStream = playerConnection.GetStream();

            var chatMsg = new ChatMessage();
            byte[] inBuffer;
            inBuffer = new byte[BUFFER_SIZE];

            do
            {
                int bytesRead = netStream.Read(inBuffer, 0, inBuffer.Length);
                byte[] temp = new byte[bytesRead];
                Array.Copy(inBuffer, temp, bytesRead);
                chatMsg.Buffer.Write(temp);
            }
            while (netStream.DataAvailable);

            if (chatMsg.Parse() && (chatMsg.MessageType == ChatMessage.Messages.JOIN))
            {
                var playerName = chatMsg.Source;
                var sessionId = chatMsg.SourceId;

                // Clear for reuse
                chatMsg.Buffer.Clear();

                if (!Authentication(playerName, sessionId))
                {
                    Console.WriteLine($"CHATSERVER: Reject {playerName} - No Session");
                    chatMsg.Buffer.Write((int)ChatMessage.Messages.REJECT);
                    chatMsg.Buffer.Write("No session ID.");
                    netStream.Write(chatMsg.Buffer.ToArray(), 0, chatMsg.Buffer.Count());
                    netStream.Close();
                    playerConnection.Close();
                }
                else
                {
                    Console.WriteLine($"CHATSERVER: JOIN {playerName}; SESSION: {sessionId}");
                    var player = _game.Players.GetPlayer(playerName);
                    player.ChatSocket = playerConnection;
                    player.NetStream = player.ChatSocket.GetStream();
                    player.NetStream.BeginRead(player.ChatBuffer, 0, BUFFER_SIZE, new AsyncCallback(HandleMessage), player);
                    players.Add(player);
                    SendSystemMessage($"[{playerName}] has joined the chat.");
                }
            }
            else
            {
                Console.WriteLine("CHATSERVER: Expecting chat JOIN message. Closing connection.");
                netStream.Close();
                playerConnection.Close();
            }
            return Task.FromResult(0);
        }

        private bool Authentication(string playerName, int sessionId)
        {
            if (_game.Players.ValidateSession(playerName, sessionId))
            {
                return true;
            }
            return false;
        }

        private void HandleMessage(IAsyncResult ar)
        {
            var player = (Player)ar.AsyncState;
            var length = player.NetStream.EndRead(ar);
            var chatMsg = new ChatMessage();
            byte[] temp = new byte[length];

            Array.Copy(player.ChatBuffer, temp, length);
            chatMsg.Buffer.Write(temp);

            while (player.NetStream.DataAvailable)
            {
                player.NetStream.BeginRead(player.ChatBuffer, 0, BUFFER_SIZE, new AsyncCallback(HandleMessage), player);
            }

            if (!chatMsg.Parse())
            {
                // Invalid Message, drop it
                return;
            }

            // Handle messages from client
            switch (chatMsg.MessageType)
            {
                case ChatMessage.Messages.LEAVE:
                    PlayerLeaving(chatMsg);
                    break;
                case ChatMessage.Messages.MSG:
                    SendMessage(chatMsg);
                    break;
                case ChatMessage.Messages.PMSG:
                    PrivateMessage(chatMsg);
                    break;
                case ChatMessage.Messages.TMSG:
                    TeamMessage(chatMsg);
                    break;
                default:
                    // Drop, invalid message
                    return;
            }
        }

        private void PlayerLeaving(ChatMessage chatMessage)
        {

        }

        private void SendMessage(ChatMessage chatMessage)
        {
            var byteBuffer = new Objects.ByteBuffer();
            byteBuffer.Write((int)ChatMessage.Messages.MSG);
            byteBuffer.Write(chatMessage.Source);
            byteBuffer.Write(chatMessage.SourceId);
            byteBuffer.Write(Encoding.ASCII.GetBytes(chatMessage.Message));

            foreach (var p in players)
            {
                if (p.ChatSocket.Connected)
                {
                    p.NetStream.Write(byteBuffer.ToArray(), 0, byteBuffer.Count());
                }
            }
        }

        private void PrivateMessage(ChatMessage chatMessage)
        {
            var byteBuffer = new Objects.ByteBuffer();
            byteBuffer.Write((int)ChatMessage.Messages.PMSG);
            byteBuffer.Write(chatMessage.Source);
            byteBuffer.Write(chatMessage.SourceId);
            byteBuffer.Write(chatMessage.Destination);
            byteBuffer.Write(Encoding.ASCII.GetBytes(chatMessage.Message));

            var player = _game.Players.GetPlayer(chatMessage.Destination);
            if (player.ChatSocket.Connected)
            {
                player.NetStream.Write(byteBuffer.ToArray(), 0, byteBuffer.Count());
            }
        }

        private void TeamMessage(ChatMessage chatMessage)
        {

        }

        private void SendSystemMessage(string message)
        {
            var byteBuffer = new Objects.ByteBuffer();
            byteBuffer.Write((int)ChatMessage.Messages.SYSTEM);
            byteBuffer.Write(Encoding.ASCII.GetBytes(message));

            foreach (var p in players)
            {
                if (p.ChatSocket.Connected)
                {
                    p.NetStream.Write(byteBuffer.ToArray(), 0, byteBuffer.Count());
                }
            }
        }
    }
}
