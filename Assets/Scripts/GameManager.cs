using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private int _currentScene;

    private void Start()
    {
        Screen.SetResolution(1920, 1080, false);
        _currentScene = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadSceneAsync(1);
    }

}
