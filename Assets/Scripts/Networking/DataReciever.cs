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
        //////// Login server messages ////////
        internal void HandleAuthMessage(byte[] data)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.Write(data);
            int packetID = buffer.ReadInt();
            NetworkManager.Instance.ChatServerIP = buffer.ReadString();
            NetworkManager.Instance.ChatServerPort = buffer.ReadInt();
            NetworkManager.Instance.GameServerIP = buffer.ReadString();
            NetworkManager.Instance.GameServerPort = buffer.ReadInt();
            NetworkManager.Instance.SetLocalPlayerID(buffer.ReadInt());
            buffer.Dispose();
            NetworkManager.Instance.loginSuccess = true;
        }
        internal void HandleRejectMessage(byte[] data)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.Write(data);
            int packetID = buffer.ReadInt();
            string msg = buffer.ReadString();
            //give player prompt to retry
            buffer.Dispose();
            NetworkManager.Instance.loginClientTCP.Disconnect();
        }
        /////// Chat server messages //////
        public void HandleWelcomeMessage(byte[] data)
        {

        }

        public void HandleChatMessage(byte[] data)
        {

        }

        

        internal void HandleJoinMessage(byte[] data)
        {

        }

        internal void HandleLeaveMessage(byte[] data)
        {

        }
        internal void HandleChatRejectMessage(byte[] data)
        {

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

