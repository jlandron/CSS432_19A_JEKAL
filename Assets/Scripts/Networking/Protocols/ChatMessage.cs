﻿namespace Common.Protocols
{
    public class ChatMessage
    {
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
    }
}
