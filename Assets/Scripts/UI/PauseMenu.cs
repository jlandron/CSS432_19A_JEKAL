using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.UI {
    public class PauseMenu : MonoBehaviour {
        public void ClosePauseMenu( ) {
            FindObjectOfType<GameManager>( ).IsPaused = false;
            gameObject.SetActive( false );
        }
        public void QuitGame( ) {
            Application.Quit( );
        }
        public void CloseSessionAndGoToMenu( ) {
            SceneManager.LoadScene( "MainMenu" );
        }
    }
}