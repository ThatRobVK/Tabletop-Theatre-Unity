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
    /// Disables a button while the user is logged out, and enables it when the user is logged in.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class LoginToggledButton : MonoBehaviour
    {
        
        #region Editor fields
        
        [SerializeField][Tooltip("When the user is logged in, is this button enabled (ticked) or not (unticked)?")] private bool loggedInStatus = true;
        [SerializeField][Tooltip("Besides highlight and press transitions, these game objects will be disabled when the user is logged out, and enabled when the user is logged in.")] private GameObject[] additionalGameObjectsToControl;
        
        #endregion
        
        
        #region Private fields
        
        private UIHighlightTransition[] _highlightTransitions;
        private UIPressTransition[] _pressTransitions;
        private Button _button;
        
        #endregion
        
        
        #region Lifecycle events
        
        private void OnEnable()
        {
            _button = GetComponent<Button>();
            _highlightTransitions = GetComponents<UIHighlightTransition>();
            _pressTransitions = GetComponents<UIPressTransition>();

            // Enable button based on logged in status
            HandleAuthEvents();

            // Hook up auth events
            Helpers.Comms.User.OnLoginSuccess += HandleAuthEvents;
            Helpers.Comms.User.OnLogout += HandleAuthEvents;
        }

        private void OnDisable()
        {
            // Unhook auth events
            if (Helpers.Comms != null && Helpers.Comms.User != null)
            {
                Helpers.Comms.User.OnLoginSuccess -= HandleAuthEvents;
                Helpers.Comms.User.OnLogout -= HandleAuthEvents;
            }
        }
        
        #endregion

        
        #region Event handlers
        
        /// <summary>
        /// Called when the user is logged in or logged out. Toggles the button on or off based on auth state.
        /// </summary>
        private void HandleAuthEvents()
        {
            bool userIsLoggedIn = Helpers.Comms.User.IsLoggedIn;
            _button.interactable = userIsLoggedIn == loggedInStatus;
            foreach (var highlightTransition in _highlightTransitions)
            {
                highlightTransition.enabled = userIsLoggedIn == loggedInStatus;
            }
            foreach (var pressTransition in _pressTransitions)
            {
                pressTransition.enabled = userIsLoggedIn == loggedInStatus;
            }
            foreach (var additionalObject in additionalGameObjectsToControl)
            {
                additionalObject.SetActive(userIsLoggedIn == loggedInStatus);
            }
        }
        
        #endregion
        
    }
}
