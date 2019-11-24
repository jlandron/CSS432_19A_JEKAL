using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Common.Protocols;

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
        public string ChatServerIP { get ; set; }
        [SerializeField]
        public int ChatServerPort { get; set ; }
        [SerializeField]
        public string GameServerIP { get; set ; }
        [SerializeField]
        public int GameServerPort { get; set; }

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
            }
        }
        private void OnApplicationQuit()
        {
            loginClientTCP.Disconnect();
            gameClientTCP.Disconnect();
            chatClientTCP.Disconnect();
        }

        public void StartLoginClient(string playerName)
        {
            loginClientTCP = new ClientTCP(ClientTypes.LOGIN);
            loginClientTCP.InitNetworking(LoginServerIP, LoginServerPort);
            loginClientTCP.dataSender.RequestLogin(playerName);
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