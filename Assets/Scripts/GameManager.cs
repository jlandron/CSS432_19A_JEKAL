using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {
    [SerializeField]
    private int _currentScene;
    private bool isPaused = false;
    public GameObject pauseMenu = null;

    public bool IsPaused { get => isPaused; set => isPaused = value; }

    private void Start( ) {
        Screen.SetResolution( 1600, 900, false );
        _currentScene = SceneManager.GetActiveScene( ).buildIndex;
    }


    private void Update( ) {
        _currentScene = SceneManager.GetActiveScene( ).buildIndex;
        switch( _currentScene ) {
            case 0: //_preload scene
            SceneManager.LoadScene( 1 );
            break;

            case 1: // main menu scene
            break;

            case 2: // login scene
            break;

            case 3: // Create Account scene
            break;

            case 4: // Game scene
            HandleGame( );
            break;

            default:
            break;
        }
    }

    private void HandleGame( ) {
        if(pauseMenu == null ) {
            pauseMenu = GameObject.FindGameObjectWithTag("PauseMenu");
            pauseMenu.SetActive( false );
        }
        if( Input.GetKeyDown( KeyCode.Escape ) ) {
            if( IsPaused ) {
                Resume( );
            } else {
                ShowPauseMenu( );
            }
        }
    }

    private void ShowPauseMenu( ) {
        IsPaused = true;
        pauseMenu.SetActive( true );
    }

    private void Resume( ) {
        IsPaused = false;
        pauseMenu.SetActive( false );
    }
    
}
