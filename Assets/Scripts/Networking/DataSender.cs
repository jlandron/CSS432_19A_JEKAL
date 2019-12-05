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
        internal void SendPrivateChatMessage(string message, string targetPlayer)
        {
            Debug.Log("Sending chat msg");
            ByteBuffer buffer = new ByteBuffer();
            buffer.Write((int)ChatMessage.Messages.PMSG);
            buffer.Write(NetworkManager.Instance.PlayerName);
            buffer.Write(NetworkManager.Instance.PlayerID);
            buffer.Write(targetPlayer);
            buffer.Write(message);
            clientTCP.dataToSend.Enqueue(buffer.ToArray());
            buffer.Dispose();
        }
        internal void SendTeamChatMessage(string message)
        {
            Debug.Log("Sending chat msg");
            ByteBuffer buffer = new ByteBuffer();
            buffer.Write((int)ChatMessage.Messages.TMSG);
            buffer.Write(NetworkManager.Instance.PlayerName);
            buffer.Write(NetworkManager.Instance.PlayerID);
            buffer.Write(message);
            clientTCP.dataToSend.Enqueue(buffer.ToArray());
            buffer.Dispose();
        }
        internal void SendLeaveMessage()
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.Write((int)ChatMessage.Messages.LEAVE);
            buffer.Write(NetworkManager.Instance.PlayerName);
            buffer.Write(NetworkManager.Instance.PlayerID);
            clientTCP.dataToSend.Enqueue(buffer.ToArray());
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
        public void SendGameJoinRequest()
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.Write((int)GameMessage.Messages.GAMEJOIN);
            buffer.Write(NetworkManager.Instance.PlayerName);
            buffer.Write(NetworkManager.Instance.PlayerID);
            clientTCP.dataToSend.Enqueue(buffer.ToArray());
            buffer.Dispose();
        }
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
            buffer.Write((int)GameMessage.Messages.TAG);
            buffer.Write(NetworkManager.Instance.PlayerID);
            buffer.Write(playerTagged);
            clientTCP.dataToSend.Enqueue(buffer.ToArray());
        }
        public void SendGameLeaveMessage()
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.Write((int)GameMessage.Messages.GAMELEAVE);
            buffer.Write(NetworkManager.Instance.PlayerName);
            buffer.Write(NetworkManager.Instance.PlayerID);
            clientTCP.dataToSend.Enqueue(buffer.ToArray());
        }
    }
}

