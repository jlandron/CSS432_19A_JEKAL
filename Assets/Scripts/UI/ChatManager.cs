using NetworkGame.Client;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;

namespace NetworkGame.UI
{
    public class ChatManager : MonoBehaviour
    {
        [SerializeField]
        internal List<Message> chatHistory;
        public ConcurrentQueue<QueuedMessage> chatMessages;

        [SerializeField]
        private int maxMessages = 50;

        [SerializeField]
        GameObject chatPanel;
        [SerializeField]
        GameObject textObject;
        [SerializeField]
        InputField chatBox;
        [SerializeField]
        GameObject localPlayer;

        public static ChatManager Instance { get; private set; }
        private void Awake()
        {
            if (Instance != null)
            {
                return;
            }
            Instance = this;
            chatHistory = new List<Message>();
            chatMessages = new ConcurrentQueue<QueuedMessage>();
        }

        private void Update()
        {
            if (chatBox.text != "")
            {
                if (Input.GetKeyDown(KeyCode.Return))
                {
                    string text = chatBox.text;
                    chatBox.text = "";
                    if(text.StartsWith("/"))
                    {
                        string[] tokens = text.Split(' ');
                        string newText = "";
                        for (int i = 1; i < tokens.Length; i++)
                        {
                            newText += tokens[i] + " ";
                        }
                        switch (tokens[0])      
                        {
                            case "/t":
                            case "/T":
                                for (int i = 1; i < tokens.Length; i++)
                                {
                                    newText += tokens[i] + " ";
                                }
                                NetworkManager.Instance.chatClientTCP.dataSender.SendTeamChatMessage(newText);
                                break;
                            case "/p":
                            case "/P":
                                string player = tokens[1];
                                for (int i = 2; i < tokens.Length; i++)
                                {
                                    newText += tokens[i] + " ";
                                }
                                NetworkManager.Instance.chatClientTCP.dataSender.SendPrivateChatMessage(newText, player);
                                break;
                            default:
                                break;
                        }
                    }
                    else
                    {
                        NetworkManager.Instance.chatClientTCP.dataSender.SendChatMessage(chatBox.text);
                    }
                   
                    //SendMessageToChat(chatBox.text);
                    
                }
            }
            else
            {
                if (!chatBox.isFocused && Input.GetKeyDown(KeyCode.Return))
                {
                    chatBox.ActivateInputField();
                }
            }
            QueuedMessage msg;
            if (chatMessages.TryDequeue(out msg))
            {

                switch (msg.type)
                {
                    case QueuedMessage.Type.NULL:
                        SendMessageToChat(msg.playerName, msg.message, Color.white);
                        break;
                    case QueuedMessage.Type.TEAM:
                        SendMessageToChat(msg.playerName, msg.message, Color.yellow);
                        break;
                    case QueuedMessage.Type.PRIVATE:
                        SendMessageToChat(msg.playerName, msg.message, Color.red);
                        break;
                    case QueuedMessage.Type.SYSTEM:
                        SendMessageToChat(msg.playerName, msg.message, Color.green);
                        break;
                    default:
                        break;
                }
            }
            if (chatBox.isFocused)
            {
                localPlayer.GetComponent<FirstPersonController>().enabled = false;
            }
            else
            {
                localPlayer.GetComponent<FirstPersonController>().enabled = true;
            }
        }

        public void SendMessageToChat(string text)
        {
            SendMessageToChat("player", text, Color.white);
        }

        //TODO : Recieve from network
        public void SendMessageToChat(string playerName, string text, Color color)
        {
            if (chatHistory.Count >= maxMessages)
            {
                Destroy(chatHistory[0].textObject.gameObject);
                chatHistory.Remove(chatHistory[0]);
            }
            Message newMessage = new Message();
            newMessage.playerName = playerName;
            newMessage.msg = text;
            newMessage.color = color;
            GameObject newText = Instantiate(textObject, chatPanel.transform);
            newMessage.textObject = newText.GetComponent<Text>();
            newMessage.textObject.text = newMessage.playerName + ": " + newMessage.msg;
            newMessage.textObject.color = color;
            chatHistory.Add(newMessage);
        }
    }

    [System.Serializable]
    internal class Message
    {
        internal string playerName;
        internal string msg;
        internal Text textObject;
        internal Color color = Color.white;
    }
}
