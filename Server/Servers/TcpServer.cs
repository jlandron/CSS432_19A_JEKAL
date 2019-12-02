using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Jekal.Servers
{
    public class TcpServer
    {
        public delegate Task ConnectionHandler(TcpClient newConnection);
        private ConnectionHandler _connectionHandler;

        public delegate void ShutDownHandler(bool error, string message = "");
        public event ShutDownHandler ShutDownEvent;

        private TcpListener _listener;
        private List<Task> _incomingConnections;

        public void Initialize(IPAddress ipAddr, int port)
        {
            _incomingConnections = new List<Task>();
            _listener = new TcpListener(ipAddr, port);
            _connectionHandler = HandleConnection;
        }

        public async Task StartServer(CancellationToken token)
        {
            // Make sure server was initialized
            if (_listener == null)
            {
                throw new ArgumentNullException(
                    nameof(_listener),
                    "Must initialize TcpServer first.");
            }

            // Make sure shutdown event is subscribed
            if (ShutDownEvent == null)
            {
                throw new ArgumentNullException(
                    nameof(ShutDownEvent),
                    "Must subscribe to the TcpServer ShutDownEvent.");
            }

            // Register the token and start the server
            token.Register(_listener.Stop);
            _listener.Start();

            while (!token.IsCancellationRequested)
            {
                try
                {
                    // Accept new connection
                    var newConnection = await _listener.AcceptTcpClientAsync();
                    var connTask = _connectionHandler(newConnection);
                    _incomingConnections.Add(connTask);
                }
                catch (ObjectDisposedException)
                {
                    // Wait for connections to finish
                    Task.WaitAll(_incomingConnections.ToArray());
                }
                catch (Exception ex)
                {
                    // Signal shutdown with error
                    ShutDownEvent.Invoke(true, ex.Message);
                }
            }

            // Signal clean shutdown
            ShutDownEvent.Invoke(false);
        }

        private static Task HandleConnection(TcpClient conn)
        {
            // Default handler
            throw new ArgumentNullException(
                nameof(ConnectionHandler),
                "Must assign a method to ConnectionHandler.");
        }
    }
}
