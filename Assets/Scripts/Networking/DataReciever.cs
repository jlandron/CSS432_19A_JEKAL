using System;
using Common.Protocols;
using NetworkGame.UI;
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

        /////// Chat server messages ////////
        internal void HandleLeaveMessage(byte[] data)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.Write(data);
            _ = buffer.ReadInt();
            string playerName = buffer.ReadString();
            _ = buffer.ReadInt();
            buffer.Dispose();
            ChatManager.Instance.chatMessages.Enqueue(new QueuedMessage(playerName, " Disconnected"));
            //Debug.Log("Leave message: " + playerName);

        }
        internal void HandleSystemChatMessage(byte[] data)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.Write(data);
            _ = buffer.ReadInt();
            string msg = buffer.ReadString();
            buffer.Dispose();
            ChatManager.Instance.chatMessages.Enqueue(new QueuedMessage("System", msg, QueuedMessage.Type.SYSTEM));
            //Debug.Log("System message: " + msg);

        }
        internal void HandleChatMessage(byte[] data)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.Write(data);
            _ = buffer.ReadInt();
            string playerName = buffer.ReadString();
            _ = buffer.ReadInt();
            string msg = buffer.ReadString();
            buffer.Dispose();
            //Debug.Log("Chat message: " + msg);
            ChatManager.Instance.chatMessages.Enqueue(new QueuedMessage(playerName, msg));
        }
        internal void HandlePrivateChatMessage(byte[] data)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.Write(data);
            _ = buffer.ReadInt();
            string playerName = buffer.ReadString();
            _ = buffer.ReadInt();
            _ = buffer.ReadString();
            string msg = buffer.ReadString();
            buffer.Dispose();
            //Debug.Log("Chat message: " + msg);
            ChatManager.Instance.chatMessages.Enqueue(new QueuedMessage(playerName, msg, QueuedMessage.Type.PRIVATE));
        }
        internal void HandleTeamChatMessage(byte[] data)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.Write(data);
            _ = buffer.ReadInt();
            string playerName = buffer.ReadString();
            _ = buffer.ReadInt();
            string msg = buffer.ReadString();
            buffer.Dispose();
            //Debug.Log("Chat message: " + msg);
            ChatManager.Instance.chatMessages.Enqueue(new QueuedMessage(playerName, msg, QueuedMessage.Type.TEAM));
        }
        internal void HandleChatRejectMessage(byte[] data)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.Write(data);
            _ = buffer.ReadInt();
            string msg = buffer.ReadString();
            //Debug.Log("System message: " + msg);
            buffer.Dispose();
            NetworkManager.Instance.ShouldKillChat = true;
        }
        internal void HandleCloseMessage(byte[] data)
        {
            //Debug.Log("Recieved chat close signal");
            NetworkManager.Instance.ShouldKillChat = true;
        }

        //////// Login server messages ////////
        internal void HandleAuthMessage(byte[] data)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.Write(data);
            _ = buffer.ReadInt();
            NetworkManager.Instance.ChatServerIP = buffer.ReadString();
            NetworkManager.Instance.ChatServerPort = buffer.ReadInt();
            NetworkManager.Instance.GameServerIP = buffer.ReadString();
            NetworkManager.Instance.GameServerPort = buffer.ReadInt();
            NetworkManager.Instance.SetLocalPlayerID(buffer.ReadInt());
            buffer.Dispose();
            NetworkManager.Instance.LoginSuccess = true;
        }
        internal void HandleRejectMessage(byte[] data)
        {
            Debug.Log("Login rejected");
            ByteBuffer buffer = new ByteBuffer();
            buffer.Write(data);
            _ = buffer.ReadInt();
            string msg = buffer.ReadString();
            buffer.Dispose();
            NetworkManager.Instance.errorMessageToPrint = msg;
            //give player prompt to retry
            NetworkManager.Instance.ShouldKillLogin = true;
        }

        /////// Game server messages ///////
        internal void HandleGameJoinMessage(byte[] data)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.Write(data);
            _ = buffer.ReadInt();
            string playerName = buffer.ReadString();
            int playerID = buffer.ReadInt();
            buffer.Dispose();
            //Debug.Log("Setting local playerID: " + playerID);
            lock (NetworkManager.Instance)
            {
                NetworkManager.Instance.PlayerID = playerID;
                NetworkManager.Instance.PlayerName = playerName;
            }
        }
        internal void HandleGameRejectMessage(byte[] data)
        {
            //Debug.Log("Game rejected");
            ByteBuffer buffer = new ByteBuffer();
            buffer.Write(data);
            _ = buffer.ReadInt();
            string msg = buffer.ReadString();
            buffer.Dispose();
            NetworkManager.Instance.errorMessageToPrint = msg;
            //give player prompt to retry
            NetworkManager.Instance.ShouldKillLogin = true;
            NetworkManager.Instance.ShouldKillGame = true;
        }
        internal void HandleTeamJoinMessage(byte[] data)
        {
            //Debug.Log("Handling team join messages");
            ByteBuffer buffer = new ByteBuffer();
            buffer.Write(data);
            _ = buffer.ReadInt(); //eliminate message type
            int playerID = buffer.ReadInt();
            int teamNum = buffer.ReadInt();
            ByteBuffer playerInst = new ByteBuffer();
            playerInst.Write(playerID); //playerID
            playerInst.Write(teamNum); //TeamID
            PlayerManager.Instance.playersToSpawn.Enqueue(playerInst.ToArray());
            playerInst.Dispose();
            buffer.Dispose();
        }
        internal void HandleTeamSwitchMessage(byte[] data)
        {
            //Debug.Log("Handling team switch messages");
            ByteBuffer buffer = new ByteBuffer();
            buffer.Write(data);
            PlayerManager.Instance.playersSwitchingTeams.Enqueue(buffer.ToArray());
            buffer.Dispose();
        }
        internal void HandleStatusMessage(byte[] data)
        {
            Debug.Log("updating players");
            ByteBuffer buffer = new ByteBuffer();
            buffer.Write(data);
            PlayerManager.Instance.playersToUpdate.Enqueue(buffer.ToArray());
            buffer.Dispose();
        }
        internal void HandleScoreMessage(byte[] data)
        {
            Debug.Log("Score message not implemented");
        }
        internal void HandleGameEndMessage(byte[] data)
        {
            GameManager.Instance.MyGameState = GameManager.GameState.END;
        }
        internal void HandleGameStartMessage(byte[] data)
        {
            GameManager.Instance.MyGameState = GameManager.GameState.START;
        }
        internal void HandleRemoveMessage(byte[] data)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.Write(data);
            PlayerManager.Instance.playersToRemove.Enqueue(buffer.ToArray());
            buffer.Dispose();
        }


        internal void HandleTeamListMessage(byte[] data)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.Write(data);
            _ = buffer.ReadInt();
            int numPlayersInGame = buffer.ReadInt();
            for (int i = 0; i < numPlayersInGame; i++)
            {
                Debug.Log("Handle teamList");
                int playerID = buffer.ReadInt();
                int teamNum = buffer.ReadInt();
                ByteBuffer playerInst = new ByteBuffer();
                playerInst.Write(playerID);
                playerInst.Write(teamNum);
                PlayerManager.Instance.playersToSpawn.Enqueue(playerInst.ToArray());
                playerInst.Dispose();
            }
            buffer.Dispose();
        }

    }
}

