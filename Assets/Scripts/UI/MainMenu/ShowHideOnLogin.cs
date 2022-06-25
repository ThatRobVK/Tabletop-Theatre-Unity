using UnityEngine;

namespace TT.UI.MainMenu
{
    /// <summary>
    /// Shows or hides the attached CanvasGroup based on whether the user is logged in.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class ShowHideOnLogin : MonoBehaviour
    {

        #region Editor fields
        
        [SerializeField]
        [Tooltip("If on the control will show when logged in and hide when logged out, and vice versa")]
        private bool showWhenLoggedIn = true;

        #endregion
        
        
        #region Private fields
        
        private CanvasGroup _canvasGroup;
        
        #endregion

        
        #region Lifecycle events
        
        private void OnEnable()
        {
            // Set initial state
            _canvasGroup = GetComponent<CanvasGroup>();
            ToggleButton(showWhenLoggedIn == Helpers.Comms.User.IsLoggedIn);

            // Add handlers
            Helpers.Comms.User.OnLoginSuccess += OnLoginSuccess;
            Helpers.Comms.User.OnLogout += OnLogout;
        }

        private void OnDisable()
        {
            if (Helpers.Comms != null && Helpers.Comms.User != null)
            {
                // Remove handlers
                Helpers.Comms.User.OnLoginSuccess -= OnLoginSuccess;
                Helpers.Comms.User.OnLogout -= OnLogout;
            }
        }
        
        #endregion

        
        #region Event handlers

        /// <summary>
        /// Called when the user is logged in successfully. Hide the button.
        /// </summary>
        private void OnLoginSuccess()
        {
            ToggleButton(showWhenLoggedIn);
        }

        /// <summary>
        /// Called when the user is logged out. Show the button.
        /// </summary>
        private void OnLogout()
        {
            ToggleButton(!showWhenLoggedIn);
        }

        #endregion

        
        
        #region Private methods
        
        /// <summary>
        /// Shows or hides the CanvasGroup.
        /// </summary>
        /// <param name="show">A boolean indicating whether the user is logged in.</param>
        private void ToggleButton(bool show)
        {
            _canvasGroup.alpha = show ? 1 : 0;
            _canvasGroup.interactable = show;
            _canvasGroup.blocksRaycasts = show;
        }
        
        #endregion

    }
}
