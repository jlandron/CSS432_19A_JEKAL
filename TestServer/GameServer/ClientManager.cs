using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace GameServer {
    public static class ClientManager {
        //Dictionary of clients
        //TODO: establish connections for chat and game
        public static Dictionary<int, Client> clients = new Dictionary<int, Client>( );

        public static void CreateNewConnection( TcpClient client ) {
            Client newClient = new Client( );
            newClient.socket = client;
            newClient.connectionID = ( ( IPEndPoint )client.Client.RemoteEndPoint ).Port;
            newClient.Start( );
            clients.Add( newClient.connectionID, newClient );
            DataSender.SendWelcomeMessage( newClient.connectionID );
            InstatiatePlayer( newClient.connectionID );
        }

        public static void InstatiatePlayer( int newConnectionID ) {
            foreach( KeyValuePair<int, Client> item in clients ) {
                //no need to send this message to new client
                if( item.Key != newConnectionID ) {
                    //Send all online clients to new client
                    DataSender.SendInstatiatePlayerMessage( item.Key, newConnectionID );
                    //send the new connection to all online clients
                    DataSender.SendInstatiatePlayerMessage( newConnectionID, item.Key );
                }
            }
        }

        public static void SendData( int connectionID, byte[] data ) {
            ByteBuffer byteBuffer = new ByteBuffer( );
            //write length
            byteBuffer.Write( ( data.GetUpperBound( 0 ) - data.GetLowerBound( 0 ) ) + 1 );
            //write data to buffer
            byteBuffer.Write( data );
            clients[ connectionID ].stream.BeginWrite( byteBuffer.ToArray( ), 0, byteBuffer.ToArray( ).Length, null, null );
            byteBuffer.Dispose( );
        }
    }
}
