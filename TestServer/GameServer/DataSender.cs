using System.Collections.Generic;
using System;

namespace GameServer {
    //!!!make sure to sync with Client code!!!
    public enum ServerPacketsType {
        ServerWelcomeMessage = 1,
        ServerChatBroadcast = 2,
        ServerInstatiatePlayerData = 3,
        ServerTransformUpdate = 4,
    }
    public static class DataSender {
        public static void SendWelcomeMessage( int connectionID ) {
            ByteBuffer buffer = new ByteBuffer( );
            buffer.Write( ( int )ServerPacketsType.ServerWelcomeMessage );
            buffer.Write( "Welcome to Jekel Server!" );
            buffer.Write( connectionID );
            ClientManager.SendData( connectionID, buffer.ToArray( ) );
            buffer.Dispose( );
        }
        //#TODO: implement data pushing from Data reciever to sender
        public static void BroadcastChatMessage( int connectionID ) {
            ByteBuffer buffer = new ByteBuffer( );
            buffer.Write( ( int )ServerPacketsType.ServerChatBroadcast );
            buffer.Write( "Broadcast message from Server!" );
            ClientManager.SendData( connectionID, buffer.ToArray( ) );
            buffer.Dispose( );
        }

        public static void BroadcastUpdateTransform( int connectionID, byte[] data ) {
            foreach( KeyValuePair<int, Client> item in ClientManager.clients ) {
                if( item.Key != connectionID ) {
                    ClientManager.SendData( item.Key, data );
                }
            }
        }

        public static void SendInstatiatePlayerMessage( int index, int connectionID ) {
            ByteBuffer buffer = new ByteBuffer( );
            buffer.Write( ( int )ServerPacketsType.ServerInstatiatePlayerData );
            buffer.Write( index );
            ClientManager.SendData( connectionID, buffer.ToArray( ) );
            buffer.Dispose( );
        }
    }
}

