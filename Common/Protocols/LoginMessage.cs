namespace Common.Protocols
{
    public class LoginMessage
    {
        string _player;

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

            // Check to make sure it's a LOGIN request
            var msgType = Buffer.ReadInt(false);
            if (msgType != (int)Messages.LOGIN)
            {
                return false;
            }

            // Get Player Name
            _player = Buffer.ReadString(false);
            return true;
        }

        public string GetPlayerName()
        {
            return _player;
        }
    }
}
