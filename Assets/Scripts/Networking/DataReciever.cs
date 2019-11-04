using UnityEngine;

namespace GameClient {
    //!!!make sure to sync with Server code!!!
    public enum ServerPacketType {
        ServerWelcomeMessage = 1,
        ServerChatBroadcast = 2,
        ServerInstatiatePlayerData = 3,
        ServerTransformUpdate = 4,
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
        }
        public static void HandleInstatiatePlayer(byte[] data ) {
            ByteBuffer buffer = new ByteBuffer( );
            buffer.Write( data );
            int packetID = buffer.ReadInt( );
            int index = buffer.ReadInt( );
            buffer.Dispose( );
            GameObject.FindObjectOfType<NetworkManager>( ).InstatiatePlayer( index );
        }
        public static void HandlePlayerTranformMessage(byte[] data ) {
            GameObject.FindObjectOfType<NetworkManager>( ).UpdatePlayerLocation( data );
        }
    }
}

