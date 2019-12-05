using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace NetworkGame.UI {
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

            if (Input.GetKeyDown(KeyCode.Escape))
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
        }
        internal void ActivateObjects(bool active)
        {
            foreach (var child in children)
            {
                child.gameObject.SetActive(active);
            }
            GameManager.Instance.AllowPlayerInput = !active;
            _isPaused = active;
        }

        public void ResumeOnClick()
        {
            ActivateObjects(false);
            _isPaused = false;
        }
    }
}