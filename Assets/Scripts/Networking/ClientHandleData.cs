using System.Collections;
using System.Collections.Generic;

namespace GameClient {
    public static class ClientHandleData{

        public static ByteBuffer playerBuffer;
        public delegate void Packet( byte[] data );
        public static Dictionary<int, Packet> packets = new Dictionary<int, Packet>( );

        public static void InitPackets( ) {
            packets.Add( ( int )ServerPacketType.ServerWelcomeMessage , DataReciever.HandleWelcomeMessage );
            packets.Add( ( int )ServerPacketType.ServerChatBroadcast, DataReciever.HandleChatMessage );
            packets.Add( ( int )ServerPacketType.ServerInstatiatePlayerData, DataReciever.HandleInstatiatePlayer );
        }
        public static void HandleData(byte[] data ) {
            byte[] buffer = ( byte[] )data.Clone( );
            int pLength = 0;
            if(playerBuffer == null ) {
                playerBuffer = new ByteBuffer( );
            }

            playerBuffer.Write( buffer );
            if(playerBuffer.Count() == 0 ) {
                playerBuffer.Clear( );
                return;
            }

            if(playerBuffer.Length() >= 4 ) {
                pLength = playerBuffer.ReadInt( false );
                if(pLength <= 0 ) {
                    playerBuffer.Clear( );
                    return;
                }
            }
            while(pLength > 0 & pLength <= playerBuffer.Length() - 4 ) {
                if(pLength <= playerBuffer.Length() - 4 ) {
                    playerBuffer.ReadInt( );
                    data = playerBuffer.ReadBytes( pLength );
                    HandleDataPackets( data );
                }
                pLength = 0;
                if(playerBuffer.Length() >= 4 ) {
                    pLength = playerBuffer.ReadInt( false );
                    if(pLength <= 0 ) {
                        playerBuffer.Clear( );
                        return;
                    }
                }
            }
            //clear buffer because it is empty
            if(pLength <= 1 ) {
                playerBuffer.Clear( );
            }

        }
        private static void HandleDataPackets(byte[] data ) {
            ByteBuffer buffer = new ByteBuffer( );
            buffer.Write( data );
            int packetID = buffer.ReadInt( );
            buffer.Dispose( );
            if( packets.TryGetValue( packetID, out Packet packet )){
                packet.Invoke( data );
            }
        }
    }
}
