using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.UI {
    public class PauseMenu : MonoBehaviour {
        public void ClosePauseMenu( ) {
            gameObject.SetActive( false );
        }
        public void QuitGame( ) {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit( );
#endif
        }
        public void CloseSessionAndGoToMenu( ) {
            SceneManager.LoadScene( "MainMenu" );
        }
    }
}