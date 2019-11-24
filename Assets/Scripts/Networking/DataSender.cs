using Common.Protocols;

namespace GameClient
{
    //!!!make sure to sync enum with Server code!!!

    public class DataSender
    {
        private ClientTCP clientTCP;

        public DataSender(ClientTCP clientTCP)
        {
            this.clientTCP = clientTCP;
        }

        public void SendChatMessage()
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.Write((int)ChatMessage.Messages.MSG);
            buffer.Write("Client chat message to sent from client!");
            clientTCP.SendData(buffer.ToArray());
            buffer.Dispose();
        }

        public void SendTransformMessage(byte[] data)
        {
            ByteBuffer buffer = new ByteBuffer();
            //buffer.Write((int)GameMessage.UPDATE);
            buffer.Write(data);
            clientTCP.SendData(buffer.ToArray());
            buffer.Dispose();
        }
        public void RequestLogin(string playerName)
        {
            ByteBuffer loginRequest = new ByteBuffer();
            loginRequest.Write((int)LoginMessage.Messages.LOGIN);
            loginRequest.Write(playerName);
            clientTCP.SendData(loginRequest.ToArray());
            loginRequest.Dispose();
        }
    }
}

