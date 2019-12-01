namespace Common.Protocols
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
    }
}
