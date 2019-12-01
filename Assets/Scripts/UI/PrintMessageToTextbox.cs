using System.Collections.Concurrent;
using UnityEngine;
using UnityEngine.UI;

namespace NetworkGame.UI
{
    class PrintMessageToTextbox : MonoBehaviour
    {
        public ConcurrentQueue<string> loginErrors;
        private string textToShow = "";
        [SerializeField]
        Text textBox = null;

        private void Start()
        {
            loginErrors = new ConcurrentQueue<string>();
        }
        void Update()
        {
            string tmp;
            if(loginErrors.TryDequeue(out tmp))
            {
                textToShow = tmp;
            }
            textBox.color = Color.white;
            textBox.text = textToShow;
        }
    }
}
