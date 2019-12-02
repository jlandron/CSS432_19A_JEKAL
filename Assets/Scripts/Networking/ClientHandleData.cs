using Common.Protocols;
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
        //public Dictionary<int, Packet> packets;
        private ClientTCP clientTCP;
        private readonly object lockObject;

        public ClientHandleData(ClientTCP clientTCP)
        {
            this.clientTCP = clientTCP;
            //packets = new Dictionary<int, Packet>();
            lockObject = new object();
        }

        public void InitPackets()
        {

            //switch (clientTCP.Type)
            //{
            //    case ClientTypes.LOGIN:
            //        packets.Add((int)LoginMessage.Messages.AUTH, clientTCP.dataReciever.HandleAuthMessage);
            //        packets.Add((int)LoginMessage.Messages.REJECT, clientTCP.dataReciever.HandleRejectMessage);
            //        packets.Add((int)LoginMessage.Messages.DOWN, clientTCP.dataReciever.HandleRejectMessage);
            //        //Debug.Log("Login packets setup");
            //        break;
            //    case ClientTypes.CHAT:
            //        packets.Add((int)ChatMessage.Messages.LEAVE, clientTCP.dataReciever.HandleLeaveMessage);
            //        packets.Add((int)ChatMessage.Messages.SYSTEM, clientTCP.dataReciever.HandleSystemChatMessage);
            //        packets.Add((int)ChatMessage.Messages.MSG, clientTCP.dataReciever.HandleChatMessage);
            //        packets.Add((int)ChatMessage.Messages.PMSG, clientTCP.dataReciever.HandlePrivateChatMessage);
            //        packets.Add((int)ChatMessage.Messages.TMSG, clientTCP.dataReciever.HandleTeamChatMessage);
            //        packets.Add((int)ChatMessage.Messages.REJECT, clientTCP.dataReciever.HandleChatRejectMessage);
            //        packets.Add((int)ChatMessage.Messages.CLOSE, clientTCP.dataReciever.HandleCloseMessage);
            //        //Debug.Log("Chat packets setup");
            //        break;
            //    case ClientTypes.GAME:
            //        packets.Add((int)GameMessage.Messages.GAMEJOIN, clientTCP.dataReciever.HandleGameJoinMessage);
            //        packets.Add((int)GameMessage.Messages.REJECT, clientTCP.dataReciever.HandleGameRejectMessage);
            //        packets.Add((int)GameMessage.Messages.TEAMJOIN, clientTCP.dataReciever.HandleTeamJoinMessage);
            //        packets.Add((int)GameMessage.Messages.TEAMSWITCH, clientTCP.dataReciever.HandleTeamSwitchMessage);
            //        packets.Add((int)GameMessage.Messages.STATUS, clientTCP.dataReciever.HandleStatusMessage);
            //        packets.Add((int)GameMessage.Messages.SCORE, clientTCP.dataReciever.HandleScoreMessage);
            //        packets.Add((int)GameMessage.Messages.GAMEEND, clientTCP.dataReciever.HandleGameEndMessage);
            //        packets.Add((int)GameMessage.Messages.GAMESTART, clientTCP.dataReciever.HandleGameStartMessage);
            //        packets.Add((int)GameMessage.Messages.GAMELEAVE, clientTCP.dataReciever.HandleRemoveMessage);
            //        //Debug.Log("Game packets setup");
            //        break;
            //    default:
            //        Debug.LogError("Incorrect connection attempted");
            //        break;
            //}
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
            lock (lockObject)
            {
                ByteBuffer buffer = new ByteBuffer();
                buffer.Write(data);
                //check packet type
                int packetID = buffer.ReadInt(false);
                //Packet packet;
                //if (packets.TryGetValue(packetID, out packet))
                //{
                //    Debug.Log("Client: " + clientTCP.Type + " recieved Packet with ID: " + packetID + " being handled");
                //    try
                //    {
                //        packet.Invoke(buffer.ToArray());
                //    }
                //    catch (System.Exception e)
                //    {
                //        Debug.Log(e.Message);
                //        Debug.Log(e.Data);
                //        Debug.Log(e.Source);
                //        Debug.Log(e.TargetSite);
                //    }
                //}
                try
                {
                    switch (clientTCP.Type)
                    {
                        case ClientTypes.LOGIN:
                            switch ((LoginMessage.Messages)packetID)
                            {
                                case LoginMessage.Messages.LOGIN:
                                    break;
                                case LoginMessage.Messages.AUTH:
                                    clientTCP.dataReciever.HandleAuthMessage(buffer.ToArray());
                                    break;
                                case LoginMessage.Messages.REJECT:
                                    clientTCP.dataReciever.HandleRejectMessage(buffer.ToArray());
                                    break;
                                case LoginMessage.Messages.DOWN:
                                    clientTCP.dataReciever.HandleRejectMessage(buffer.ToArray());
                                    break;
                                default:
                                    break;
                            }
                            break;
                        case ClientTypes.CHAT:
                            switch ((ChatMessage.Messages)packetID)
                            {
                                case ChatMessage.Messages.JOIN:
                                    break;
                                case ChatMessage.Messages.LEAVE:
                                    clientTCP.dataReciever.HandleLeaveMessage(buffer.ToArray());
                                    break;
                                case ChatMessage.Messages.SYSTEM:
                                    clientTCP.dataReciever.HandleSystemChatMessage(buffer.ToArray());
                                    break;
                                case ChatMessage.Messages.MSG:
                                    clientTCP.dataReciever.HandleChatMessage(buffer.ToArray());
                                    break;
                                case ChatMessage.Messages.PMSG:
                                    clientTCP.dataReciever.HandlePrivateChatMessage(buffer.ToArray());
                                    break;
                                case ChatMessage.Messages.TMSG:
                                    clientTCP.dataReciever.HandleTeamChatMessage(buffer.ToArray());
                                    break;
                                case ChatMessage.Messages.REJECT:
                                    clientTCP.dataReciever.HandleChatRejectMessage(buffer.ToArray());
                                    break;
                                case ChatMessage.Messages.CLOSE:
                                    clientTCP.dataReciever.HandleCloseMessage(buffer.ToArray());
                                    break;
                                default:
                                    break;
                            }
                            break;
                        case ClientTypes.GAME:
                            switch ((GameMessage.Messages)packetID)
                            {
                                case GameMessage.Messages.GAMEJOIN:
                                    clientTCP.dataReciever.HandleGameJoinMessage(buffer.ToArray());
                                    break;
                                case GameMessage.Messages.REJECT:
                                    clientTCP.dataReciever.HandleGameRejectMessage(buffer.ToArray());
                                    break;
                                case GameMessage.Messages.TEAMJOIN:
                                    clientTCP.dataReciever.HandleTeamJoinMessage(buffer.ToArray());
                                    break;
                                case GameMessage.Messages.TEAMSWITCH:
                                    clientTCP.dataReciever.HandleTeamSwitchMessage(buffer.ToArray());
                                    break;
                                case GameMessage.Messages.UPDATE:
                                    break;
                                case GameMessage.Messages.TAG:
                                    break;
                                case GameMessage.Messages.STATUS:
                                    clientTCP.dataReciever.HandleStatusMessage(buffer.ToArray());
                                    break;
                                case GameMessage.Messages.SCORE:
                                    clientTCP.dataReciever.HandleScoreMessage(buffer.ToArray());
                                    break;
                                case GameMessage.Messages.GAMEEND:
                                    clientTCP.dataReciever.HandleGameEndMessage(buffer.ToArray());
                                    break;
                                case GameMessage.Messages.GAMESTART:
                                    clientTCP.dataReciever.HandleGameStartMessage(buffer.ToArray());
                                    break;
                                case GameMessage.Messages.GAMELEAVE:
                                    clientTCP.dataReciever.HandleRemoveMessage(buffer.ToArray());
                                    break;
                                default:
                                    break;
                            }
                            break;
                        default:
                            break;
                    }
                }
                catch (System.Exception e)
                {
                    Debug.Log(e.Message);
                }
                buffer.Dispose();
            }
        }
    }
}
