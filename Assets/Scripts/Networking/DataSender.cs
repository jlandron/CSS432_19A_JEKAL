using Common.Protocols;
using UnityEngine;

namespace NetworkGame.Client
{
    public class DataSender
    {
        private ClientTCP clientTCP;

        public DataSender(ClientTCP clientTCP)
        {
            this.clientTCP = clientTCP;
        }
        ////// Chat ///////
        internal void RequestJoin()
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.Write((int)ChatMessage.Messages.JOIN);
            buffer.Write(NetworkManager.Instance.PlayerName);
            buffer.Write(NetworkManager.Instance.PlayerID);
            clientTCP.dataToSend.Enqueue(buffer.ToArray());
            buffer.Dispose();
        }
        internal void SendChatMessage(string message)
        {
            Debug.Log("Sending chat msg");
            ByteBuffer buffer = new ByteBuffer();
            buffer.Write((int)ChatMessage.Messages.MSG);
            buffer.Write(NetworkManager.Instance.PlayerName);
            buffer.Write(NetworkManager.Instance.PlayerID);
            buffer.Write(message);
            clientTCP.dataToSend.Enqueue(buffer.ToArray());
            buffer.Dispose();
        }

        /////// Login //////
        public void RequestLogin()
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.Write((int)LoginMessage.Messages.LOGIN);
            buffer.Write(NetworkManager.Instance.PlayerName);
            clientTCP.dataToSend.Enqueue(buffer.ToArray());
            buffer.Dispose();
        }

        /////// Game ////////
        public void SendTransformMessage(byte[] data)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.Write((int)GameMessage.Messages.UPDATE);
            buffer.Write(data);
            clientTCP.dataToSend.Enqueue(buffer.ToArray());
            buffer.Dispose();
        }
        public void SendTagMessage(int playerTagged)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.Write(NetworkManager.Instance.PlayerID);
            buffer.Write(playerTagged);
            clientTCP.dataToSend.Enqueue(buffer.ToArray());
        }
    }
}

