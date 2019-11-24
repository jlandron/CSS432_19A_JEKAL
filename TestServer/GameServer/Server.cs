using System;
using System.Net;
using System.Net.Sockets;

namespace GameServer {
    
    static class Server {
        public static int port = 51092;
        public static TcpListener serverSocket = new TcpListener( IPAddress.Any, port );
        public static void StartServer( ) {
            InitNetwork( );
            Console.WriteLine("Server Started");
        }
        public static void InitNetwork( ) {
            Console.WriteLine( "Initilizaing Packets" );
            ServerHandleData.InitPackets( );
            serverSocket.Start( );
            serverSocket.BeginAcceptTcpClient( new AsyncCallback( OnClientConnect ), null );
        }

        //accept new connections infinitly
        private static void OnClientConnect( IAsyncResult ar ) {
            TcpClient client = serverSocket.EndAcceptTcpClient( ar );
            serverSocket.BeginAcceptTcpClient( new AsyncCallback( OnClientConnect ), null );
            ClientManager.CreateNewConnection( client );
        }
    }
    
}
