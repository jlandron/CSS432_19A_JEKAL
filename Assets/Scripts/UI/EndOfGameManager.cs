using Common.Protocols;
using NetworkGame.Client;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace NetworkGame.UI
{
    class EndOfGameManager : MonoBehaviour
    {
        [SerializeField]
        GameObject[] menuObjects;
        [SerializeField]
        GameObject textObject;

        [Header("Panels")]
        [SerializeField]
        GameObject namePanel;
        [SerializeField]
        GameObject tagsPanel;
        [SerializeField]
        GameObject taggedPanel;


        public int WinningTeam { get; private set; }

        public ConcurrentQueue<byte[]> scores;

        private bool _gameEnded = false;
        public static EndOfGameManager Instance { get; private set; }
        private void Awake()
        {
            if (Instance != null)
            {
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            scores = new ConcurrentQueue<byte[]>();
            for (int i = 0; i < menuObjects.Length; i++)
            {
                menuObjects[i].SetActive(false);
            }
        }

        private void Update()
        {
            if (GameManager.Instance.MyGameState == GameManager.GameState.END && !_gameEnded)
            {
                _gameEnded = true;
                for (int i = 0; i < menuObjects.Length; i++)
                {
                    menuObjects[i].SetActive(true);
                }
                Time.timeScale = 0;
                NetworkManager.Instance.EndConnections(1);
            }
            if (_gameEnded)
            {
                byte[] scoreTextData;
                if (scores.TryDequeue(out scoreTextData))
                {
                    ByteBuffer byteBuffer = new ByteBuffer();
                    byteBuffer.Write(scoreTextData);
                    _ = byteBuffer.ReadInt();
                    WinningTeam = byteBuffer.ReadInt();
                    int numPlayers = byteBuffer.ReadInt();
                    for (int i = 0; i < numPlayers; i++)
                    {
                        int playerID = byteBuffer.ReadInt();
                        string playerName = byteBuffer.ReadString();
                        int playerTag = byteBuffer.ReadInt();
                        int playerTagged = byteBuffer.ReadInt();
                        GameObject pn = Instantiate(textObject, namePanel.transform);
                        pn.GetComponent<Text>().text = playerName;
                        GameObject pt = Instantiate(textObject, tagsPanel.transform);
                        pt.GetComponent<Text>().text = "" + playerTag;
                        GameObject ptd = Instantiate(textObject, taggedPanel.transform);
                        ptd.GetComponent<Text>().text = "" + playerTagged;
                    }
                    byteBuffer.Dispose();
                }
            }
        }

        public void GoToMenuOnPress()
        {
            Time.timeScale = 1;
            SceneManager.LoadScene(1);
        }
    }
}
