namespace Common.Protocols
{
    public class ChatProtocol
    {
        public enum Messages
        {
            JOIN = 1,
            LEAVE,
            SYSTEM,
            MSG,
            PMSG,
            TMSG
        };
    }
}
