using Common.Protocols;
using Jekal.Objects;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Jekal.Servers
{
    internal class GameServer : IServer
    {
        const int BUFFER_SIZE = 4096;
        private readonly JekalGame _game;
        private readonly IPAddress _ipAddress;
        int nPort = 0;
        List<Task> connections;
        List<Player> players;

        public GameServer(JekalGame game)
        {
            _game = game;
            nPort = Convert.ToInt32(_game.Settings["gameServerPort"]);
            string serverName = Dns.GetHostName();
            IPHostEntry hostEntry = Dns.GetHostEntry(serverName);
            _ipAddress = Array.FindAll(hostEntry.AddressList, a => a.AddressFamily == AddressFamily.InterNetwork)[0];
            connections = new List<Task>();
            players = new List<Player>();
        }

        public async Task<int> StartServer(CancellationToken token)
        {
            Console.WriteLine($"GAMESERVER: Starting on {_ipAddress.ToString()}:{nPort}");
            TcpListener gameListener = new TcpListener(_ipAddress, nPort);
            token.Register(gameListener.Stop);

            gameListener.Start();

            while (!token.IsCancellationRequested)
            {
                try
                {
                    TcpClient playerConnection = await gameListener.AcceptTcpClientAsync();
                    Task playerTask = HandleConnection(playerConnection);
                    connections.Add(playerTask);
                }
                catch (ObjectDisposedException) when (token.IsCancellationRequested)
                {
                    Console.WriteLine("GAMESERVER: Stopping Server...");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"GAMESERVER: Error handling client connetion: {ex.Message}");
                }
            }

            Console.WriteLine("GAMESERVER: Stopped Server...");
            return 0;
        }

        private Task HandleConnection(TcpClient playerConnection)
        {
            Console.WriteLine("GAMESERVER: Incoming Connection");
            NetworkStream netStream = playerConnection.GetStream();

            var gameMsg = new GameMessage();
            byte[] inBuffer;
            inBuffer = new byte[BUFFER_SIZE];

            do
            {
                int bytesRead = netStream.Read(inBuffer, 0, inBuffer.Length);
                byte[] temp = new byte[bytesRead];
                Array.Copy(inBuffer, temp, bytesRead);
                gameMsg.Buffer.Write(temp);
            }
            while (netStream.DataAvailable);

            if (gameMsg.Parse() && (gameMsg.MessageType == GameMessage.Messages.GAMEJOIN))
            {
                var playerName = gameMsg.Source;
                var sessionId = gameMsg.SourceId;

                // Clear for reuse
                gameMsg.Buffer.Clear();

                if (!Authentication(playerName, sessionId))
                {
                    Console.WriteLine($"GAMESERVER: Reject {playerName} - No Session");
                    gameMsg.Buffer.Write((int)ChatMessage.Messages.REJECT);
                    gameMsg.Buffer.Write("No session ID.");
                    netStream.Write(gameMsg.Buffer.ToArray(), 0, gameMsg.Buffer.Count());
                    netStream.Close();
                    playerConnection.Close();
                }
                else
                {
                    Console.WriteLine($"GAMESERVER: JOIN {playerName}; SESSION: {sessionId}");
                    var player = _game.Players.GetPlayer(playerName);
                    player.GameSocket = playerConnection;
                    player.GameStream = player.GameSocket.GetStream();

                    // TODO: Get available game from GameManager
                    // TODO: Get team from game
                    // TODO: Add player to game and team
                    // TODO: Set listener handler to game handler
                }
            }
            else
            {
                Console.WriteLine("GAMESERVER: Expecting chat GAMEJOIN message. Closing connection.");
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

        public int GetPort()
        {
            return nPort;
        }

        public string GetIP()
        {
            return _ipAddress.ToString();
        }
    }
}
