using Common.Protocols;
using System.Collections.Generic;
using UnityEngine;

namespace NetworkGame.Client
{
    public enum ClientTypes
    {
        LOGIN = 1,
        CHAT,
        GAME,
    }
    public class ClientHandleData
    {

        public ByteBuffer playerBuffer;
        public delegate void Packet(byte[] data);
        public Dictionary<int, Packet> packets;
        private ClientTCP clientTCP;

        public ClientHandleData(ClientTCP clientTCP)
        {
            this.clientTCP = clientTCP;
            packets = new Dictionary<int, Packet>();
        }

        public void InitPackets()
        {

            switch (clientTCP.Type)
            {
                case ClientTypes.LOGIN:
                    packets.Add((int)LoginMessage.Messages.AUTH, clientTCP.dataReciever.HandleAuthMessage);
                    packets.Add((int)LoginMessage.Messages.REJECT, clientTCP.dataReciever.HandleRejectMessage);
                    packets.Add((int)LoginMessage.Messages.DOWN, clientTCP.dataReciever.HandleRejectMessage);
                    Debug.Log("Login packets setup");
                    break;
                case ClientTypes.CHAT:
                    packets.Add((int)ChatMessage.Messages.LEAVE, clientTCP.dataReciever.HandleLeaveMessage);
                    packets.Add((int)ChatMessage.Messages.SYSTEM, clientTCP.dataReciever.HandleSystemChatMessage);
                    packets.Add((int)ChatMessage.Messages.MSG, clientTCP.dataReciever.HandleChatMessage);
                    packets.Add((int)ChatMessage.Messages.PMSG, clientTCP.dataReciever.HandlePrivateChatMessage);
                    packets.Add((int)ChatMessage.Messages.TMSG, clientTCP.dataReciever.HandleTeamChatMessage);
                    packets.Add((int)ChatMessage.Messages.REJECT, clientTCP.dataReciever.HandleChatRejectMessage);
                    packets.Add((int)ChatMessage.Messages.CLOSE, clientTCP.dataReciever.HandleCloseMessage);
                    Debug.Log("Chat packets setup");
                    break;
                case ClientTypes.GAME:
                    packets.Add((int)GameMessage.Messages.GAMEJOIN, clientTCP.dataReciever.HandleGameJoinMessage);
                    packets.Add((int)GameMessage.Messages.REJECT, clientTCP.dataReciever.HandleGameRejectMessage);
                    packets.Add((int)GameMessage.Messages.TEAMJOIN, clientTCP.dataReciever.HandleTeamJoinMessage);
                    packets.Add((int)GameMessage.Messages.TEAMSWITCH, clientTCP.dataReciever.HandleTeamSwitchMessage);
                    packets.Add((int)GameMessage.Messages.STATUS, clientTCP.dataReciever.HandleStatusMessage);
                    packets.Add((int)GameMessage.Messages.SCORE, clientTCP.dataReciever.HandleScoreMessage);
                    packets.Add((int)GameMessage.Messages.GAMEEND, clientTCP.dataReciever.HandleGameEndMessage);
                    packets.Add((int)GameMessage.Messages.GAMESTART, clientTCP.dataReciever.HandleGameStartMessage);
                    packets.Add((int)GameMessage.Messages.GAMELEAVE, clientTCP.dataReciever.HandleRemoveMessage);
                    break;
                default:
                    Debug.LogError("Incorrect connection attempted");
                    break;
            }
        }
        public void HandleData(byte[] data)
        {
            if (playerBuffer == null)
            {
                playerBuffer = new ByteBuffer();
            }

            playerBuffer.Write(data);
            if (playerBuffer.Count() == 0)
            {
                playerBuffer.Clear();
                return;
            }

            if (data.Length <= 0)
            {
                playerBuffer.Clear();
                return;
            }

            HandleDataPackets(playerBuffer.ToArray());
            playerBuffer.Clear();
        }
        private void HandleDataPackets(byte[] data)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.Write(data);
            //check packet type
            int packetID = buffer.ReadInt(false);
            Debug.Log("Client: " + clientTCP.Type + " recieved Packet with ID: " + packetID + " being handled");

            if (packets.TryGetValue(packetID, out Packet packet))
            {
                Debug.Log("invoking: " + packetID);
                packet.Invoke(buffer.ToArray());
            }
            buffer.Dispose();
        }
    }
}
