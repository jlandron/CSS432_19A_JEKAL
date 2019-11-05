using System.Collections;
using System.Collections.Generic;
using System;


namespace GameServer {
    //!!!make sure to sync with Client code!!!
    public enum ClientPacketType {
        ClientChatMessage = 1,
        ClientTransformMessage = 4,
    }
    public static class DataReciever {
        public static void HandleChatMessage(int connectionID, byte[] data ) {
            ByteBuffer buffer = new ByteBuffer( );
            buffer.Write( data );
            int packetID = buffer.ReadInt( );
            string msg = buffer.ReadString( );
            buffer.Dispose( );

            Console.Write( msg );
        }
        //TODO: gameUpdateMassage
        public static void HandleTransformMessage( int connectionID, byte[] data ) {
            ByteBuffer buffer = new ByteBuffer( );
            buffer.Write( data );
            buffer.Write( connectionID );
            DataSender.BroadcastUpdateTransform( connectionID , buffer.ToArray());
            buffer.Dispose( );
        }
        //TODO: handle LoginMessage
    }
}

