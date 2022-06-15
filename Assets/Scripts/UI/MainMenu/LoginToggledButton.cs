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
    [RequireComponent(typeof(Button))]
    public class LoginToggledButton : MonoBehaviour
    {
        [SerializeField][Tooltip("When the user is logged in, is this button enabled (ticked) or not (unticked)?")] private bool loggedInStatus = true;
        
        private UIHighlightTransition[] _highlightTransitions;
        private UIPressTransition[] _pressTransitions;
        private Button _button;
        
        private void OnEnable()
        {
            _button = GetComponent<Button>();
            _highlightTransitions = GetComponents<UIHighlightTransition>();
            _pressTransitions = GetComponents<UIPressTransition>();

            // Enable button based on logged in status
            SetButtonState();

            // Hook up auth events
            Helpers.Comms.User.OnLoginSuccess += SetButtonState;
            Helpers.Comms.User.OnLogout += SetButtonState;
        }

        private void OnDisable()
        {
            // Unhook auth events
            if (Helpers.Comms != null && Helpers.Comms.User != null)
            {
                Helpers.Comms.User.OnLoginSuccess -= SetButtonState;
                Helpers.Comms.User.OnLogout -= SetButtonState;
            }
        }

        private void SetButtonState()
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
        }
    }
}
