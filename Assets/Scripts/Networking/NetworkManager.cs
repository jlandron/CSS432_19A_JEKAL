﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Common.Protocols;

namespace GameClient
{
    public class NetworkManager : MonoBehaviour
    {
        private static NetworkManager _instance;

        //TODO : make these settable in game menu
        [SerializeField]
        private string loginServerIP;
        [SerializeField]
        private int loginServerPort = 51092;

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

        //game connection
        public ClientTCP gameClientTCP;

        //chat connection
        public ClientTCP chatClientTCP;


        public static NetworkManager Instance { get => _instance; private set => _instance = value; }

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

            loginClientTCP = new ClientTCP(ClientTypes.LOGIN);
            gameClientTCP = new ClientTCP(ClientTypes.GAME); //create now and await AUTH from server
            chatClientTCP = new ClientTCP(ClientTypes.CHAT);
        }

        private void OnApplicationQuit()
        {
            loginClientTCP.Disconnect();
            gameClientTCP.Disconnect();
            chatClientTCP.Disconnect();
        }

        public void UpdatePlayerLocation(byte[] data)
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
        public void InstatiatePlayer(int _playerID)
        {
            //add spawning locations for teams, more game information needed
            GameObject player = Instantiate(playerPrefab);
            player.name = "Player: " + _playerID;
            player.tag = "ExtPlayer";
            player.GetComponent<NetworkPlayer>().playerID = _playerID;
            AddPlayerToConnectedPlayers(_playerID, player);
        }

        public void AddPlayerToConnectedPlayers(int _playerID, GameObject _playerObject)
        {
            if (!ConnectedPlayers.ContainsKey(_playerID))
            {
                ConnectedPlayers.Add(_playerID, _playerObject);
                NumberConnectedPlayers++;
            }
        }

        public void RemovePlayerFromConnectedPlayers(int _playerID)
        {
            if (ConnectedPlayers.ContainsKey(_playerID))
            {
                ConnectedPlayers.Remove(_playerID);
                NumberConnectedPlayers--;
            }
        }

        public GameObject[] GetConnectedPlayers()
        {
            return ConnectedPlayers.Values.ToArray();
        }

        public void SetLocalPlayerID(int _playerID)
        {
            PlayerID = _playerID;
        }

        public GameObject GetPlayerFromConnectedPlayers(int _playerID)
        {
            if (ConnectedPlayers.ContainsKey(_playerID))
            {
                return ConnectedPlayers[_playerID];
            }
            return null;
        }
    }
}