using System.Collections;
using System.Collections.Generic;


namespace GameClient {
    //!!!make sure to sync enum with Server code!!!
    public enum ClientPacketType {
        ClientChatMessage = 1,
        ClientTransformMessage = 2,
    }
    public static class DataSender {
        public static void SendChatMessage( ) {
            ByteBuffer buffer = new ByteBuffer( );
            buffer.Write( ( int )ClientPacketType.ClientChatMessage );
            buffer.Write( "Client chat message to sent from client!" );
            ClientTCP.SendData( buffer.ToArray( ) );
            buffer.Dispose( );
        }
        public static void SendTransformMessage( float posX, float posY, float posZ , float rotX, float rotY, float rotZ ) {
            ByteBuffer buffer = new ByteBuffer( );
            buffer.Write( (int)ClientPacketType.ClientTransformMessage );
            buffer.Write( posX );
            buffer.Write( posY );
            buffer.Write( posZ );
            buffer.Write( rotX );
            buffer.Write( rotY );
            buffer.Write( rotZ );
            ClientTCP.SendData( buffer.ToArray( ) );
            buffer.Dispose( );
        }
    }
}

