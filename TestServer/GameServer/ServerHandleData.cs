using System.Collections;
using System.Collections.Generic;

namespace GameServer {
    public static class ServerHandleData{
        public static int identifierLength = 4; //size of an Integer

        public delegate void Packet(int connectionID, byte[] data );
        public static Dictionary<int, Packet> packets = new Dictionary<int, Packet>( );

        //add all ways packets might be handled here so that the async calls direct to the correct method
        public static void InitPackets( ) {
            packets.Add( ( int )ClientPacketType.ClientChatMessage, DataReciever.HandleChatMessage );
            packets.Add( ( int )ClientPacketType.ClientTransformMessage, DataReciever.HandleTransformMessage );
        }

        public static void HandleData(int connectionID, byte[] data ) {
            int pLength = 0;
            //if buffer is not made, make one
            if(ClientManager.clients[connectionID].buffer == null ) {
                ClientManager.clients[connectionID].buffer = new ByteBuffer( );
            }
            //write passed in data to client connection
            ClientManager.clients[connectionID].buffer.Write( data );
            //check if empty package
            if(ClientManager.clients[connectionID].buffer.Count() == 0 ) {
                ClientManager.clients[connectionID].buffer.Clear( );
                return;
            }
            //make sure packet has at least a length
            if(ClientManager.clients[connectionID].buffer.Length() >= identifierLength ) {
                pLength = ClientManager.clients[connectionID].buffer.ReadInt( false );
                //make sure packet exists
                if(pLength <= 0 ) {
                    ClientManager.clients[connectionID].buffer.Clear( );
                    return;
                }
            }
            //now read byte[] data in packet
            while(pLength > 0 & pLength <= ClientManager.clients[connectionID].buffer.Length() - identifierLength ) {
                if(pLength <= ClientManager.clients[connectionID].buffer.Length() - identifierLength ) {
                    ClientManager.clients[connectionID].buffer.ReadInt( );
                    data = ClientManager.clients[connectionID].buffer.ReadBytes( pLength );
                    HandleDataPackets(connectionID,  data );
                }
                pLength = 0;
                if(ClientManager.clients[connectionID].buffer.Length() >= identifierLength ) {
                    pLength = ClientManager.clients[connectionID].buffer.ReadInt( false );
                    if(pLength <= 0 ) {
                        ClientManager.clients[connectionID].buffer.Clear( );
                        return;
                    }
                }
            }
            //clear buffer because it is empty
            if(pLength <= 1 ) {
                ClientManager.clients[connectionID].buffer.Clear( );
            }

        }
        //method to handle each packet and dispose of the buffer
        private static void HandleDataPackets(int connectionID, byte[] data ) {
            ByteBuffer buffer = new ByteBuffer( );
            buffer.Write( data );
            int packetID = buffer.ReadInt( );
            buffer.Dispose( );
            //invoke the execution of the correct method to handle data
            if( packets.TryGetValue( packetID, out Packet packet )){
                packet.Invoke(connectionID, data );
            }
        }
    }
}
