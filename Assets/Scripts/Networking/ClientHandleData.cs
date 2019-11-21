using System;
using System.Collections;
using System.Collections.Generic;

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

        public ClientHandleData(ClientTCP clientTCP)
        {
            this.clientTCP = clientTCP;
        }

        public void InitPackets(ClientTypes clientType) {
            //TODO: switch message types!!
            switch (clientType)
            {
                case ClientTypes.LOGIN:
                    packets.Add((int)ServerPacketType.ServerWelcomeMessage, clientTCP.dataReciever.HandleWelcomeMessage);
                    packets.Add((int)ServerPacketType.ServerChatBroadcast, clientTCP.dataReciever.HandleChatMessage);
                    packets.Add((int)ServerPacketType.ServerInstatiatePlayerData, clientTCP.dataReciever.HandleInstatiatePlayer);
                    packets.Add((int)ServerPacketType.ServerTransformUpdate, clientTCP.dataReciever.HandlePlayerTranformMessage);
                    break;
                case ClientTypes.CHAT:
                    packets.Add((int)ServerPacketType.ServerWelcomeMessage, clientTCP.dataReciever.HandleWelcomeMessage);
                    packets.Add((int)ServerPacketType.ServerChatBroadcast, clientTCP.dataReciever.HandleChatMessage);
                    packets.Add((int)ServerPacketType.ServerInstatiatePlayerData, clientTCP.dataReciever.HandleInstatiatePlayer);
                    packets.Add((int)ServerPacketType.ServerTransformUpdate, clientTCP.dataReciever.HandlePlayerTranformMessage);
                    break;
                case ClientTypes.GAME:
                    packets.Add((int)ServerPacketType.ServerWelcomeMessage, clientTCP.dataReciever.HandleWelcomeMessage);
                    packets.Add((int)ServerPacketType.ServerChatBroadcast, clientTCP.dataReciever.HandleChatMessage);
                    packets.Add((int)ServerPacketType.ServerInstatiatePlayerData, clientTCP.dataReciever.HandleInstatiatePlayer);
                    packets.Add((int)ServerPacketType.ServerTransformUpdate, clientTCP.dataReciever.HandlePlayerTranformMessage);
                    break;
                default:
                    Console.Error.WriteLine("Client type does nto exist");
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
