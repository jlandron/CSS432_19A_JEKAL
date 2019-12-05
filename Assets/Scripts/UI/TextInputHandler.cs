using UnityEngine;

namespace NetworkGame.UI
{
    public class TextInputHandler : MonoBehaviour
    {
        public string _userName;
        public string _serverIP;

        public void ServerIP(string s)
        {
            _serverIP = s;
        }
        public void UserName(string s)
        {
            _userName = s;
        }

    }
}