using UnityEngine;
using Common.Protocols;
using System;
using Game.UI;

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
            Debug.LogError("Login rejected");
            ByteBuffer buffer = new ByteBuffer();
            buffer.Write(data);
            int packetID = buffer.ReadInt();
            string msg = buffer.ReadString();
            GameObject.FindObjectOfType<PrintMessageToTextbox>().WriteMessage(msg);
            //give player prompt to retry
            buffer.Dispose();
            NetworkManager.Instance.loginSuccess = false;
            NetworkManager.Instance.loginRequestSent = false;
            NetworkManager.Instance.loginClientTCP.Disconnect();
        }

        /////// Chat server messages ////////
        public void HandleWelcomeMessage(byte[] data)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.Write(data);
            int packetID = buffer.ReadInt();
            string msg = buffer.ReadString();
            //send message to chat controller
            buffer.Dispose();
        }

        public void HandleChatMessage(byte[] data)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.Write(data);
            int packetID = buffer.ReadInt();
            string msg = buffer.ReadString();
            //send message to chat controller
            buffer.Dispose();
        }

        internal void HandleJoinMessage(byte[] data)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.Write(data);
            int packetID = buffer.ReadInt();
            string msg = buffer.ReadString();
            //send message to chat controller
            buffer.Dispose();
        }

        internal void HandleLeaveMessage(byte[] data)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.Write(data);
            int packetID = buffer.ReadInt();
            string msg = buffer.ReadString();
            //send message to chat controller
            buffer.Dispose();
        }
        internal void HandleChatRejectMessage(byte[] data)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.Write(data);
            int packetID = buffer.ReadInt();
            string msg = buffer.ReadString();

            //send message to chat controller
            buffer.Dispose();
            NetworkManager.Instance.chatClientTCP.Disconnect();
            NetworkManager.Instance.chatServerRequestSent = false;
        }
        
        /////// Game server messages ///////
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

