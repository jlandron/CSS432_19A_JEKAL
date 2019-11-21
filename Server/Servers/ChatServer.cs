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
        private readonly JekalGame _game;
        private readonly IPAddress _ipAddress;
        int nPort = 0;
        List<Task> connections;

        public ChatServer(JekalGame game)
        {
            _game = game;
            nPort = Convert.ToInt32(_game.Settings["chatServerPort"]);
            var serverName = Dns.GetHostName();
            var hostEntry = Dns.GetHostEntry(serverName);
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
            //Console.WriteLine("Starting Chat Server...");
            //return await StartListening();
            var serverName = Dns.GetHostName();
            var hostEntry = Dns.GetHostEntry(serverName);
            var ipAddress = Array.FindAll(hostEntry.AddressList, a => a.AddressFamily == AddressFamily.InterNetwork)[0];

            Console.WriteLine($"CHATSERVER: Starting on {ipAddress}:{nPort}");
            var chatListener = new TcpListener(ipAddress, nPort);
            token.Register(chatListener.Stop);

            chatListener.Start();
            while (!token.IsCancellationRequested)
            {
                try
                {
                    var playerConnection = await chatListener.AcceptTcpClientAsync();
                    var playerTask = HandlePlayer(playerConnection);
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

            Console.WriteLine("CHATSERVER: Stopped Server...");
            return 0;
        }

        private Task HandlePlayer(TcpClient playerConnection)
        {
            return Task.FromResult(0);
        }
    }
}
