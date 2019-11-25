
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    class PrintMessageToTextbox : MonoBehaviour
    {
        [SerializeField]
        Text textBox = null;
        public void WriteMessage(string msg)
        {
            textBox.text = msg;
        }
    }
}
