using System.Collections;
using System.Collections.Generic;


namespace GameClient {
    //!!!make sure to sync enum with Server code!!!
    public enum ClientPacketType {
        ClientChatMessage = 1,
        ClientTransformMessage = 4,
    }
    public static class DataSender {
        public static void SendChatMessage( ) {
            ByteBuffer buffer = new ByteBuffer( );
            buffer.Write( ( int )ClientPacketType.ClientChatMessage );
            buffer.Write( "Client chat message to sent from client!" );
            ClientTCP.SendData( buffer.ToArray( ) );
            buffer.Dispose( );
        }
        public static void SendTransformMessage( byte[] data ) {
            ByteBuffer buffer = new ByteBuffer( );
            buffer.Write( (int)ClientPacketType.ClientTransformMessage );
            buffer.Write( data );
            ClientTCP.SendData( buffer.ToArray( ) );
            buffer.Dispose( );
        }
    }
}

