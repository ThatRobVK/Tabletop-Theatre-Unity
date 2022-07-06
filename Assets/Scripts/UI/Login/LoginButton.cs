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

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace TT.UI.Login
{
    /// <summary>
    /// Attached to the login button on the login window (see LoginWindow.cs). Logs the user in when clicked.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class LoginButton : MonoBehaviour
    {
        
        #region Editor fields
        
        [SerializeField][Tooltip("The textbox containing the username.")] private Textbox username;
        [SerializeField][Tooltip("The textbox containing the password.")] private Textbox password;
        
        #endregion
        
        
        #region Private fields

        private Button _button;
        
        #endregion
        
        
        #region Lifecycle events

        void Start()
        {
            _button = GetComponent<Button>();

            // Handle button click events
            _button.onClick.AddListener(OnButtonClick);

            // Listen for auth events
            Helpers.Comms.User.OnLogout += HandleLogout;
        }

        private void OnDestroy()
        {
            // Remove event handlers
            if (_button)
                _button.onClick.RemoveListener(OnButtonClick);
            
            // Stop listening for auth events
            if (Helpers.Comms != null && Helpers.Comms.User != null)
                Helpers.Comms.User.OnLogout -= HandleLogout;
        }

        #endregion


        #region Event handlers

        /// <summary>
        /// Called when the login button is clicked. Start the login process.
        /// </summary>
        private void OnButtonClick()
        {
            // Request the sign in
            Helpers.Comms.User.LoginAsync(username.text, password.text).ConfigureAwait(false);
        }

        /// <summary>
        /// Called when the user is logged out. Clear the form.
        /// </summary>
        private void HandleLogout()
        {
            // On logout clear the boxes to stop others logging in later
            username.text = string.Empty;
            password.text = string.Empty;
        }

        #endregion

    }
}