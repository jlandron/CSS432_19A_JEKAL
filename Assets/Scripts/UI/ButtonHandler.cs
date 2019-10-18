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
            //handle login prior to loading scene
            SceneManager.LoadScene( "Game" );
        }
        public void GoToLoginOnPress( ) {
            //check if in create account and process that before moving back to login
            SceneManager.LoadScene( "LoginScreen" );
        }
        public void GoToCreateAccountOnPress( ) {
            SceneManager.LoadScene( "CreateAccount" );
        }
        public void GoToMainMenuOnPress( ) {
            SceneManager.LoadScene( "MainMenu" );
        }
        public void QuitApplication( ) {
            Application.Quit( );
        }
        public void TryCreateAccount( string username, string password ) {

        }
        public void SecureLogin( string username, string password ) {

        }
    }
}
