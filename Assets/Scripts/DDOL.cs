using UnityEngine;

namespace NetworkGame
{
    public class DDOL : MonoBehaviour
    {
        private static DDOL _instance = null;
        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}
