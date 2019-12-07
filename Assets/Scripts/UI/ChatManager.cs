﻿using NetworkGame.Client;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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


        [Header("Chat colors")]
        [SerializeField]
        private Color regularMessageColor = Color.white;
        [SerializeField]
        private Color systemMessageColor = Color.green;
        [SerializeField]
        private Color privateMessageColor = Color.red;
        [SerializeField]
        private Color teamMessageColor = Color.yellow;

        private bool chatInputEnabled = false;

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
            //if you are typing in chat and press enter, parse and send
            if (chatBox.text.Trim() != "")
            {
                if (Input.GetKeyDown(KeyCode.Return))
                {
                    if (GameManager.Instance != null)
                        GameManager.Instance.AllowPlayerInput = true;
                    string text = chatBox.text;
                    chatBox.text = "";
                    chatBox.enabled = false;
                    chatInputEnabled = false;
                    if (text.StartsWith("/"))
                    {
                        Debug.Log("Parsing special message");
                        string[] tokens = text.Split(' ');
                        string newText = "";

                        switch (tokens[0])
                        {
                            case "/t":
                            case "/T":
                                for (int i = 1; i < tokens.Length; i++)
                                {
                                    newText += tokens[i] + " ";
                                }
                                Debug.Log(newText);
                                if (NetworkManager.Instance != null)
                                {
                                    NetworkManager.Instance.chatClientTCP.dataSender.SendTeamChatMessage(newText);
                                }

                                break;
                            case "/p":
                            case "/P":
                                string player = tokens[1];
                                for (int i = 2; i < tokens.Length; i++)
                                {
                                    newText += tokens[i] + " ";
                                }
                                if (NetworkManager.Instance != null)
                                {
                                    NetworkManager.Instance.chatClientTCP.dataSender.SendPrivateChatMessage(newText, player);
                                }

                                break;
                            default:
                                break;
                        }
                    }
                    else
                    {
                        if (NetworkManager.Instance != null)
                        {
                            NetworkManager.Instance.chatClientTCP.dataSender.SendChatMessage(text);
                        }
                    }
                }
            }
            if (Input.GetKeyDown(KeyCode.Return))
            {
                //if you press enter, and the chatbox is empty and you had pressed enter before leave the chatbox alone and go back to the game
                if (chatInputEnabled && chatBox.text.Trim() == "")
                {
                    chatInputEnabled = false;
                    chatBox.enabled = false;
                    if (GameManager.Instance != null)
                        GameManager.Instance.AllowPlayerInput = true;
                }
                else//if you press enter, and the chatbox is empty, and your not typing, enable the chatbox
                {
                    chatInputEnabled = true;
                    chatBox.enabled = true;
                    chatBox.ActivateInputField();
                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.AllowPlayerInput = false;
                    }
                }
            }

            //parse messages in queue
            QueuedMessage msg;
            if (chatMessages.TryDequeue(out msg))
            {

                switch (msg.type)
                {
                    case QueuedMessage.Type.NULL:
                        SendMessageToChat(msg.playerName, msg.message, regularMessageColor);
                        break;
                    case QueuedMessage.Type.TEAM:
                        SendMessageToChat(msg.playerName, msg.message, teamMessageColor);
                        break;
                    case QueuedMessage.Type.PRIVATE:
                        SendMessageToChat(msg.playerName, msg.message, privateMessageColor);
                        break;
                    case QueuedMessage.Type.SYSTEM:
                        SendMessageToChat(msg.playerName, msg.message, systemMessageColor);
                        break;
                    default:
                        break;
                }
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
