using Jekal.Objects;

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
        public float PosX { get; set; }
        public float PosY { get; set; }
        public float PosZ { get; set; }
        public float RotX { get; set; }
        public float RotY { get; set; }
        public float RotZ { get; set; }
        public float RotW { get; set; }


        public GameMessage()
        {
            Buffer = new ByteBuffer();
        }

        public bool Parse()
        {
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
                    break;
                case Messages.TEAMSWITCH:
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
                    break;
                case Messages.SCORE:
                    break;
                case Messages.GAMEEND:
                    break;
                case Messages.GAMESTART:
                    break;
                case Messages.GAMELEAVE:
                    break;
                default:
                    return false;
            }
            return true;
        }
    }
}
