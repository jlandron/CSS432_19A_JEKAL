using GameClient;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace GAME.UI
{
    public class ButtonHandler : MonoBehaviour
    {
        public void GoToAboutOnPress()
        {
            SceneManager.LoadScene("About");
        }
        public void TryLogin()
        {
            string userName = GetComponent<TextInputHandler>()._textToHandle;
            Debug.Log("Logging on as: " + userName);
            NetworkManager.Instance.StartLoginClient(userName);

        }
        public void GoToGameOnPress()
        {
            //handle login prior to loading scene
            SceneManager.LoadScene("Game");
        }
        public void GoToLoginOnPress()
        {
            //check if in create account and process that before moving back to login
            SceneManager.LoadScene("LoginScreen");
        }
        public void GoToMainMenuOnPress()
        {
            SceneManager.LoadScene("MainMenu");
        }
        public void QuitApplication()
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit( );
#endif
        }
    }
}
