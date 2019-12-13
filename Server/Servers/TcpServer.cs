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
        public delegate Task<int> ConnectionHandlerMethod(TcpClient newConnection);
        public ConnectionHandlerMethod ConnectionHandler;

        public delegate void ShutDownHandler(bool error, string message = "");
        public event ShutDownHandler ShutDownEvent;

        private TcpListener _listener;
        private List<Task> _incomingConnections;
        private string _name = "TcpServer";
        private bool _error = false;
        private string _errMsg = string.Empty;

        public void Initialize(IPAddress ipAddr, int port, string name)
        {
            _incomingConnections = new List<Task>();
            _listener = new TcpListener(ipAddr, port);
            ConnectionHandler = HandleConnection;
            _name = name;
            Console.WriteLine($"{_name}: Server initialized to: {ipAddr}:{port}");
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
            Console.WriteLine($"{_name}: Server started...");

            while (!token.IsCancellationRequested)
            {
                try
                {
                    // Accept new connection
                    var newConnection = await _listener.AcceptTcpClientAsync();
                    var connTask = ConnectionHandler(newConnection);
                    _incomingConnections.Add(connTask);
                }
                catch (ObjectDisposedException)
                {
                    // Wait for connections to finish
                    Console.WriteLine($"{_name}: Server stopping, waiting to process connections...");
                    Task.WaitAll(_incomingConnections.ToArray());
                }
                catch (Exception ex)
                {
                    // Signal shutdown with error
                    _error = true;
                    _errMsg = ex.Message;
                    break;
                }
            }

            // Signal shutdown
            ShutDownEvent.Invoke(_error, _errMsg);
            Console.WriteLine($"{_name}: Server stopped.");
        }

        private static Task<int> HandleConnection(TcpClient conn)
        {
            // Default handler
            throw new ArgumentNullException(
                nameof(ConnectionHandler),
                "Must assign a method to ConnectionHandler.");
        }
    }
}
