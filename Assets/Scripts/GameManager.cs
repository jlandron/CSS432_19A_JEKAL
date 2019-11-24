using UnityEngine;
using UnityEngine.SceneManagement;
using Common.Protocols;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private int _currentScene;
    private bool isPaused = false;
    private GAME_STATE state;
    public GameObject pauseMenu = null;

    public bool IsPaused { get => isPaused; set => isPaused = value; }

    private void Start()
    {
        Screen.SetResolution(1920, 1080, false);
        _currentScene = SceneManager.GetActiveScene().buildIndex;
    }

    private enum GAME_STATE
    {
        WAIT = 1,
        LOGIN,
        CONNECTING,
        PLAYING,
    }
    private void Update()
    {
        _currentScene = SceneManager.GetActiveScene().buildIndex;
        switch (_currentScene)
        {
            case 0: //_preload scene
                SceneManager.LoadScene(1);
                state = GAME_STATE.WAIT;
                break;

            case 1: // login scene
                HandleLogin();
                break;

            case 2: // Create Account scene
                HandleCreateAccount();
                break;

            case 3: // Game scene
                HandleGame();
                break;

            default:
                break;
        }
    }

    private void HandleLogin()
    {
        //Set flags in NetworkManager
    }

    private void HandleCreateAccount()
    {
        //Set flags in NetworkManager
    }

    private void HandleGame()
    {
        if (pauseMenu == null)
        {
            pauseMenu = GameObject.FindGameObjectWithTag("PauseMenu");
            pauseMenu.SetActive(false);
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (IsPaused)
            {
                Resume();
            }
            else
            {
                ShowPauseMenu();
            }
        }
    }

    private void ShowPauseMenu()
    {
        IsPaused = true;
        pauseMenu.SetActive(true);
    }

    private void Resume()
    {
        IsPaused = false;
        pauseMenu.SetActive(false);
    }

}
