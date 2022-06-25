using System;
using DuloGames.UI;
using UnityEngine;
using UnityEngine.UI;

namespace TT.UI
{
    [RequireComponent(typeof(Button))]
    public class HideWindowButton : MonoBehaviour
    {
        
        #region Private fields
        
        private Button _button;
        
        #endregion
        
        
        #region Lifecycle events
        
        void Start()
        {
            // Attach event handler
            _button = GetComponent<Button>();
            _button.onClick.AddListener(HandleButtonClicked);
        }

        private void OnDestroy()
        {
            // Remove event handler
            if (_button) _button.onClick.RemoveListener(HandleButtonClicked);
        }
        
        #endregion
        
        
        #region Event handlers

        /// <summary>
        /// Called when the attached button is clicked. Hide the window is button belongs to. 
        /// </summary>
        private void HandleButtonClicked()
        {
            var window = GetComponentInParent<UIWindow>();
            if (window)
                window.Hide();
        }
        
        #endregion
        
    }
}
