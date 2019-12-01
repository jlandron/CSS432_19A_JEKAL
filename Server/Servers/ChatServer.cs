using Jekal.Objects;
using Jekal.Protocols;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
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
        List<Player> closedConnections;

        public ChatServer(JekalGame game)
        {
            _game = game;
            nPort = Convert.ToInt32(_game.Settings["chatServerPort"]);
            string serverName = Dns.GetHostName();
            IPHostEntry hostEntry = Dns.GetHostEntry(serverName);
            _ipAddress = Array.FindAll(hostEntry.AddressList, a => a.AddressFamily == AddressFamily.InterNetwork)[0];
            connections = new List<Task>();
            players = new List<Player>();
            closedConnections = new List<Player>();
        }

        public int GetPort()
        {
            return nPort;
        }

        public string GetIP()
        {
            return _ipAddress.ToString();
        }

        public async Task<int> StartServer(CancellationToken token)
        {
            Console.WriteLine($"CHATSERVER: Starting on {_ipAddress.ToString()}:{nPort}");
            TcpListener chatListener = new TcpListener(_ipAddress, nPort);
            token.Register(chatListener.Stop);

            chatListener.Start();
            while (!token.IsCancellationRequested)
            {
                try
                {
                    TcpClient playerConnection = await chatListener.AcceptTcpClientAsync();
                    Task playerTask = HandleConnection(playerConnection);
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

        private Task HandleConnection(TcpClient playerConnection)
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
                    player.AssignChatConnection(playerConnection, new AsyncCallback(HandleMessage));
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
            object chatLock = new object();
            Player player = (Player)ar.AsyncState;

            // Handle one message at a time
            lock (chatLock)
            {
                try
                {
                    int length = 0;
                    length = player.EndReadChat(ar);

                    var chatMsg = new ChatMessage();
                    byte[] temp = new byte[length];

                    Array.Copy(player.GetChatBuffer(), temp, length);
                    chatMsg.Buffer.Write(temp);

                    // Continue reading if more data
                    while (player.ChatHasData())
                    {
                        player.BeginReadChat(new AsyncCallback(HandleMessage));
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
                            StandardMessage(chatMsg);
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

                    player.BeginReadChat(new AsyncCallback(HandleMessage));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"CHATSERVER ERROR: {ex.Message}");
                    CloseConnection(player);
                }
            }
        }

        private void PlayerLeaving(ChatMessage chatMessage)
        {
            SendSystemMessage($"[{chatMessage.Source}] has left the chat.");
            CheckClosedConnections();
        }

        private void StandardMessage(ChatMessage chatMessage)
        {
            var byteBuffer = new Objects.ByteBuffer();
            byteBuffer.Write((int)ChatMessage.Messages.MSG);
            byteBuffer.Write(chatMessage.Source);
            byteBuffer.Write(chatMessage.SourceId);
            byteBuffer.Write(chatMessage.Message);

            foreach (var p in players)
            {
                if (!p.SendChatMessage(byteBuffer))
                {
                    CloseConnection(p);
                }
            }
            byteBuffer.Dispose();
            CheckClosedConnections();
        }

        private void PrivateMessage(ChatMessage chatMessage)
        {
            var byteBuffer = new Objects.ByteBuffer();
            byteBuffer.Write((int)ChatMessage.Messages.PMSG);
            byteBuffer.Write(chatMessage.Source);
            byteBuffer.Write(chatMessage.SourceId);
            byteBuffer.Write(chatMessage.Destination);
            byteBuffer.Write(chatMessage.Message);

            var player = _game.Players.GetPlayer(chatMessage.Destination);
            if (!player.SendChatMessage(byteBuffer))
            {
                CloseConnection(player);
            }
            byteBuffer.Dispose();
            CheckClosedConnections();
        }

        private void TeamMessage(ChatMessage chatMessage)
        {
            CheckClosedConnections();
        }

        private void SendSystemMessage(string message)
        {
            var byteBuffer = new Objects.ByteBuffer();
            byteBuffer.Write((int)ChatMessage.Messages.SYSTEM);
            byteBuffer.Write(message);

            foreach (var p in players)
            {
                if (!p.SendChatMessage(byteBuffer))
                {
                    CloseConnection(p);
                }
            }
            byteBuffer.Dispose();

            CheckClosedConnections();
        }

        private void CheckClosedConnections()
        {
            if (closedConnections.Count > 0)
            {
                foreach (var p in closedConnections)
                {
                    SendSystemMessage($"[{p.Name}] has left chat.");
                }

                closedConnections.Clear();
            }
        }

        private void CloseConnection(Player player)
        {
            Console.WriteLine($"CHATSERVER: Error communicating to {player.Name}.  Closing chat connection.");
            players.Remove(player);
            _game.Players.RemovePlayer(player);
            closedConnections.Add(player);
        }
    }
}
