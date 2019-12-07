using NetworkGame.Client;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace NetworkGame.UI
{
    public class ButtonHandler : MonoBehaviour
    {
        public void GoToAboutOnPress()
        {
            SceneManager.LoadScene("About");
        }
        public void TryLogin()
        {
            string userName = GetComponent<TextInputHandler>()._userName;
            string serverIP = GetComponent<TextInputHandler>()._serverIP;
            //Debug.Log("Logging on as: " + userName);
            if (NetworkManager.Instance != null)
            {
                NetworkManager.Instance.StartLoginClient(userName, serverIP);
            }
        }
        public void GoToGameOnPress()
        {
            SceneManager.LoadScene("Game");
        }
        public void GoToLoginOnPress()
        {
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
