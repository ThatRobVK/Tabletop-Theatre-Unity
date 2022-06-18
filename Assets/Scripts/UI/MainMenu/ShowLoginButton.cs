/*
 * Tabletop Theatre
 * Copyright (C) 2020-2022 Robert van Kooten
 * Original source code: https://github.com/ThatRobVK/Tabletop-Theatre
 * License: https://github.com/ThatRobVK/Tabletop-Theatre/blob/main/LICENSE
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 * 
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using DuloGames.UI;
using UnityEngine;
using UnityEngine.UI;

namespace TT.UI.MainMenu
{
    /// <summary>
    /// A button that shows the login window when clicked, and is hidden while the user is logged in.
    /// </summary>
    [RequireComponent(typeof(Button))]
    [RequireComponent(typeof(CanvasGroup))]
    public class ShowLoginButton : MonoBehaviour
    {
        
        #region Editor fields

        [SerializeField][Tooltip("The window with the login form.")] private UIWindow loginWindow;

        #endregion
        
        
        #region Private fields
        
        private Button _button;
        private CanvasGroup _canvasGroup;
        
        #endregion
        
        
        #region Lifecycle events
        
        private void OnEnable()
        {
            _button = GetComponent<Button>();

            // Set initial state
            bool userIsLoggedIn = Helpers.Comms.User.IsLoggedIn;
            _canvasGroup = GetComponent<CanvasGroup>();
            ToggleButton(userIsLoggedIn);

            // Add handlers
            Helpers.Comms.User.OnLoginSuccess += OnLoginSuccess;
            Helpers.Comms.User.OnLogout += OnLogout;
            _button.onClick.AddListener(HandleButtonClick);
        }

        private void OnDisable()
        {
            if (Helpers.Comms != null && Helpers.Comms.User != null)
            {
                // Remove handlers
                Helpers.Comms.User.OnLoginSuccess -= OnLoginSuccess;
                Helpers.Comms.User.OnLogout -= OnLogout;
            }
            
            // Remove button handler
            if (_button)
                _button.onClick.AddListener(HandleButtonClick);
        }
        
        #endregion

        
        #region Event handlers
        
        /// <summary>
        /// Called when the user is logged out. Show the button.
        /// </summary>
        private void OnLogout()
        {
            ToggleButton(false);
        }

        /// <summary>
        /// Called when the user is logged in successfully. Hide the button.
        /// </summary>
        private void OnLoginSuccess()
        {
            ToggleButton(true);
        }

        /// <summary>
        /// Called when the button is clicked. Show the login window.
        /// </summary>
        private void HandleButtonClick()
        {
            if (!loginWindow.IsOpen)
            {
                loginWindow.Show();
            }
        }

        #endregion

        
        #region Private methods
        
        /// <summary>
        /// Hides the button when logged in, shows the button when logged out.
        /// </summary>
        /// <param name="userIsLoggedIn">A boolean indicating whether the user is logged in.</param>
        private void ToggleButton(bool userIsLoggedIn)
        {
            _canvasGroup.alpha = userIsLoggedIn ? 0 : 1;
            _canvasGroup.interactable = !userIsLoggedIn;
            _canvasGroup.blocksRaycasts = !userIsLoggedIn;
        }
        
        #endregion
        
    }
}