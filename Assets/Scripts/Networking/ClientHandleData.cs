using System;
using System.Collections;
using System.Collections.Generic;
using Common.Protocols;

namespace GameClient {
    public enum ClientTypes
    {
        LOGIN = 1,
        CHAT,
        GAME,
    }
    public  class ClientHandleData{
        
        public ByteBuffer playerBuffer;
        public delegate void Packet( byte[] data );
        public Dictionary<int, Packet> packets = new Dictionary<int, Packet>( );
        private ClientTCP clientTCP;
        private ClientTypes myType;

        public ClientHandleData(ClientTCP clientTCP)
        {
            this.clientTCP = clientTCP;
        }

        public void InitPackets(ClientTypes clientType) {
            myType = clientType;
            switch (myType)
            {
                case ClientTypes.LOGIN:
                    packets.Add((int)LoginMessage.Messages.AUTH, clientTCP.dataReciever.HandleAuthMessage);
                    packets.Add((int)LoginMessage.Messages.DOWN, clientTCP.dataReciever.HandleRejectMessage);
                    packets.Add((int)LoginMessage.Messages.REJECT, clientTCP.dataReciever.HandleRejectMessage);
                    break;
                case ClientTypes.CHAT:
                    packets.Add((int)ChatMessage.Messages.JOIN, clientTCP.dataReciever.HandleJoinMessage);
                    packets.Add((int)ChatMessage.Messages.LEAVE, clientTCP.dataReciever.HandleLeaveMessage);
                    packets.Add((int)ChatMessage.Messages.MSG, clientTCP.dataReciever.HandleChatMessage);
                    packets.Add((int)ChatMessage.Messages.PMSG, clientTCP.dataReciever.HandleChatMessage);
                    packets.Add((int)ChatMessage.Messages.REJECT, clientTCP.dataReciever.HandleChatRejectMessage);
                    packets.Add((int)ChatMessage.Messages.SYSTEM, clientTCP.dataReciever.HandleChatMessage);
                    packets.Add((int)ChatMessage.Messages.TMSG, clientTCP.dataReciever.HandleChatMessage);
                    break;
                case ClientTypes.GAME:

                    break;
                default:
                    Console.Error.WriteLine("Client type does not exist");
                    break;
            }
            
        }
        public void HandleData(byte[] data ) {
            int pLength = 0;
            if(playerBuffer == null ) {
                playerBuffer = new ByteBuffer( );
            }

            playerBuffer.Write( data );
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
        private void HandleDataPackets(byte[] data ) {
            ByteBuffer buffer = new ByteBuffer( );
            buffer.Write( data );
            //check packet type
            int packetID = buffer.ReadInt();
            buffer.Dispose( );
            if( packets.TryGetValue( packetID, out Packet packet )){
                packet.Invoke( data );
            }
        }
    }
}
