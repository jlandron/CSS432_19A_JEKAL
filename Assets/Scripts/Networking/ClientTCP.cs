using System;
using System.Net.Sockets;

namespace GameClient {
    public static class ClientTCP {
        private const int BUFFER_SIZE = 4096;
        private static TcpClient _clientSocket;
        private static NetworkStream _myStream;
        private static byte[] _recieveBuffer;

        private static string _serverIP;
        private static int _serverPort;

        public static void InitNetworking(string serverIP, int serverPort ) {
            _clientSocket = new TcpClient( );
            _clientSocket.ReceiveBufferSize = BUFFER_SIZE;
            _clientSocket.SendBufferSize = BUFFER_SIZE;
            _recieveBuffer = new byte[ BUFFER_SIZE * 2 ];
            _serverIP = serverIP;
            _serverPort = serverPort;
            _clientSocket.BeginConnect( serverIP, serverPort, new System.AsyncCallback( ClientConnectCallback ), _clientSocket );
        }

        private static void ClientConnectCallback( IAsyncResult ar ) {
            _clientSocket.EndConnect( ar );
            if( _clientSocket.Connected == false ) {
                return;
            } else {
                _clientSocket.NoDelay = true;
                _myStream = _clientSocket.GetStream( );
                _myStream.BeginRead( _recieveBuffer, 0, BUFFER_SIZE * 2, RecieveCallback, null );
            }
        }

        private static void RecieveCallback( IAsyncResult ar ) {
            try {
                int length = _myStream.EndRead( ar );
                if( length <= 0 ) {
                    return;
                }
                byte[] newBytes = new byte[ length ];
                Array.Copy( _recieveBuffer, newBytes, length );
                UnityThread.executeInFixedUpdate( ( ) => {
                    ClientHandleData.HandleData( newBytes );
                } );
                _myStream.BeginRead( _recieveBuffer, 0, BUFFER_SIZE * 2, RecieveCallback, null );
            } catch( Exception ) {
                return;
            }
        }

        public static void SendData( byte[] data ) {
            ByteBuffer buffer = new ByteBuffer( );
            buffer.Write( ( data.GetUpperBound( 0 ) - data.GetLowerBound( 0 ) ) + 1 );
            buffer.Write( data );
            _myStream.BeginWrite( buffer.ToArray( ), 0, buffer.ToArray( ).Length, null, null );
            buffer.Dispose( );
        }
        public static void Disconnect( ) {
            _clientSocket.Close( );
        }
    }

}
