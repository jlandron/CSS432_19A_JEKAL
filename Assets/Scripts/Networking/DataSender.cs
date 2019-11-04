using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace GameClient {
    //!!!make sure to sync with Server code!!!
    public enum ClientPacketType {
        ClientChatMessage = 1,
    }
    public static class DataSender {
        public static void SendChatMessage( ) {
            ByteBuffer buffer = new ByteBuffer( );
            buffer.Write( ( int )ClientPacketType.ClientChatMessage );
            buffer.Write( "Client chat message to sent from client!" );
            ClientTCP.SendData( buffer.ToArray( ) );
            buffer.Dispose( );
        }
    }
}

