using Jekal.Objects;

namespace Jekal.Protocols
{
    public class LoginMessage
    {
        public string Player { get; set; }
        public string ChatIP { get; set; }
        public int ChatPort { get; set; }
        public string GameIP { get; set; }
        public int GamePort { get; set; }
        public int SessionID { get; set; }
        public string Message { get; set; }
        public Messages MessageType { get; set; }

        public ByteBuffer Buffer { get; set; }

        public enum Messages
        {
            LOGIN = 1,
            AUTH,
            REJECT,
            DOWN
        };

        public LoginMessage()
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
                case Messages.LOGIN:
                    Player = Buffer.ReadString();
                    break;
                case Messages.AUTH:
                    ChatPort = Buffer.ReadInt();
                    GamePort = Buffer.ReadInt();
                    SessionID = Buffer.ReadInt();
                    break;
                case Messages.REJECT:
                case Messages.DOWN:
                    Message = Buffer.ReadString();
                    break;
                default:
                    return false;
            }
            return true;
        }
    }
}
