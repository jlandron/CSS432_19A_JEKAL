using Common.Protocols;
using Jekal.Objects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Jekal.Servers
{
    public class LoginServer : IServer
    {
        bool stopServer = false;
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
            string msg;

            var netStream = playerConnection.GetStream();
            var reader = new StreamReader(netStream);
            msg = reader.ReadToEnd();

            var login = new LoginMessage();
            var valid = login.Parse(msg);

            if (!valid)
            {
                Console.WriteLine("LOGINSERVER: Invalid login message received. Closing connection.");
            }
            else
            {
                var playerName = login.GetPlayerName();

                // TODO: "Auth" player (check for duplicate names)
                if (!Authentication(playerName))
                {
                    login.MessageType = LoginMessage.Messages.REJECT;
                    login.Body = "User name in use.";
                }
                else
                {
                    login.MessageType = LoginMessage.Messages.AUTH;
                    var body = new StringBuilder();
                    body.Append($"{_game.Chat.GetIP()}:{_game.Chat.GetPort()}\r\n");
                    body.Append($"{_game.Games.GetGameIPAddress()}:{_game.Games.GetGamePort()}\r\n");
                    login.Body = body.ToString();

                    // TODO: Create Player object
                    var player = new Player();
                    player.Name = playerName;
                    _game.Players.AddPlayer(player);

                    // TODO: Get Game
                    // TODO: Add Player to it
                }


                // TODO: Respond with AUTH
                var sw = new StreamWriter(netStream);
                //netStream.Write(login.GetByteArray());
                sw.Write(login);

            }

            // Close connection
            playerConnection.Close();
            return Task.FromResult(0);
        }

        public void StopServer()
        {
            Console.WriteLine("LOGINSERVER: Stopping Server...");
            stopServer = true;
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
