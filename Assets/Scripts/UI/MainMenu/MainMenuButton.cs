using System;
using System.Collections.Generic;
using DuloGames.UI;
using UnityEngine;
using UnityEngine.Rendering.VirtualTexturing;
using UnityEngine.UI;

namespace TT.UI.MainMenu
{
    [RequireComponent(typeof(Button))]
    public class MainMenuButton : MonoBehaviour
    {
        [SerializeField] [Tooltip("The custom window id to show when this button is clicked.")]
        private UIWindow window;

        private Button _button;

        // Static so it's common across all instances
        private static List<UIWindow> _allWindows;
        

        void Start()
        {
            // If this is the first button to initialise, get
            if (_allWindows == null)
            {
                _allWindows = UIWindow.GetWindows();
            }

            _button = GetComponent<Button>();
            _button.onClick.AddListener(HandleButtonClick);
        }

        private void OnDestroy()
        {
            if (_button)
                _button.onClick.RemoveListener(HandleButtonClick);
        }

        private void HandleButtonClick()
        {
            foreach (var loopWindow in _allWindows)
            {
                if (loopWindow == window && !window.IsOpen)
                    loopWindow.Show();
                else if (loopWindow.ID != UIWindowID.GameMenu)
                    loopWindow.Hide();
            }
        }
    }
}
