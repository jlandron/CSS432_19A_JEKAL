using Jekal.Objects;

namespace Jekal.Protocols
{
    public class GameMessage
    {
        public enum Messages
        {
            GAMEJOIN = 1,
            TEAMJOIN,
            TEAMSWITCH,
            UPDATE,
            TAG,
            STATUS,
            SCORE,
            GAMEEND,
            GAMESTART,
            GAMEWAIT
        }

        public ByteBuffer Buffer { get; set; }
        public Messages MessageType { get; set; }
        public string Source { get; set; }
        public int SourceId { get; set; }

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
                    break;
                case Messages.TAG:
                    break;
                case Messages.STATUS:
                    break;
                case Messages.SCORE:
                    break;
                case Messages.GAMEEND:
                    break;
                case Messages.GAMESTART:
                    break;
                case Messages.GAMEWAIT:
                    break;
                default:
                    return false;
            }
            return true;
        }
    }
}
