using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameClient {
    public class NetworkManager : MonoBehaviour {
        private static NetworkManager _instance;

        [SerializeField]
        private string serverIP = "127.0.0.1";
        [SerializeField]
        private int serverPort = 51092;
        [SerializeField]
        private int loginServerPort = 51092;
        [SerializeField]
        private int chatServerPort = 51092;
        [SerializeField]
        private int gameServerPort = 51092;

        [SerializeField]
        private GameObject[] startingPositions;

        public Dictionary<int, GameObject> ConnectedPlayers { get; set; }

        public int NumberConnectedPlayers { get; private set; }

        [SerializeField]
        private GameObject playerPrefab;

        public int PlayerID { get; private set; }

        public static NetworkManager Instance { get => _instance; private set => _instance = value; }

        private void Awake( ) {
            if( Instance != null ) {
                return;
            }
            ConnectedPlayers = new Dictionary<int, GameObject>( );
            NumberConnectedPlayers = 0;
            Instance = this;
        }
        private void Start( ) {
            UnityThread.initUnityThread( );
            ClientHandleData.InitPackets( );
            ClientTCP.InitNetworking( serverIP, serverPort );
        }

        private void OnApplicationQuit( ) {
            ClientTCP.Disconnect( );
        }

        public void UpdatePlayerLocation( byte[] data ) {
            ByteBuffer buffer = new ByteBuffer( );
            buffer.Write( data );
            int packetID = buffer.ReadInt( );
            int index = buffer.ReadInt( );
            buffer.Dispose( );
            //update the player
            ConnectedPlayers[ index ].GetComponent<NetworkPlayer>( ).ReceiveMovementMessage( data );
        }

        //TODO: add team locations and instantiations
        public void InstatiatePlayer( int _playerID ) {
            //add spawning locations for teams, more game information needed
            GameObject player = Instantiate( playerPrefab );
            player.name = "Player: " + _playerID;
            player.tag = "ExtPlayer";
            player.GetComponent<NetworkPlayer>( ).playerID = _playerID;
            AddPlayerToConnectedPlayers( _playerID, player );
        }

        public void AddPlayerToConnectedPlayers( int _playerID, GameObject _playerObject ) {
            if( !ConnectedPlayers.ContainsKey( _playerID ) ) {
                ConnectedPlayers.Add( _playerID, _playerObject );
                NumberConnectedPlayers++;
            }
        }

        public void RemovePlayerFromConnectedPlayers( int _playerID ) {
            if( ConnectedPlayers.ContainsKey( _playerID ) ) {
                ConnectedPlayers.Remove( _playerID );
                NumberConnectedPlayers--;
            }
        }

        public GameObject[] GetConnectedPlayers( ) {
            return ConnectedPlayers.Values.ToArray( );
        }

        public void SetLocalPlayerID( int _playerID ) {
            PlayerID = _playerID;
        }

        public GameObject GetPlayerFromConnectedPlayers( int _playerID ) {
            if( ConnectedPlayers.ContainsKey( _playerID ) ) {
                return ConnectedPlayers[ _playerID ];
            }
            return null;
        }
    }
}