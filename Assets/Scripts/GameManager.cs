using UnityEngine;
using UnityEngine.SceneManagement;

namespace NetworkGame
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField]
        private int _currentScene;

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
                //SceneManager.LoadSceneAsync(3);
            }
#endif
        }

    }
}
