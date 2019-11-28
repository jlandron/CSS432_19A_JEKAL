using Common.Protocols;
using NetworkGame.UI;
using System;
using UnityEngine;

namespace NetworkGame.Client
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
            NetworkManager.Instance.LoginSuccess = true;
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
            NetworkManager.Instance.errorMessageToPrint = msg;
            //give player prompt to retry
            NetworkManager.Instance.ShouldKillLogin = true;
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

        internal void HandleCloseMessage(byte[] data)
        {
            Debug.Log("Recieved chat close signal");
            NetworkManager.Instance.chatClientTCP.Disconnect();
            NetworkManager.Instance.ChatRequestSent = false;
        }

        public void HandleChatMessage(byte[] data)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.Write(data);
            int packetID = buffer.ReadInt();
            string playerName = buffer.ReadString();
            int playerID = buffer.ReadInt();
            string msg = buffer.ReadString();
            Debug.Log("Chat message: " + msg);
            ChatManager.Instance.chatMessages.Enqueue(new QueuedMessage(playerName, msg));
            //send message to chat controller
            buffer.Dispose();
        }
        public void HandleSystemChatMessage(byte[] data)
        {

            ByteBuffer buffer = new ByteBuffer();
            buffer.Write(data);
            int packetID = buffer.ReadInt();
            string msg = buffer.ReadString();
            ChatManager.Instance.chatMessages.Enqueue(new QueuedMessage("System", msg));
            Debug.Log("System message: " + msg);
            buffer.Dispose();

        }
        internal void HandlePrivateChatMessage(byte[] data)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.Write(data);
            int packetID = buffer.ReadInt();
            string msg = buffer.ReadString();
            ChatManager.Instance.chatMessages.Enqueue(new QueuedMessage("System", msg));
            Debug.Log("System message: " + msg);
            buffer.Dispose();
        }
        internal void HandleTeamChatMessage(byte[] data)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.Write(data);
            int packetID = buffer.ReadInt();
            string msg = buffer.ReadString();
            ChatManager.Instance.chatMessages.Enqueue(new QueuedMessage("System", msg));
            Debug.Log("System message: " + msg);
            buffer.Dispose();
        }

        internal void HandleJoinMessage(byte[] data)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.Write(data);
            int packetID = buffer.ReadInt();
            string msg = buffer.ReadString();
            ChatManager.Instance.chatMessages.Enqueue(new QueuedMessage("System", msg));
            Debug.Log("System message: " + msg);
            buffer.Dispose();
        }

        internal void HandleLeaveMessage(byte[] data)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.Write(data);
            int packetID = buffer.ReadInt();
            string msg = buffer.ReadString();
            ChatManager.Instance.chatMessages.Enqueue(new QueuedMessage("System", msg));
            Debug.Log("System message: " + msg);
            buffer.Dispose();
        }
        internal void HandleChatRejectMessage(byte[] data)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.Write(data);
            int packetID = buffer.ReadInt();
            string msg = buffer.ReadString();
            ChatManager.Instance.chatMessages.Enqueue(new QueuedMessage("System", msg));
            Debug.Log("System message: " + msg);
            buffer.Dispose();
            NetworkManager.Instance.ShouldKillChat = true;
            NetworkManager.Instance.ChatRequestSent = false;
        }

        /////// Game server messages ///////
        public void HandleInstatiatePlayer(byte[] data)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.Write(data);
            int packetID = buffer.ReadInt();
            int index = buffer.ReadInt();
            buffer.Dispose();
            PlayerManager.Instance.playersToSpawn.Enqueue(index);
        }

        public void HandlePlayerTranformMessage(byte[] data)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.Write(data);
            PlayerManager.Instance.playersToUpdate.Enqueue(buffer.ToArray());
            buffer.Dispose();
        }
    }
}

