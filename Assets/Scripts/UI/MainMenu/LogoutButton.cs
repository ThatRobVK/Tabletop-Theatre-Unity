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

using UnityEngine;
using UnityEngine.UI;

namespace TT.UI.MainMenu
{
    /// <summary>
    /// A logout button that shows when logged in, logs the user out when clicked, and hides when logged out.
    /// </summary>
    [RequireComponent(typeof(Button))]
    [RequireComponent(typeof(CanvasGroup))]
    public class LogoutButton : MonoBehaviour
    {
        
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
            _button.onClick.AddListener(HandleButtonClick);
            Helpers.Comms.User.OnLoginSuccess += HandleLoginSuccess;
            Helpers.Comms.User.OnLogout += HandleLogout;
        }
        
        private void OnDisable()
        {
            if (Helpers.Comms != null && Helpers.Comms.User != null)
            {
                // Remove handlers
                Helpers.Comms.User.OnLoginSuccess -= HandleLoginSuccess;
                Helpers.Comms.User.OnLogout -= HandleLogout;
            }

            // Remove button handler
            if (_button)
                _button.onClick.AddListener(HandleButtonClick);
        }
        
        #endregion
        
        
        #region Event handlers

        /// <summary>
        /// Called when the user is logged out. Hide the button.
        /// </summary>
        private void HandleLogout()
        {
            ToggleButton(false);
        }

        /// <summary>
        /// Called when the user is logged in successfully. Show the button.
        /// </summary>
        private void HandleLoginSuccess()
        {
            ToggleButton(true);
        }

        /// <summary>
        /// Called when the button is clicked. Log the user out.
        /// </summary>
        private void HandleButtonClick()
        {
            if (Helpers.Comms.User.IsLoggedIn)
            {
                Helpers.Comms.User.Logout();
            }
        }

        #endregion


        #region Private methods

        /// <summary>
        /// Shows or hides the button.
        /// </summary>
        /// <param name="userIsLoggedIn">A boolean indicating whether the user is currently logged in or not.</param>
        private void ToggleButton(bool userIsLoggedIn)
        {
            _canvasGroup.alpha = userIsLoggedIn ? 1 : 0;
            _canvasGroup.interactable = userIsLoggedIn;
            _canvasGroup.blocksRaycasts = userIsLoggedIn;
        }

        #endregion

    }
}