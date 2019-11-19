namespace Common.Protocols
{
    public class LoginMessage
    {
        string _player;
        
        public Messages MessageType { get; set; }
        public string Body { get; set; }

        public enum Messages
        {
            LOGIN = 1,
            AUTH,
            REJECT,
            DOWN
        };

        public bool Parse(string msg)
        {
            _player = "Thaldin";
            return true;
        }

        public string GetPlayerName()
        {
            return _player;
        }
    }
}
