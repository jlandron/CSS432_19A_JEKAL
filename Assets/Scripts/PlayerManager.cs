using Common.Protocols;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

namespace NetworkGame
{
    public class PlayerManager : MonoBehaviour
    {

        [SerializeField]
        public Dictionary<int, Player> ConnectedPlayers { get; private set; }
        public static PlayerManager Instance { get; private set; }
        public ConcurrentQueue<byte[]> playersToSpawn { get; private set; }
        public ConcurrentQueue<byte[]> playersJoiningTeam { get; private set; }
        public ConcurrentQueue<byte[]> playersToRemove { get; private set; }
        public ConcurrentQueue<byte[]> playersToUpdate { get; private set; }
        public ConcurrentQueue<byte[]> playersTagged { get; private set; }

        [Header("Player")]
        [SerializeField]
        private GameObject[] startingPositions;
        [SerializeField]
        private GameObject playerPrefab;
        [SerializeField]
        public int NumberConnectedPlayers { get; private set; }
        // Start is called before the first frame update
        void Awake()
        {
            if (Instance != null)
            {
                return;
            }
            ConnectedPlayers = new Dictionary<int, Player>();
            playersToSpawn = new ConcurrentQueue<byte[]>();
            playersJoiningTeam = new ConcurrentQueue<byte[]>();
            playersToRemove = new ConcurrentQueue<byte[]>();
            playersToUpdate = new ConcurrentQueue<byte[]>();

            NumberConnectedPlayers = 0;
        }

        // Update is called once per frame
        void Update()
        {
            byte[] playerToSpawnData;
            if (playersToSpawn.TryDequeue(out playerToSpawnData))
            {
                InstatiatePlayer(playerToSpawnData);
            }
            byte[] playerToJoinTeamData;
            if (playersJoiningTeam.TryDequeue(out playerToJoinTeamData))
            {
                HandleJoinTeam(playerToJoinTeamData);
            }
            byte[] playerToRemoveData;
            if (playersJoiningTeam.TryDequeue(out playerToRemoveData))
            {
                HandleRemovePlayer(playerToRemoveData);
            }

            byte[] updateData;
            if (playersToUpdate.TryDequeue(out updateData))
            {
                UpdateGame(updateData);
            }
        }

        private void HandleRemovePlayer(byte[] data)
        {
            ByteBuffer byteBuffer = new ByteBuffer();
            byteBuffer.Write(data);
            _ = byteBuffer.ReadInt();
            int playerID = byteBuffer.ReadInt();
            RemovePlayerFromConnectedPlayers(playerID);
            byteBuffer.Dispose();
        }
        private void HandleJoinTeam(byte[] data)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.Write(data);
            _ = buffer.ReadInt();
            _ = buffer.ReadString();
            int playerID = buffer.ReadInt();
            int teamNum = buffer.ReadInt();
            buffer.Dispose();
            ConnectedPlayers[playerID].teamNum = teamNum;
        }
        internal void UpdateGame(byte[] data)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.Write(data);
            _ = buffer.ReadInt(); //message type
            UpdateGameTime(buffer.ReadFloat());
            try
            {
                for (int i = 0; i < NumberConnectedPlayers; i++)
                {
                    int playerID = buffer.ReadInt();
                    Vector3 playerPos = new Vector3(buffer.ReadFloat(), buffer.ReadFloat(), buffer.ReadFloat());
                    Quaternion rotation = new Quaternion(buffer.ReadFloat(), buffer.ReadFloat(), buffer.ReadFloat(), buffer.ReadFloat());
                    ConnectedPlayers[playerID].playerObject.GetComponent<Client.NetworkPlayer>().ReceiveMovementMessage(playerPos, rotation, buffer.ReadFloat());
                }
            }
            catch (Exception)
            {

            }
            buffer.Dispose();
        }

        private void UpdateGameTime(float v)
        {
            //throw new NotImplementedException();
        }

        //TODO: add team locations and instantiations
        internal void InstatiatePlayer(byte[] data)
        {
            //add spawning locations for teams, more game information needed
            if (playerPrefab == null)
            {
                Debug.LogError("No player prefab!");
                return;
            }
            ByteBuffer buffer = new ByteBuffer();
            buffer.Write(data);
            _ = buffer.ReadInt();
            string playerName = buffer.ReadString();
            int playerID = buffer.ReadInt();
            buffer.Dispose();
            //make player object
            Player newPlayer = new Player(playerName, playerID);
            //instantiate new player in game world
            GameObject playerObject = Instantiate(playerPrefab);
            //set player atributes
            playerObject.name = "Player: " + playerName;
            playerObject.tag = "ExtPlayer";
            playerObject.GetComponent<Client.NetworkPlayer>().playerID = playerID;

            newPlayer.playerObject = playerObject;
            AddPlayerToConnectedPlayers(newPlayer);
        }

        internal void AddPlayerToConnectedPlayers(Player _player)
        {
            if (!ConnectedPlayers.ContainsKey(_player.playerID))
            {
                ConnectedPlayers.Add(_player.playerID, _player);
                NumberConnectedPlayers++;
            }
        }

        public void RemovePlayerFromConnectedPlayers(int _playerID)
        {
            if (ConnectedPlayers.ContainsKey(_playerID))
            {
                Destroy(ConnectedPlayers[_playerID].playerObject);
                ConnectedPlayers.Remove(_playerID);
                NumberConnectedPlayers--;
            }
        }

        [System.Serializable]
        public class Player
        {
            public string playerName;
            public int playerID;
            public int teamNum;
            internal GameObject playerObject;
            public Player(string pName, int pID)
            {
                playerName = pName;
                playerID = pID;
            }
        }
    }
}
