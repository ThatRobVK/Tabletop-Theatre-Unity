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
    [RequireComponent(typeof(CanvasGroup))]
    public class ShowLoginButton : MonoBehaviour
    {
        [SerializeField][Tooltip("The window with the login form.")] private UIWindow loginWindow;
        private Button _button;
        private CanvasGroup _canvasGroup;
        
        private void OnEnable()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(HandleButtonClick);

            bool userIsLoggedIn = Helpers.Comms.User.IsLoggedIn;
            _canvasGroup = GetComponent<CanvasGroup>();
            ToggleButton(userIsLoggedIn);

            // Add handlers
            Helpers.Comms.User.OnLoginSuccess += OnUserOnOnLoginSuccess;
            Helpers.Comms.User.OnLogout += OnUserOnOnLogout;
        }

        private void OnDisable()
        {
            // Remove handlers
            Helpers.Comms.User.OnLoginSuccess -= OnUserOnOnLoginSuccess;
            Helpers.Comms.User.OnLogout -= OnUserOnOnLogout;
        }

        private void OnUserOnOnLogout()
        {
            ToggleButton(false);
        }

        private void OnUserOnOnLoginSuccess()
        {
            ToggleButton(true);
        }

        private void ToggleButton(bool userIsLoggedIn)
        {
            _canvasGroup.alpha = userIsLoggedIn ? 0 : 1;
            _canvasGroup.interactable = !userIsLoggedIn;
        }

        private void HandleButtonClick()
        {
            if (!loginWindow.IsOpen)
            {
                loginWindow.Show();
            }
        }
    }
}