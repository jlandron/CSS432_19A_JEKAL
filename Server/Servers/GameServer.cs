using Jekal.Servers.GameServerClasses;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Jekal.Servers
{
    internal class GameServer : IServer
    {
        private TcpListener serverSocket;
        private readonly JekalGame _game;
        private readonly IPAddress _ipAddress;
        int nPort = 0;
        public HandleGameData gameData;
        public GameClientManager clientManager;
        public DataSender dataSender;
        public DataReciever dataReciever;

        //TODO: talk to Ed about integrating this code fully into his server architecture
        public async Task<int> StartServer(CancellationToken token)
        {
            Console.WriteLine($"GAMESERVER: Starting on {_ipAddress.ToString()}:{nPort}");
            token.Register(serverSocket.Stop);

            InitGame(); //start allowing clients to connect

            while (!token.IsCancellationRequested)
            {
                try
                {
                    Thread.Sleep(100);
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

        public GameServer(JekalGame game)
        {
            string serverName = Dns.GetHostName();
            IPHostEntry hostEntry = Dns.GetHostEntry(serverName);
            _ipAddress = Array.FindAll(hostEntry.AddressList, a => a.AddressFamily == AddressFamily.InterNetwork)[0];
            _game = game;
            nPort = Convert.ToInt32(_game.Settings["gameServerPort"]);
            serverSocket = new TcpListener(_ipAddress, nPort);
            gameData = new HandleGameData(this); //handles taking in the async mesages and passes them to the correct method in recieve data
            clientManager = new GameClientManager(this); //holds dictionary of each client connection
            dataSender = new DataSender(this); //responsible to sending data to clients
            dataReciever = new DataReciever(this); //responsible to recieving data from clients
        }
        public void InitGame()
        {
            Console.WriteLine("Initilizaing Packets");
            gameData.InitPackets();
            serverSocket.Start();
            serverSocket.BeginAcceptTcpClient(new AsyncCallback(OnClientConnect), null);
        }

        //accept new connections infinitly
        //TODO: find a way to integrate this async method call with Ed's cancellationToken system
        private void OnClientConnect(IAsyncResult ar)
        {
            TcpClient client = serverSocket.EndAcceptTcpClient(ar);
            serverSocket.BeginAcceptTcpClient(new AsyncCallback(OnClientConnect), null);
            clientManager.CreateNewConnection(client);
        }


        public void StopServer()
        {
            throw new System.NotImplementedException();
        }


    }
}
