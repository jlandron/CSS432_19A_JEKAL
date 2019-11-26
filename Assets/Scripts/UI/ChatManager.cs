using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChatManager : MonoBehaviour
{
    [SerializeField]
    internal List<Message> chatHistory = new List<Message>();
    [SerializeField]
    private int maxMessages = 50;

    [SerializeField]
    GameObject chatPanel;
    [SerializeField]
    GameObject textObject;
    [SerializeField]
    InputField chatBox;

    private string currentMessage = string.Empty;

    private void Update()
    {
        if (chatBox.text != "")
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                //TODO : send to network
                SendMessageToChat(chatBox.text);
                chatBox.text = "";
            }
        }
        else
        {
            if (!chatBox.isFocused && Input.GetKeyDown(KeyCode.Return))
            {
                chatBox.ActivateInputField();
            }
        }
#if UNITY_EDITOR
        if (!chatBox.isFocused)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                SendMessageToChat("You Pressed the Space bar");
                Debug.Log("space");
            }
        }
#endif
    }

    public void SendMessageToChat(string text)
    {
        SendMessageToChat("player", text);
    }

    //TODO : Recieve from network
    public void SendMessageToChat(string playerName, string text)
    {
        if (chatHistory.Count >= maxMessages)
        {
            Destroy(chatHistory[0].textObject.gameObject);
            chatHistory.Remove(chatHistory[0]);
        }
        Message newMessage = new Message();
        newMessage.playerName = playerName;
        newMessage.text = text;
        GameObject newText = Instantiate(textObject, chatPanel.transform);
        newMessage.textObject = newText.GetComponent<Text>();
        newMessage.textObject.text = newMessage.playerName + ": " + newMessage.text;
        chatHistory.Add(newMessage);
    }
}

[System.Serializable]
internal class Message
{
    internal string playerName = "";
    internal string text = "";
    internal Text textObject;
}
