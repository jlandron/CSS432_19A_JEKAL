using System;
using System.Net.Sockets;

namespace GameClient {
    public class ClientTCP {
        private const int BUFFER_SIZE = 4096;
        private TcpClient _clientSocket;
        private NetworkStream _myStream;
        private byte[] _recieveBuffer;

        private string _serverIP;
        private int _serverPort;

        public DataSender dataSender;
        public DataReciever dataReciever;
        public ClientHandleData clientHandleData;
        public ClientTCP(ClientTypes type)
        {
            dataSender = new DataSender(this);
            dataReciever = new DataReciever(this);
            clientHandleData = new ClientHandleData(this);
            clientHandleData.InitPackets(type);
        }

        public void InitNetworking(string serverIP, int serverPort ) {
            _clientSocket = new TcpClient( );
            _clientSocket.ReceiveBufferSize = BUFFER_SIZE;
            _clientSocket.SendBufferSize = BUFFER_SIZE;
            _recieveBuffer = new byte[ BUFFER_SIZE * 2 ];
            _serverIP = serverIP;
            _serverPort = serverPort;
            _clientSocket.BeginConnect( serverIP, serverPort, new System.AsyncCallback( ClientConnectCallback ), _clientSocket );
        }

        private void ClientConnectCallback( IAsyncResult ar ) {
            _clientSocket.EndConnect( ar );
            if( _clientSocket.Connected == false ) {
                return;
            } else {
                _clientSocket.NoDelay = true;
                _myStream = _clientSocket.GetStream( );
                _myStream.BeginRead( _recieveBuffer, 0, BUFFER_SIZE * 2, RecieveCallback, null );
            }
        }

        private void RecieveCallback( IAsyncResult ar ) {
            try {
                int length = _myStream.EndRead( ar );
                if( length <= 0 ) {
                    return;
                }
                byte[] newBytes = new byte[ length ];
                Array.Copy( _recieveBuffer, newBytes, length );
                UnityThread.executeInFixedUpdate( ( ) => {
                    clientHandleData.HandleData( newBytes );
                } );
                _myStream.BeginRead( _recieveBuffer, 0, BUFFER_SIZE * 2, RecieveCallback, null );
            } catch( Exception ) {
                return;
            }
        }

        public void SendData( byte[] data ) {
            ByteBuffer buffer = new ByteBuffer( );
            buffer.Write( ( data.GetUpperBound( 0 ) - data.GetLowerBound( 0 ) ) + 1 );
            buffer.Write( data );
            _myStream.BeginWrite( buffer.ToArray( ), 0, buffer.ToArray( ).Length, null, null );
            buffer.Dispose( );
        }
        public void Disconnect( ) {
            _clientSocket.Close( );
        }
    }

}
