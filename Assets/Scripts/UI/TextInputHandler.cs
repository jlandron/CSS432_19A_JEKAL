using UnityEngine;

namespace NetworkGame.UI
{
    public class TextInputHandler : MonoBehaviour
    {
        public string _textToHandle;

        public void HandleText(string userName)
        {
            _textToHandle = userName;
        }
    }
}