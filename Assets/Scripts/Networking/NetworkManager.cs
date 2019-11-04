using UnityEngine;
using GameClient;
using System;
using System.Collections.Generic;

public class NetworkManager : MonoBehaviour {
    public Dictionary<int, GameObject> playerList;
    [SerializeField]
    private string serverIP = "127.0.0.1";
    [SerializeField]
    private int serverPort = 51092;
    [SerializeField]
    private GameObject playerPrefab;
    [SerializeField]
    private GameObject[] startingPositions;

    private void Start( ) {
        playerPrefab = Resources.Load( "prefabs/ExtPlayer" ) as GameObject;
        playerList = new Dictionary<int, GameObject>( );
        UnityThread.initUnityThread( );
        ClientHandleData.InitPackets( );
        ClientTCP.InitNetworking(serverIP, serverPort );
    }
    
    private void OnApplicationQuit( ) {
        ClientTCP.Disconnect( );
    }

    //TODO: add team locations and instantiations
    internal void InstatiatePlayer( int index ) {
        //add spawning locations for teams, more game information needed
        GameObject player = Instantiate( playerPrefab );
        player.name = "Player: " + index;
        player.tag = "ExtPlayer";
        playerList.Add( index, player );
    }

    public void UpdatePlayerLocation( byte[] data ) {
        ByteBuffer buffer = new ByteBuffer( );
        int packetID = buffer.ReadInt( );
        //read position and rotation
        Vector3 position = new Vector3( buffer.ReadFloat( ), buffer.ReadFloat( ), buffer.ReadFloat( ) );
        Quaternion rotation = new Quaternion ( buffer.ReadFloat( ), buffer.ReadFloat( ), buffer.ReadFloat( ), buffer.ReadFloat() );
        //read player connectionID that was appended to packet
        int index = buffer.ReadInt( );
        //update the player
        playerList[ index ].GetComponent<PlayerUpdater>( ).UpdatePlayerTransform(position, rotation );
        buffer.Dispose( );
    }
}