using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {

    private int _currentScene;

    private void Start( ) {
        _currentScene = SceneManager.GetActiveScene( ).buildIndex;
    }

    
    private void FixedUpdate( ) {
        _currentScene = SceneManager.GetActiveScene( ).buildIndex;
        switch( _currentScene ) {
            case 0: //_preload scene
            SceneManager.LoadScene( 1 );
            break;

            case 1: // main menu scene
            break;

            case 2: // login scene
            break;

            case 3: // Game scene
            break;

            default:
            break;
        }
    }

}
