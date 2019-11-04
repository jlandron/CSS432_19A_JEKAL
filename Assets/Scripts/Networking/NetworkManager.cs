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
    private GameObject Player;
    private bool havePlayer = false;


    private void Start( ) {
        playerList = new Dictionary<int, GameObject>( );
        UnityThread.initUnityThread( );
        ClientHandleData.InitPackets( );
        ClientTCP.InitNetworking(serverIP, serverPort );
    }
    private void FixedUpdate( ) {
        if( !havePlayer ) {
            Player = GameObject.FindGameObjectWithTag( "Player" );
            if( Player != null ) {
                havePlayer = true;
            }
        } else {
            //send my player data to server
            DataSender.SendTransformMessage( Player.transform.position.x,
                Player.transform.position.y, Player.transform.position.z,
                Player.transform.rotation.x, Player.transform.rotation.y,
                Player.transform.rotation.z );
            Debug.Log( "SendTransformData Called" );
        }
    }
    private void OnApplicationQuit( ) {
        ClientTCP.Disconnect( );
    }

    internal void InstatiatePlayer( int index ) {
        //add spawning locations for teams, more game information needed
        GameObject player = Instantiate( playerPrefab );
        player.name = "Player: " + index;
        player.tag = "ExtPlayer";
        playerList.Add( index, player );
    }

    internal void UpdatePlayerLocation( byte[] data ) {
        ByteBuffer buffer = new ByteBuffer( );
        int packetID = buffer.ReadInt( );
        Vector3 position = new Vector3( buffer.ReadFloat( ), buffer.ReadFloat( ), buffer.ReadFloat( ) );
        Vector3 rotation = new Vector3( buffer.ReadFloat( ), buffer.ReadFloat( ), buffer.ReadFloat( ) );


        buffer.Dispose( );
    }
}