using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    private static NetworkManager _instance = null;
    private void Awake( ) {
        if( _instance == null ) {
            _instance = this;
            DontDestroyOnLoad( this );
        } else {
            Destroy( this );
        }
    }
}
