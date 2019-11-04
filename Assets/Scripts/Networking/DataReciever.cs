using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameClient {
    //!!!make sure to sync with Server code!!!
    public enum ServerPacketType {
        ServerWelcomeMessage = 1,
        ServerChatBroadcast = 2,
    }
    public static class DataReciever {
        public static void HandleWelcomeMessage( byte[] data ) {
            ByteBuffer buffer = new ByteBuffer( );
            buffer.Write( data );
            int packetID = buffer.ReadInt( );
            string msg = buffer.ReadString( );
            buffer.Dispose( );
            //TODO: push message to chat window
            Debug.Log( msg );
        }
        //implement pushing chat to data sender
        public static void HandleChatMessage(byte[] data ) {
            ByteBuffer buffer = new ByteBuffer( );
            buffer.Write( data );
            int packetID = buffer.ReadInt( );
            string msg = buffer.ReadString( );
            buffer.Dispose( );
            //TODO: push message to chat window
            Debug.Log( msg );
        }
    }
}

