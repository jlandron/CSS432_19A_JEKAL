﻿using Jekal;
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
            playerConnection.Close();
            return Task.FromResult(0);
        }

        private async Task<int> StartListening()
        {
            var serverName = Dns.GetHostName();
            var hostEntry = Dns.GetHostEntry(serverName);
            var ipAddress = Array.FindAll(hostEntry.AddressList, a => a.AddressFamily == AddressFamily.InterNetwork)[0];

            Console.WriteLine($"LOGINSERVER: Starting on {ipAddress}:{nPort}");
            var loginListener = new TcpListener(ipAddress, nPort);

            while (!stopServer)
            {
                await Task.Delay(1000);
            }

            loginListener.Stop();
            return 0;
        }

        public void StopServer()
        {
            Console.WriteLine("LOGINSERVER: Stopping Server...");
            stopServer = true;
        }

        private void OnSocketConnect()
        {
            object playerLock = new object();

            lock (playerLock)
            {
                _game.Players.AddPlayer(new Objects.Player());
            }
        }
    }
}
