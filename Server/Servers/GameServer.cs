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
        public Task<int> StartServer(CancellationToken token)
        {
            throw new NotImplementedException();
        }
        public GameServer(JekalGame game)
        {
            _game = game;
            nPort = Convert.ToInt32(_game.Settings["gameServerPort"]);
            serverSocket = new TcpListener(IPAddress.Any, nPort);
            gameData = new HandleGameData(this); //handles taking in the async mesages and passes them to the correct method in recieve data
            clientManager = new GameClientManager(this); //holds dictionary of each client connection
            dataSender = new DataSender(this); //responsible to sending data to clients
            dataReciever = new DataReciever(this); //responsible to recieving data from clients
            InitGame(); //start allowing clients to connect
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
