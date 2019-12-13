using NetworkGame.UI;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NetworkGame.Client
{
    public class NetworkManager : MonoBehaviour
    {

        [Header("Servers")]
        //TODO : make these settable in game menu
        [SerializeField]
        private string serverIP;
        [SerializeField]
        private int loginServerPort = -1;
        [SerializeField]
        private int chatServerPort;
        [SerializeField]
        private int gameServerPort;
        //login connection
        public ClientTCP loginClientTCP;
        //game connection
        public ClientTCP gameClientTCP;
        //chat connection
        public ClientTCP chatClientTCP;

        [Header("Internal notifications")]
        [SerializeField]
        private bool shouldKillLogin = false;
        [SerializeField]
        private bool shouldKillChat = false;
        [SerializeField]
        private bool shouldKillGame = false;
        [SerializeField]
        private bool loginSuccess = false;
        [SerializeField]
        private bool loginRequestSent = false;
        [SerializeField]
        private bool gameIsLaunched = false;
        [SerializeField]
        private bool chatRequestSent = false;

        [SerializeField]
        private bool gameRequestSent = false;
        [SerializeField]
        private bool chatIsReady = false;
        [SerializeField]
        private bool gameIsReady = false;
        
        public int NumberConnectedPlayers { get; private set; }
        public int PlayerID { get; set; }
        public string PlayerName { get; set; }


        public string errorMessageToPrint = "";
        private GameObject Canvas;

        public static NetworkManager Instance { get; private set; }


        private void Awake()
        {
            if (Instance != null)
            {
                return;
            }
            Instance = this;
            if (ServerIP == null)
            {
                Debug.LogError("Login IP not set");
            }
            if (LoginServerPort == -1)
            {
                Debug.LogError("Login port not set");
            }
            GameIsLaunching = false;
        }
        private void Update()
        {
            if (Canvas == null)
            {
                Canvas = GameObject.FindGameObjectWithTag("Canvas");
            }
            if (Canvas != null && errorMessageToPrint != "")
            {
                try
                {
                    Canvas.GetComponent<PrintMessageToTextbox>().loginErrors.Enqueue(errorMessageToPrint);
                    errorMessageToPrint = "";
                }
                catch (Exception)
                {

                }
            }
        
            if (loginClientTCP != null && loginClientTCP.IsConnected && !LoginSuccess && !LoginRequestSent)
            {
                LoginRequestSent = true;
                //Debug.Log("Sending request to server");
                loginClientTCP.dataSender.RequestLogin();
            }
            if (chatClientTCP != null && chatClientTCP.IsConnected && GameIsLaunched && !ChatRequestSent && ChatIsReady)
            {
                ChatRequestSent = true;
                //Debug.Log("Sending chat request to server");
                chatClientTCP.dataSender.RequestJoin();
            }
            if (gameClientTCP != null && gameClientTCP.IsConnected && GameIsLaunched && !GameRequestSent && GameIsReady)
            {
                GameRequestSent = true;
               //Debug.Log("Sending game request to server");
                gameClientTCP.dataSender.SendGameJoinRequest();
            }
            if (LoginSuccess && !GameIsLaunched && !GameIsLaunching)
            {
                GameIsLaunching = true;
                //Debug.Log("Launching game");
                LaunchGame();
                GameIsLaunched = true;
                Debug.Log("Game is launched");
            }
            if (ShouldKillLogin)
            {
                KillLoginTcp();
                ShouldKillLogin = false;
            }
            if (ShouldKillChat)
            {
                KillChatTcp();
                ShouldKillChat = false;
            }
            if (ShouldKillGame)
            {
                KillGameTcp();
                ShouldKillGame = false;
            }

        }

        internal IEnumerator EndConnections(float timeToWait)
        {
            yield return new WaitForSeconds(timeToWait);
            ShouldKillChat = true;
            ShouldKillGame = true;
        }
        private void LaunchGame()
        {
            ShouldKillLogin = true;
            SceneManager.LoadScene("Game");
            StartGameClient();
            StartChatClient();
        }

        public void StartLoginClient(string playerName, string serverIP)
        {
            ServerIP = serverIP;
            if (loginClientTCP != null)
            {
                KillLoginTcp();
            }
            loginClientTCP = gameObject.AddComponent<ClientTCP>();
            loginClientTCP.Type = ClientTypes.LOGIN;
            PlayerName = playerName;
            loginClientTCP.CustomAwake();
        }
        public void StartChatClient()
        {
            if (chatClientTCP != null)
            {
                KillChatTcp();
            }
            chatClientTCP = gameObject.AddComponent<ClientTCP>();
            chatClientTCP.Type = ClientTypes.CHAT;

            chatClientTCP.CustomAwake();
        }
        public void StartGameClient()
        {
            if (gameClientTCP != null)
            {
                KillGameTcp();
            }
            gameClientTCP = gameObject.AddComponent<ClientTCP>();
            gameClientTCP.Type = ClientTypes.GAME;

            gameClientTCP.CustomAwake();
        }
        internal void KillLoginTcp()
        {
            loginClientTCP.Disconnect();
            Destroy(loginClientTCP);
            loginClientTCP = null;
            LoginSuccess = false;
            LoginRequestSent = false;
        }
        internal void KillChatTcp()
        {
            chatClientTCP.dataSender.SendLeaveMessage();
            chatClientTCP.Disconnect();
            Destroy(chatClientTCP);
            chatClientTCP = null;
            ChatRequestSent = false;
        }
        internal void KillGameTcp()
        {
            gameClientTCP.dataSender.SendGameLeaveMessage();
            gameClientTCP.Disconnect();
            Destroy(gameClientTCP);
            SceneManager.LoadScene(1);
            gameClientTCP = null;
            GameIsLaunched = false;
            GameRequestSent = false;
        }
        internal void SetLocalPlayerID(int _playerID)
        {
            PlayerID = _playerID;
        }

        public int ChatServerPort { get => ChatServerPort1; set => ChatServerPort1 = value; }
        public int GameServerPort { get => GameServerPort1; set => GameServerPort1 = value; }
        public bool ShouldKillLogin { get => shouldKillLogin; set => shouldKillLogin = value; }
        public bool ShouldKillChat { get => shouldKillChat; set => shouldKillChat = value; }
        public bool ShouldKillGame { get => shouldKillGame; set => shouldKillGame = value; }
        public bool LoginSuccess { get => loginSuccess; set => loginSuccess = value; }
        public bool LoginRequestSent { get => loginRequestSent; set => loginRequestSent = value; }
        public bool GameIsLaunched { get => gameIsLaunched; set => gameIsLaunched = value; }
        public bool ChatRequestSent { get => chatRequestSent; set => chatRequestSent = value; }
        public bool GameRequestSent { get => gameRequestSent; set => gameRequestSent = value; }
        public bool ChatIsReady { get => chatIsReady; set => chatIsReady = value; }
        public bool GameIsReady { get => gameIsReady; set => gameIsReady = value; }
        public string ServerIP { get => serverIP; set => serverIP = value; }
        public int LoginServerPort { get => loginServerPort; set => loginServerPort = value; }
        public int ChatServerPort1 { get => chatServerPort; set => chatServerPort = value; }
        public int GameServerPort1 { get => gameServerPort; set => gameServerPort = value; }
        public bool GameIsLaunching { get; private set; }
    }
}