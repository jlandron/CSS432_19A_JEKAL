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
    internal class GameServer : IServer
    {
        const int BUFFER_SIZE = 4096;
        private readonly JekalGame _jekal;
        private readonly IPAddress _ipAddress;
        int nPort = 0;
        List<Task> connections;

        public GameServer(JekalGame jekal)
        {
            _jekal = jekal;
            connections = new List<Task>();

            nPort = Convert.ToInt32(_jekal.Settings["gameServerPort"]);
            IPHostEntry hostEntry = Dns.GetHostEntry(Dns.GetHostName());
            _ipAddress = Array.FindAll(hostEntry.AddressList, a => a.AddressFamily == AddressFamily.InterNetwork)[0];
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
                    Task.WaitAll(connections.ToArray());
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
                    gameMsg.Buffer.Write((int)GameMessage.Messages.REJECT);
                    gameMsg.Buffer.Write("No session ID.");
                    netStream.Write(gameMsg.Buffer.ToArray(), 0, gameMsg.Buffer.Count());
                    netStream.Close();
                    playerConnection.Close();
                }
                else
                {
                    Console.WriteLine($"GAMESERVER: JOIN {playerName}; SESSION: {sessionId}");
                    var player = _jekal.Players.GetPlayer(playerName);
                    var game = _jekal.Games.GetWaitingGame();
                    player.AssignGameConnection(playerConnection, new AsyncCallback(game.HandleMessage));
                    player.GameID = game.GameId;
                    if (!game.AddPlayer(player))
                    {
                        Console.WriteLine("GAMESERVER: Unable to add player to game.");
                        return Task.FromResult(0);
                    }

                    Console.WriteLine($"GAMESERVER: GAME: {player.GameID}; TEAMJOIN {playerName}; TEAM: {player.TeamID}");
                    var buffer = new ByteBuffer();
                    buffer.Write((int)GameMessage.Messages.TEAMJOIN);
                    buffer.Write(player.Name);
                    buffer.Write(player.TeamID);
                    game.GetTeam(player.TeamID).SendMessage(buffer);

                    if (game.ReadyToStart)
                    {
                        var gameTask = game.Start();
                        _jekal.Games.AddGame(gameTask);
                    }
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
            if (_jekal.Players.ValidateSession(playerName, sessionId))
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
