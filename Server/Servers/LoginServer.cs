using Common.Protocols;
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
            var serverName = Dns.GetHostName();
            var hostEntry = Dns.GetHostEntry(serverName);
            var ipAddress = Array.FindAll(hostEntry.AddressList, a => a.AddressFamily == AddressFamily.InterNetwork)[0];

            Console.WriteLine($"LOGINSERVER: Starting on {ipAddress}:{nPort}");
            var loginListener = new TcpListener(ipAddress, nPort);
            token.Register(loginListener.Stop);

            loginListener.Start();
            while (!token.IsCancellationRequested)
            {
                try
                {
                    var playerConnection = await loginListener.AcceptTcpClientAsync();
                    var playerTask = HandlePlayer(playerConnection);
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
            var netStream = playerConnection.GetStream();

            var login = new LoginMessage();
            while (netStream.CanRead)
            {
                byte[] inBuffer;
                inBuffer = new byte[BUFFER_SIZE];

                do
                {
                    int bytesRead = netStream.Read(inBuffer, 0, inBuffer.Length);
                    var temp = new byte[bytesRead];
                    Array.Copy(inBuffer, temp, bytesRead);
                    login.Buffer.Write(temp);
                }
                while (netStream.DataAvailable);
            }

            if (!login.Parse())
            {
                Console.WriteLine("LOGINSERVER: Invalid login message received. Closing connection.");
            }
            else
            {
                var playerName = login.GetPlayerName();

                if (!Authentication(playerName))
                {
                    login.Buffer.Write((int)LoginMessage.Messages.REJECT);
                    login.Buffer.Write("User name in use.");
                }
                else
                {
                    // Player Validated, create an auth message and a session
                    login.Buffer.Write((int)LoginMessage.Messages.AUTH);
                    login.Buffer.Write(_game.Chat.GetIP());
                    login.Buffer.Write(_game.Chat.GetPort());
                    login.Buffer.Write(_game.Games.GetGameIPAddress());
                    login.Buffer.Write(_game.Games.GetGamePort());
                    login.Buffer.Write(_game.Sessions.CreateSession(playerName));
                }

                netStream.Write(login.Buffer.ToArray(), 0, login.Buffer.Count());
            }

            // Close connection
            netStream.Close();
            playerConnection.Close();
            return Task.FromResult(0);
        }

        public void StopServer()
        {
            // TODO: Remove this from class and interface
            return;
        }

        private bool Authentication(string playerName)
        {
            // This is where real security would go.
            // We are just checking for unique user names.
            if (_game.Sessions.PlayerExists(playerName))
            {
                return false;
            }

            return true;
        }
    }
}
