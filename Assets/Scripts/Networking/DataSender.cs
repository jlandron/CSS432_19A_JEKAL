using System.Collections;
using System.Collections.Generic;


namespace GameClient {
    //!!!make sure to sync enum with Server code!!!
    public enum ClientPacketType {
        ClientChatMessage = 1,
        ClientTransformMessage = 4,
    }

    public class DataSender {
        private ClientTCP clientTCP;

        public DataSender(ClientTCP clientTCP)
        {
            this.clientTCP = clientTCP;
        }

        public  void SendChatMessage( ) {
            ByteBuffer buffer = new ByteBuffer( );
            buffer.Write( ( int )ClientPacketType.ClientChatMessage );
            buffer.Write( "Client chat message to sent from client!" );
            clientTCP.SendData( buffer.ToArray( ) );
            buffer.Dispose( );
        }

        public  void SendTransformMessage( byte[] data ) {
            ByteBuffer buffer = new ByteBuffer( );
            buffer.Write( (int)ClientPacketType.ClientTransformMessage );
            buffer.Write( data );
            clientTCP.SendData( buffer.ToArray( ) );
            buffer.Dispose( );
        }
    }
}

