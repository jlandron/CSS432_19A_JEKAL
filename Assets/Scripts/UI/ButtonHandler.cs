using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace GAME.UI {
    public class ButtonHandler : MonoBehaviour {
        public void GoToAboutOnPress( ) {
            SceneManager.LoadScene( "About" );
        }
        public void GoToGameOnPress( ) {
            SceneManager.LoadScene( "Game" );
        }
        public void GoToLoginOnPress( ) {
            SceneManager.LoadScene( "LoginScreen" );
        }
        public void GoToCreateAccountOnPress( ) {
            SceneManager.LoadScene( "CreateAccount" );
        }
        public void TryCreateAccount( string username, string password ) {

        }
        public void SecureLogin( string username, string password ) {

        }
    }
}
