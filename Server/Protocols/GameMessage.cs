using Jekal.Objects;
using System.Collections.Generic;

namespace Jekal.Protocols
{
    public class GameMessage
    {
        public enum Messages
        {
            GAMEJOIN = 1,
            REJECT,
            TEAMJOIN,
            TEAMSWITCH,
            UPDATE,
            TAG,
            STATUS,
            SCORE,
            GAMEEND,
            GAMESTART,
            GAMELEAVE
        }

        public ByteBuffer Buffer { get; set; }
        public Messages MessageType { get; set; }
        public string Source { get; set; }
        public int SourceId { get; set; }
        public string Target { get; set; }
        public int TargetId { get; set; }
        public int CurrentTeamId { get; set; }
        public int NewTeamId { get; set; }
        public int GameTime { get; set; }
        public float PosX { get; set; }
        public float PosY { get; set; }
        public float PosZ { get; set; }
        public float RotX { get; set; }
        public float RotY { get; set; }
        public float RotZ { get; set; }
        public float RotW { get; set; }
        public List<Player> Players { get; set; }
        public int PlayerCount { get; set; }


        public GameMessage()
        {
            Buffer = new ByteBuffer();
            Players = new List<Player>();
        }

        public bool Parse()
        {
            // Just to be safe
            Players.Clear();

            // Check to make sure we have a message
            if (Buffer.Length() < sizeof(int))
            {
                return false;
            }

            // Parse the type of message
            int msgType = Buffer.ReadInt();
            MessageType = (Messages)msgType;

            switch (MessageType)
            {
                case Messages.GAMEJOIN:
                    Source = Buffer.ReadString();
                    SourceId = Buffer.ReadInt();
                    break;
                case Messages.TEAMJOIN:
                    Source = Buffer.ReadString();
                    CurrentTeamId = Buffer.ReadInt();
                    break;
                case Messages.TEAMSWITCH:
                    Source = Buffer.ReadString();
                    Target = Buffer.ReadString();
                    CurrentTeamId = Buffer.ReadInt();
                    NewTeamId = Buffer.ReadInt();
                    break;
                case Messages.UPDATE:
                    Source = Buffer.ReadString();
                    PosX = Buffer.ReadFloat();
                    PosY = Buffer.ReadFloat();
                    PosZ = Buffer.ReadFloat();
                    RotX = Buffer.ReadFloat();
                    RotY = Buffer.ReadFloat();
                    RotZ = Buffer.ReadFloat();
                    RotW = Buffer.ReadFloat();
                    break;
                case Messages.TAG:
                    Source = Buffer.ReadString();
                    Target = Buffer.ReadString();
                    break;
                case Messages.STATUS:
                    GameTime = Buffer.ReadInt();
                    PlayerCount = Buffer.ReadInt();
                    for (int i = 0; i < PlayerCount; i++)
                    {
                        var player = new Player();
                        player.SessionID = Buffer.ReadInt();
                        player.PosX = Buffer.ReadFloat();
                        player.PosY = Buffer.ReadFloat();
                        player.PosZ = Buffer.ReadFloat();
                        player.RotX = Buffer.ReadFloat();
                        player.RotY = Buffer.ReadFloat();
                        player.RotZ = Buffer.ReadFloat();
                        player.RotW = Buffer.ReadFloat();
                        Players.Add(player);
                    }
                    break;
                case Messages.SCORE:
                    break;
                case Messages.GAMEEND:
                    CurrentTeamId = Buffer.ReadInt();
                    break;
                case Messages.GAMESTART:
                    GameTime = Buffer.ReadInt();
                    break;
                case Messages.GAMELEAVE:
                    Source = Buffer.ReadString();
                    SourceId = Buffer.ReadInt();
                    break;
                default:
                    return false;
            }
            return true;
        }
    }
}
