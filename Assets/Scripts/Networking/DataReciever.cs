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

        internal void HandleJoinMessage(byte[] data)
        {
            Debug.Log("Not sure why I am reciving this message: Chat JOIN");
        }
        internal void HandleLeaveMessage(byte[] data)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.Write(data);
            _ = buffer.ReadInt();
            string playerName = buffer.ReadString();
            _ = buffer.ReadInt();
            buffer.Dispose();
            ChatManager.Instance.chatMessages.Enqueue(new QueuedMessage(playerName, " Disconnected"));
            Debug.Log("Leave message: " + playerName);

        }
        internal void HandleSystemChatMessage(byte[] data)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.Write(data);
            _ = buffer.ReadInt();
            string msg = buffer.ReadString();
            buffer.Dispose();
            ChatManager.Instance.chatMessages.Enqueue(new QueuedMessage("System", msg, QueuedMessage.Type.SYSTEM));
            Debug.Log("System message: " + msg);

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
            Debug.Log("Chat message: " + msg);
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
            Debug.Log("Chat message: " + msg);
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
            Debug.Log("Chat message: " + msg);
            ChatManager.Instance.chatMessages.Enqueue(new QueuedMessage(playerName, msg, QueuedMessage.Type.TEAM));
        }
        internal void HandleChatRejectMessage(byte[] data)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.Write(data);
            _ = buffer.ReadInt();
            string msg = buffer.ReadString();
            Debug.Log("System message: " + msg);
            buffer.Dispose();
            NetworkManager.Instance.ShouldKillChat = true;
            NetworkManager.Instance.ChatRequestSent = false;
        }
        internal void HandleCloseMessage(byte[] data)
        {
            Debug.Log("Recieved chat close signal");
            NetworkManager.Instance.chatClientTCP.Disconnect();
            NetworkManager.Instance.ChatRequestSent = false;
        }
        //////// Login server messages ////////
        internal void HandleLoginMessage(byte[] data)
        {
            Debug.Log("Not sure why I am reciving this message: Login request");
        }
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
            PlayerManager.Instance.playersToSpawn.Enqueue(data);
        }
        internal void HandleGameRejectMessage(byte[] data)
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
            NetworkManager.Instance.ShouldKillGame = true;
        }
        internal void HandleTeamJoinMessage(byte[] data)
        {
            PlayerManager.Instance.playersJoiningTeam.Enqueue(data);
        }
        internal void HandleTeamSwitchMessage(byte[] data)
        {
            PlayerManager.Instance.playersJoiningTeam.Enqueue(data);
        }
        internal void HandleUpdateMessage(byte[] data)
        {
            Debug.Log("Not sure why I am reciving this message: Game Update");
        }
        internal void HandleTagMessage(byte[] data)
        {
            Debug.Log("Not sure why I am reciving this message: Game Tag");
        }
        internal void HandleStatusMessage(byte[] data)
        {
            PlayerManager.Instance.playersToUpdate.Enqueue(data);
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
        internal void HandleGameWaitMessage(byte[] data)
        {
            GameManager.Instance.MyGameState = GameManager.GameState.WAIT;
        }
        internal void HandleRemoveMessage(byte[] data)
        {
            PlayerManager.Instance.playersToRemove.Enqueue(data);
        }
    }
}

