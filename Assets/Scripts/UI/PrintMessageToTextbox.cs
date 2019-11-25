
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    class PrintMessageToTextbox : MonoBehaviour
    {
        public void WriteMessage(string msg)
        {
            GetComponent<Text>().text = msg;
        }
    }
}
