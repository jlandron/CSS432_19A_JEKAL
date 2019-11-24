using UnityEngine;
using Common.Protocols;
using System;

namespace GameClient
{

    public class DataReciever
    {
        private ClientTCP clientTCP;

        public DataReciever(ClientTCP clientTCP)
        {
            this.clientTCP = clientTCP;
        }

        public void HandleWelcomeMessage(byte[] data)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.Write(data);
            int packetID = buffer.ReadInt();
            string msg = buffer.ReadString();
            GameObject.FindGameObjectWithTag("Player").GetComponent<NetworkPlayer>().playerID = buffer.ReadInt();
            buffer.Dispose();
            //TODO: push message to chat window
            Debug.Log(msg);
        }

        //implement pushing chat to data sender
        public void HandleChatMessage(byte[] data)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.Write(data);
            int packetID = buffer.ReadInt();
            string msg = buffer.ReadString();
            buffer.Dispose();
            //TODO: push message to chat window
        }

        internal void HandleRejectMessage(byte[] data)
        {
            throw new NotImplementedException();
        }

        internal void HandleJoinMessage(byte[] data)
        {
            throw new NotImplementedException();
        }

        internal void HandleLeaveMessage(byte[] data)
        {
            throw new NotImplementedException();
        }

        internal void HandleDownMessage(byte[] data)
        {
            throw new NotImplementedException();
        }

        internal void HandleAuthMessage(byte[] data)
        {
            throw new NotImplementedException();
        }

        public void HandleInstatiatePlayer(byte[] data)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.Write(data);
            int packetID = buffer.ReadInt();
            int index = buffer.ReadInt();
            buffer.Dispose();
            NetworkManager.Instance.InstatiatePlayer(index);
        }

        public void HandlePlayerTranformMessage(byte[] data)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.Write(data);
            NetworkManager.Instance.UpdatePlayerLocation(buffer.ToArray());
            buffer.Dispose();
        }
    }
}

