using Jekal.Protocols;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Jekal.Servers
{
    public class LoginServer : IServer
    {
        int BUFFER_SIZE = 4096;
        readonly JekalGame _game;
        readonly int nPort = 0;
        List<Task> connections;

        public LoginServer(JekalGame game)
        {
            _game = game;
            nPort = Convert.ToInt32(_game.Settings["loginServerPort"]);
            connections = new List<Task>();
        }

        public async Task<int> StartServer(CancellationToken token)
        {
            string serverName = Dns.GetHostName();
            IPHostEntry hostEntry = Dns.GetHostEntry(serverName);
            IPAddress ipAddress = Array.FindAll(hostEntry.AddressList, a => a.AddressFamily == AddressFamily.InterNetwork)[0];

            Console.WriteLine($"LOGINSERVER: Starting on {ipAddress}:{nPort}");
            TcpListener loginListener = new TcpListener(ipAddress, nPort);
            token.Register(loginListener.Stop);

            loginListener.Start();
            while (!token.IsCancellationRequested)
            {
                try
                {
                    TcpClient playerConnection = await loginListener.AcceptTcpClientAsync();
                    Task playerTask = HandlePlayer(playerConnection);
                    connections.Add(playerTask);
                }
                catch (ObjectDisposedException) when (token.IsCancellationRequested)
                {
                    Console.WriteLine("LOGINSERVER: Stopping Server...");
                    Task.WaitAll(connections.ToArray());
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"LOGINSERVER: Error handling client connetion: {ex.Message}");
                }
            }

            Console.WriteLine("LOGINSERVER: Stopped Server...");
            return 0;
        }

        private Task HandlePlayer(TcpClient playerConnection)
        {
            Console.WriteLine("LOGINSERVER: Incoming Connection");
            NetworkStream netStream = playerConnection.GetStream();

            var login = new LoginMessage();
            byte[] inBuffer;
            inBuffer = new byte[BUFFER_SIZE];

            do
            {
                int bytesRead = netStream.Read(inBuffer, 0, inBuffer.Length);
                byte[] temp = new byte[bytesRead];
                Array.Copy(inBuffer, temp, bytesRead);
                login.Buffer.Write(temp);
            }
            while (netStream.DataAvailable);

            if (login.Parse() && (login.MessageType == LoginMessage.Messages.LOGIN))
            {
                login.Buffer.Clear();
                if (!Authentication(login.Player))
                {
                    Console.WriteLine($"LOGINSERVER: REJECT {login.Player} - Dupe.");
                    login.Buffer.Write((int)LoginMessage.Messages.REJECT);
                    login.Buffer.Write("User name in use.");
                }
                else if (login.Player.Length < 3 || login.Player.Contains(" "))
                {
                    Console.WriteLine($"LOGINSERVER: REJECT {login.Player} - Name too short or spaces in name.");
                    login.Buffer.Write((int)LoginMessage.Messages.REJECT);
                    login.Buffer.Write("Invalid name format, min 3 characters with no spaces");
                }
                else
                {
                    var playerName = login.Player;

                    if (!Authentication(playerName))
                    {
                        Console.WriteLine($"LOGINSERVER: Reject {playerName} - Dupe");
                        login.Buffer.Write((int)LoginMessage.Messages.REJECT);
                        login.Buffer.Write("User name in use.");
                    }
                    else
                    {
                        int sessionID = _game.Players.CreateSession(playerName).SessionID;
                        Console.WriteLine($"LOGINSERVER: AUTH {playerName}; SESSION: {sessionID}");

                        // Player Validated, create an auth message and a session
                        login.Buffer.Write((int)LoginMessage.Messages.AUTH);
                        login.Buffer.Write(_game.GetChatIP());
                        login.Buffer.Write(_game.GetChatPort());
                        login.Buffer.Write(_game.GetGameIP());
                        login.Buffer.Write(_game.GetGamePort());
                        login.Buffer.Write(sessionID);
                    }
                }
                netStream.Write(login.Buffer.ToArray(), 0, login.Buffer.Count());
            }
            else
            {
                Console.WriteLine("LOGINSERVER: Invalid login message received. Closing connection.");
            }

            // Close connection
            netStream.Close();
            playerConnection.Close();
            return Task.FromResult(0);
        }

        private bool Authentication(string playerName)
        {
            // This is where real security would go.
            // We are just checking for unique user names.
            if (_game.Players.PlayerExists(playerName))
            {
                return false;
            }

            return true;
        }
    }
}
