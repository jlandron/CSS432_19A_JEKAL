using Common.Protocols;
using Game.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Concurrent;

namespace GameClient
{
    public class NetworkManager : MonoBehaviour
    {

        [Header("Servers")]
        //TODO : make these settable in game menu
        [SerializeField]
        private string LoginServerIP;
        [SerializeField]
        private int LoginServerPort;
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

        [Header("Player")]
        [SerializeField]
        private GameObject[] startingPositions;
        [SerializeField]
        private GameObject playerPrefab;

        [Header("Internal notifications")]
        [SerializeField]
        private bool shouldKillLogin;
        [SerializeField]
        private bool shouldKillChat;
        [SerializeField]
        private bool shouldKillGame;
        [SerializeField]
        public bool loginSuccess = false;
        [SerializeField]
        public bool loginRequestSent = false;
        [SerializeField]
        public bool gameIsLaunched = false;
        [SerializeField]
        public bool chatRequestSent = false;
        [SerializeField]
        public bool gameRequestSent = false;

        public int NumberConnectedPlayers { get; private set; }
        public int PlayerID { get; private set; }
        public string PlayerName { get; private set; }

        public Dictionary<int, GameObject> ConnectedPlayers { get; set; }
        private string errorMessageToPrint = "";
        private GameObject Canvas;

        public static NetworkManager Instance { get; private set; }
        public string ChatServerIP { get => chatServerIP; set => chatServerIP = value; }
        public int ChatServerPort { get => chatServerPort; set => chatServerPort = value; }
        public string GameServerIP { get => gameServerIP; set => gameServerIP = value; }
        public int GameServerPort { get => gameServerPort; set => gameServerPort = value; }
        public bool ShouldKillLogin { get => shouldKillLogin; set => shouldKillLogin = value; }
        public bool ShouldKillChat { get => shouldKillChat; set => shouldKillChat = value; }
        public bool ShouldKillGame { get => shouldKillGame; set => shouldKillGame = value; }

        private void Awake()
        {
            if (Instance != null)
            {
                return;
            }
            ConnectedPlayers = new Dictionary<int, GameObject>();

            NumberConnectedPlayers = 0;
            Instance = this;
        }
        private void Update()
        {
            //handle player login
            if (loginClientTCP != null && loginClientTCP.IsConnected && !loginSuccess && !loginRequestSent)
            {
                loginRequestSent = true;
                //Debug.Log("Sending request to server");
                loginClientTCP.dataSender.RequestLogin();
            }
            else if (loginSuccess && !gameIsLaunched)
            {
                gameIsLaunched = true;
                StartCoroutine(LaunchGame());
            }
            if (chatClientTCP != null && chatClientTCP.IsConnected && gameIsLaunched && !chatRequestSent)
            {
                chatRequestSent = true;
                chatClientTCP.dataSender.RequestJoin();
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

            if (Canvas == null)
            {
                Canvas = GameObject.FindGameObjectWithTag("Canvas");
                if (Canvas != null)
                {
                    Canvas.GetComponent<PrintMessageToTextbox>().WriteMessage(errorMessageToPrint);
                }
            }
        }
        private IEnumerator LaunchGame()
        {
            yield return SceneManager.LoadSceneAsync("Game");
            ShouldKillLogin = true;
            StartChatClient();
            //StartGameClient();
        }

        private void OnApplicationQuit()
        {
            if (loginClientTCP != null)
            {
                loginClientTCP.Disconnect();
            }

            if (gameClientTCP != null)
            {
                gameClientTCP.Disconnect();
            }

            if (chatClientTCP != null)
            {
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
            Destroy(loginClientTCP);
            loginClientTCP = null;
            loginSuccess = false;
            loginRequestSent = false;
        }
        public void StartChatClient()
        {
            if (chatClientTCP != null)
            {
                KillChatTcp();
            }
            chatClientTCP = gameObject.AddComponent<ClientTCP>();
            chatClientTCP.SetType(ClientTypes.CHAT);
            chatClientTCP.InitNetworking(ChatServerIP, ChatServerPort);
        }
        internal void KillChatTcp()
        {
            Destroy(chatClientTCP);
            chatClientTCP = null;
            chatRequestSent = false;
        }

        public void StartGameClient()
        {
            if (gameClientTCP != null)
            {
                KillGameTcp();
            }
            gameClientTCP = gameObject.AddComponent<ClientTCP>();
            gameClientTCP.SetType(ClientTypes.GAME);
            gameClientTCP.InitNetworking(GameServerIP, GameServerPort);
        }

        internal void KillGameTcp()
        {
            Destroy(gameClientTCP);
            gameClientTCP = null;
            gameIsLaunched = false;
            gameRequestSent = false;
        }

        internal void UpdatePlayerLocation(byte[] data)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.Write(data);
            int packetID = buffer.ReadInt();
            int index = buffer.ReadInt();
            buffer.Dispose();
            //update the player
            ConnectedPlayers[index].GetComponent<NetworkPlayer>().ReceiveMovementMessage(data);
        }

        //TODO: add team locations and instantiations
        internal void InstatiatePlayer(int _playerID)
        {
            //add spawning locations for teams, more game information needed
            GameObject player = Instantiate(playerPrefab);
            player.name = "Player: " + _playerID;
            player.tag = "ExtPlayer";
            player.GetComponent<NetworkPlayer>().playerID = _playerID;
            AddPlayerToConnectedPlayers(_playerID, player);
        }

        internal void AddPlayerToConnectedPlayers(int _playerID, GameObject _playerObject)
        {
            if (!ConnectedPlayers.ContainsKey(_playerID))
            {
                ConnectedPlayers.Add(_playerID, _playerObject);
                NumberConnectedPlayers++;
            }
        }

        internal void RemovePlayerFromConnectedPlayers(int _playerID)
        {
            if (ConnectedPlayers.ContainsKey(_playerID))
            {
                ConnectedPlayers.Remove(_playerID);
                NumberConnectedPlayers--;
            }
        }

        internal GameObject[] GetConnectedPlayers()
        {
            return ConnectedPlayers.Values.ToArray();
        }

        internal void SetLocalPlayerID(int _playerID)
        {
            PlayerID = _playerID;
        }

        internal GameObject GetPlayerFromConnectedPlayers(int _playerID)
        {
            if (ConnectedPlayers.ContainsKey(_playerID))
            {
                return ConnectedPlayers[_playerID];
            }
            return null;
        }
    }
}