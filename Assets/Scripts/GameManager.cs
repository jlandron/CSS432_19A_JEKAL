using UnityEngine;
using UnityEngine.SceneManagement;

namespace NetworkGame
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField]
        private int _currentScene;

        [SerializeField]
        private GameState gameState;
        public GameState MyGameState { get => gameState; set => gameState = value; }

        public static GameManager Instance { get; private set; }
        public bool AllowPlayerInput { get; internal set; }

        //TODO: DO SOMETHING WITH THIS IN GAME
        public enum GameState
        {
            WAIT = 1,
            START,
            PLAYING,
            END
        }
        private void Awake()
        {
            AllowPlayerInput = true;
            if (Instance != null)
            {
                return;
            }
            Instance = this;
        }
        private void Start()
        {
            Screen.SetResolution(1920, 1080, false);
            _currentScene = SceneManager.GetActiveScene().buildIndex;
            if (_currentScene == 0)
            {
                SceneManager.LoadSceneAsync(1);
            }
#if UNITY_EDITOR
            else
            {
                SceneManager.LoadSceneAsync(2);
            }
#endif
        }
#if UNITY_EDITOR
        void LateUpdate()
        {
            Cursor.visible = true;
        }
#endif

    }
}
