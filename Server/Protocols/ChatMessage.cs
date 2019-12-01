using Jekal.Objects;

namespace Jekal.Protocols
{
    public class ChatMessage
    {
        public string Source { get; set; }
        public int SourceId { get; set; }
        public string Destination { get; set; }
        public string Message { get; set; }
        public Messages MessageType { get; set; }

        public ByteBuffer Buffer { get; set; }

        public enum Messages
        {
            JOIN = 1,
            LEAVE,
            SYSTEM,
            MSG,
            PMSG,
            TMSG,
            REJECT,
            CLOSE
        };

        public ChatMessage()
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
                case Messages.JOIN:
                case Messages.LEAVE:
                    Source = Buffer.ReadString();
                    SourceId = Buffer.ReadInt();
                    break;
                case Messages.SYSTEM:
                    Message = Buffer.ReadString();
                    break;
                case Messages.MSG:
                    Source = Buffer.ReadString();
                    SourceId = Buffer.ReadInt();
                    Message = Buffer.ReadString();
                    break;
                case Messages.PMSG:
                    Source = Buffer.ReadString();
                    SourceId = Buffer.ReadInt();
                    Destination = Buffer.ReadString();
                    Message = Buffer.ReadString();
                    break;
                case Messages.TMSG:
                    Source = Buffer.ReadString();
                    SourceId = Buffer.ReadInt();
                    Destination = Buffer.ReadString();
                    Message = Buffer.ReadString();
                    break;
                case Messages.REJECT:
                    Message = Buffer.ReadString();
                    break;
                case Messages.CLOSE:
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
