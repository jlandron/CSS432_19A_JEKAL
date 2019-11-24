using Common.Protocols;
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

        public ChatServer(JekalGame game)
        {
            _game = game;
            nPort = Convert.ToInt32(_game.Settings["chatServerPort"]);
            string serverName = Dns.GetHostName();
            IPHostEntry hostEntry = Dns.GetHostEntry(serverName);
            _ipAddress = Array.FindAll(hostEntry.AddressList, a => a.AddressFamily == AddressFamily.InterNetwork)[0];
            connections = new List<Task>();
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

        public void SendSystemMessage(string message)
        {

        }
    }
}
