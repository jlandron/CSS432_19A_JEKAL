using UnityEngine;

public class GameManager : MonoBehaviour {

    private static GameManager _instance = null;
    private void Awake( ) {
        if(_instance == null ) {
            _instance = this;
            DontDestroyOnLoad( this );
        } else {
            Destroy( this );
        }
    }
}
