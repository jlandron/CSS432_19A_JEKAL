using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GAME.UI {
    public class TextInputHandler : MonoBehaviour
    {
        public string _textToHandle;

        public void HandleText(string userName)
        {
            _textToHandle = userName;
            Debug.Log(_textToHandle);
        }
    }
}