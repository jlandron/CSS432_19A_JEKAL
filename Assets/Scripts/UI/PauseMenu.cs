using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Game.UI {
    public class PauseMenu : MonoBehaviour {
        [SerializeField]
        RectTransform[] children;
        [SerializeField]
        private bool _isPaused;

        void Start()
        {
            ActivateObjects(false);
        }

        void Update()
        {

            if (Input.GetKeyDown(KeyCode.P))
            {
                if (!_isPaused)
                {
                    ActivateObjects(true);
                }
                else
                {
                    ActivateObjects(false);
                }
            }
            if (_isPaused)
            {
                
            }
        }
        internal void ActivateObjects(bool active)
        {
            foreach (var child in children)
            {
                child.gameObject.SetActive(active);
            }
            _isPaused = active;
        }

        public void ResumeOnClick()
        {
            ActivateObjects(false);
            _isPaused = false;
        }
    }
}