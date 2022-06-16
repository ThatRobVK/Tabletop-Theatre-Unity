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
using System.Collections.Generic;
using DuloGames.UI;
using TMPro;
using TT.Shared;
using UnityEngine;
using UnityEngine.UI;

namespace TT.UI.Login
{
    [RequireComponent(typeof(Button))]
    public class LoginButton : MonoBehaviour
    {
        private LoginWindow _loginWindow;
        private Button _button;
        [SerializeField][Tooltip("The textbox containing the username.")] private Textbox username;
        [SerializeField][Tooltip("The textbox containing the password.")] private Textbox password;

        void OnEnable()
        {
            _button = GetComponent<Button>();
            _loginWindow = GetComponentInParent<LoginWindow>();

            _button.onClick.AddListener(OnButtonClick);
        }

        private void OnDisable()
        {
            if (_button)
                _button.onClick.RemoveListener(OnButtonClick);
        }
        
        private void OnButtonClick()
        {
            _loginWindow.ToggleWaitPanel(true);
            StartCoroutine(DoLogin());
        }

        private IEnumerator DoLogin()
        {
            // Let one frame go by to update the UI to avoid race conditions on a very fast fail of the login
            yield return null;

            // Request the sign in
            Helpers.Comms.User.LoginAsync(username.text, password.text).ConfigureAwait(false);
        }
    }
}