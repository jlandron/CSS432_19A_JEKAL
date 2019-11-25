using Common.Protocols;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameClient
{
    public class NetworkManager : MonoBehaviour
    {

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

        [SerializeField]
        private GameObject[] startingPositions;

        public Dictionary<int, GameObject> ConnectedPlayers { get; set; }

        public int NumberConnectedPlayers { get; private set; }

        [SerializeField]
        private GameObject playerPrefab;

        public int PlayerID { get; private set; }
        public string PlayerName { get; private set; }
        //login connection
        public ClientTCP loginClientTCP;
        

        //game connection
        public ClientTCP gameClientTCP;

        //chat connection
        public ClientTCP chatClientTCP;

        internal bool loginSuccess = false;
        
        public bool loginRequestSent = false;
        public bool gameIsLaunched = false;
        public bool chatServerRequestSent = false;

        public static NetworkManager Instance { get; private set; }
        public string ChatServerIP { get => chatServerIP; set => chatServerIP = value; }
        public int ChatServerPort { get => chatServerPort; set => chatServerPort = value; }
        public string GameServerIP { get => gameServerIP; set => gameServerIP = value; }
        public int GameServerPort { get => gameServerPort; set => gameServerPort = value; }

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
        private void Start()
        {
            UnityThread.initUnityThread();
        }
        private void Update()
        {
            //handle player login
            if (loginClientTCP != null && loginClientTCP.IsConnected && !loginSuccess && !loginRequestSent)
            {
                loginRequestSent = true;
                Debug.Log("Sending request to server");
                loginClientTCP.dataSender.RequestLogin();
                
            }else if(loginSuccess && !gameIsLaunched)
            {
                gameIsLaunched = true;
                StartCoroutine(LaunchGame());
                
            }
            if(chatClientTCP != null && chatClientTCP.IsConnected && gameIsLaunched && !chatServerRequestSent)
            {
                chatServerRequestSent = true;
                chatClientTCP.dataSender.RequestJoin();
                
            }
        }
        private IEnumerator LaunchGame()
        {
            yield return SceneManager.LoadSceneAsync("Game");
            StartChatClient();
            //StartGameClient();
        }
        private void OnApplicationQuit()
        {
            if (loginClientTCP != null)
                loginClientTCP.Disconnect();
            if (gameClientTCP != null)
                gameClientTCP.Disconnect();
            if (chatClientTCP != null)
                chatClientTCP.Disconnect();
        }

        public void StartLoginClient(string playerName)
        {
            loginClientTCP = new ClientTCP(ClientTypes.LOGIN);
            loginClientTCP.InitNetworking(LoginServerIP, LoginServerPort);
            PlayerName = playerName;
        }
        public void StartChatClient()
        {
            chatClientTCP = new ClientTCP(ClientTypes.CHAT);
            chatClientTCP.InitNetworking(ChatServerIP, ChatServerPort);
        }

        public void StartGameClient()
        {
            gameClientTCP = new ClientTCP(ClientTypes.GAME);
            gameClientTCP.InitNetworking(GameServerIP, GameServerPort);
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