using UnityEngine;
using GameClient;

public class NetworkManager : MonoBehaviour {
    private void Start( ) {
        UnityThread.initUnityThread( );
        ClientHandleData.InitPackets( );
        ClientTCP.InitNetworking( );
    }

    private void OnApplicationQuit( ) {
        ClientTCP.Disconnect( );
    }
}