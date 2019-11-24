using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DDOL : MonoBehaviour
{
    private static DDOL _instance = null;
    private void Awake( ) {
        if( _instance == null ) {
            _instance = this;
            DontDestroyOnLoad( this.gameObject );
        } else {
            Destroy( this.gameObject );
        }
    }
}
