namespace Common.Protocols
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
            GAMELEAVE,
        }
    }
}
