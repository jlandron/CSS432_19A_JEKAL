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
        private string LoginServerIP;
        [SerializeField]
        private int LoginServerPort = -1;
        [SerializeField]
        private string chatServerIP;
        [SerializeField]
        private int chatServerPort;
        [SerializeField]
        private string gameServerIP;
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
        private bool shouldKillLogin;
        [SerializeField]
        private bool shouldKillChat;
        [SerializeField]
        private bool shouldKillGame;
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
            if (LoginServerIP == null)
            {
                Debug.LogError("Login IP not set");
            }
            if (LoginServerPort == -1)
            {
                Debug.LogError("Login port not set");
            }
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
        }
        private void FixedUpdate()
        {
            //handle player login
            if (loginClientTCP != null && loginClientTCP.IsConnected && !LoginSuccess && !LoginRequestSent)
            {
                LoginRequestSent = true;
                //Debug.Log("Sending request to server");
                loginClientTCP.dataSender.RequestLogin();
            }
            else if (LoginSuccess && !GameIsLaunched)
            {
                GameIsLaunched = true;
                StartCoroutine(LaunchGame());
            }
            if (chatClientTCP != null && chatClientTCP.IsConnected && GameIsLaunched && !ChatRequestSent)
            {
                ChatRequestSent = true;
                chatClientTCP.dataSender.RequestJoin();
            }
            if (gameClientTCP != null && gameClientTCP.IsConnected && GameIsLaunched && !GameRequestSent)
            {
                GameRequestSent = true;
                gameClientTCP.dataSender.SendGameJoinRequest();
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
        private IEnumerator LaunchGame()
        {
            yield return SceneManager.LoadSceneAsync("Game");
            ShouldKillLogin = true;
            StartChatClient();
            StartGameClient();
        }

        private void OnApplicationQuit()
        {
            if (loginClientTCP != null)
            {
                loginClientTCP.Disconnect();
            }

            if (gameClientTCP != null)
            {
                gameClientTCP.dataSender.SendGameLeaveMessage();
                gameClientTCP.Disconnect();
            }

            if (chatClientTCP != null)
            {
                chatClientTCP.dataSender.SendLeaveMessage();
                chatClientTCP.Disconnect();
            }
        }
        public void StartLoginClient(string playerName)
        {
            if (loginClientTCP != null)
            {
                KillLoginTcp();
            }
            loginClientTCP = gameObject.AddComponent<ClientTCP>();
            loginClientTCP.SetType(ClientTypes.LOGIN);
            loginClientTCP.InitNetworking(LoginServerIP, LoginServerPort);
            PlayerName = playerName;
        }
        internal void KillLoginTcp()
        {
            loginClientTCP.Disconnect();
            Destroy(loginClientTCP);
            loginClientTCP = null;
            LoginSuccess = false;
            LoginRequestSent = false;
        }
        public void StartChatClient()
        {
            if (chatClientTCP != null)
            {
                KillChatTcp();
            }
            chatClientTCP = gameObject.AddComponent<ClientTCP>();
            chatClientTCP.SetType(ClientTypes.CHAT);
            chatClientTCP.InitNetworking(LoginServerIP, ChatServerPort);
        }
        internal void KillChatTcp()
        {
            chatClientTCP.dataSender.SendLeaveMessage();
            chatClientTCP.Disconnect();
            Destroy(chatClientTCP);
            chatClientTCP = null;
            ChatRequestSent = false;
        }

        public void StartGameClient()
        {
            if (gameClientTCP != null)
            {
                KillGameTcp();
            }
            gameClientTCP = gameObject.AddComponent<ClientTCP>();
            gameClientTCP.SetType(ClientTypes.GAME);
            gameClientTCP.InitNetworking(LoginServerIP, GameServerPort);
        }

        internal void KillGameTcp()
        {
            gameClientTCP.dataSender.SendGameLeaveMessage();
            gameClientTCP.Disconnect();
            Destroy(gameClientTCP);
            gameClientTCP = null;
            GameIsLaunched = false;
            GameRequestSent = false;
        }
        internal void SetLocalPlayerID(int _playerID)
        {
            PlayerID = _playerID;
        }

        public string ChatServerIP { get => chatServerIP; set => chatServerIP = value; }
        public int ChatServerPort { get => chatServerPort; set => chatServerPort = value; }
        public string GameServerIP { get => gameServerIP; set => gameServerIP = value; }
        public int GameServerPort { get => gameServerPort; set => gameServerPort = value; }
        public bool ShouldKillLogin { get => shouldKillLogin; set => shouldKillLogin = value; }
        public bool ShouldKillChat { get => shouldKillChat; set => shouldKillChat = value; }
        public bool ShouldKillGame { get => shouldKillGame; set => shouldKillGame = value; }
        public bool LoginSuccess { get => loginSuccess; set => loginSuccess = value; }
        public bool LoginRequestSent { get => loginRequestSent; set => loginRequestSent = value; }
        public bool GameIsLaunched { get => gameIsLaunched; set => gameIsLaunched = value; }
        public bool ChatRequestSent { get => chatRequestSent; set => chatRequestSent = value; }
        public bool GameRequestSent { get => gameRequestSent; set => gameRequestSent = value; }
    }
}