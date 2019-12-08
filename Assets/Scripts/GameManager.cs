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
        private bool preloaded = false;

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
            
            if (Instance != null)
            {
                return;
            }
            Instance = this;
            AllowPlayerInput = true;
        }
        private void Start()
        {
            Screen.SetResolution(1920, 1080, false);
            MyGameState = GameState.WAIT;
        }
        private void Update()
        {
            _currentScene = SceneManager.GetActiveScene().buildIndex;

            if (_currentScene == 0 && !preloaded)
            {
                preloaded = true;
                SceneManager.LoadScene(1);
            }

        }
#if UNITY_EDITOR
        void LateUpdate()
        {
            Cursor.visible = true;
        }
#endif

    }
}
