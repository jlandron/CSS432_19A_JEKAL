using System;
using Common.Protocols;
using Game.UI;
using UnityEngine;

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

        internal void HandleLoginMessage(byte[] data)
        {
            Debug.Log("Not sure why I am reciving this message");
        }

        internal void HandleRejectMessage(byte[] data)
        {
            
            Debug.Log("Login rejected");
            ByteBuffer buffer = new ByteBuffer();
            buffer.Write(data);
            int packetID = buffer.ReadInt();
            string msg = buffer.ReadString();
            buffer.Dispose();
            NetworkManager.Instance.PrintErrorToCanvas(msg);
            //give player prompt to retry
            lock (NetworkManager.Instance)
            {
                NetworkManager.Instance.ShouldKillLogin = true;
            }

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

