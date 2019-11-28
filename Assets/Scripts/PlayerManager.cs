using Common.Protocols;
using NetworkGame.Client;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NetworkGame
{
    public class PlayerManager : MonoBehaviour
    {

        [SerializeField]
        public Dictionary<int, GameObject> ConnectedPlayers { get; private set; }
        public static PlayerManager Instance { get; private set; }
        public ConcurrentQueue<int> playersToSpawn { get; private set; }
        public ConcurrentQueue<byte[]> playersToUpdate { get; private set; }

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
            ConnectedPlayers = new Dictionary<int, GameObject>();
            playersToSpawn = new ConcurrentQueue<int>();
            playersToUpdate = new ConcurrentQueue<byte[]>();
            NumberConnectedPlayers = 0;
        }

        // Update is called once per frame
        void Update()
        {
            int playerToSpawnID;
            if (playersToSpawn.TryDequeue(out playerToSpawnID))
            {
                InstatiatePlayer(playerToSpawnID);
            }
            
            for(int i = 0; i < NumberConnectedPlayers; i++)
            {
                byte[] playerToUpdate;
                if (playersToUpdate.TryDequeue(out playerToUpdate))
                {
                    UpdatePlayerLocation(playerToUpdate);
                }
            }
            
        }

        internal GameObject GetPlayerFromConnectedPlayers(int _playerID)
        {
            if (ConnectedPlayers.ContainsKey(_playerID))
            {
                return ConnectedPlayers[_playerID];
            }
            return null;
        }
        internal void UpdatePlayerLocation(byte[] data)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.Write(data);
            int packetID = buffer.ReadInt();
            int index = buffer.ReadInt();
            buffer.Dispose();
            //update the player
            ConnectedPlayers[index].GetComponent<Client.NetworkPlayer>().ReceiveMovementMessage(data);
        }

        //TODO: add team locations and instantiations
        internal void InstatiatePlayer(int _playerID)
        {
            //add spawning locations for teams, more game information needed
            if(playerPrefab == null)
            {
                Debug.LogError("No player prefab!");
                return;
            }
            GameObject player = Instantiate(playerPrefab);
            player.name = "Player: " + _playerID;
            player.tag = "ExtPlayer";
            player.GetComponent<Client.NetworkPlayer>().playerID = _playerID;
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
    }
}
