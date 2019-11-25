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
        //login connection
        public ClientTCP loginClientTCP;
        internal bool loginSuccess = false;

        //game connection
        public ClientTCP gameClientTCP;

        //chat connection
        public ClientTCP chatClientTCP;


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
            if (loginSuccess)
            {
                loginSuccess = false;
                StartCoroutine(LaunchGame());
            }
        }
        private IEnumerator LaunchGame()
        {
            yield return SceneManager.LoadSceneAsync("Game");
            StartChatClient();
            StartGameClient();
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
            if (loginClientTCP.IsConnected)
            {
                loginClientTCP.dataSender.RequestLogin(playerName);
            }
            else
            {
                SceneManager.LoadScene("Game");
            }
        }
        public void StartChatClient()
        {
            gameClientTCP = new ClientTCP(ClientTypes.GAME);
            chatClientTCP.InitNetworking(ChatServerIP, ChatServerPort);
        }

        public void StartGameClient()
        {
            chatClientTCP = new ClientTCP(ClientTypes.CHAT);
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