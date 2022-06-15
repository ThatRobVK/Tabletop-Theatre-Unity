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

using System;
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
        private Button _button;
        private UIWindow _window;
        [SerializeField][Tooltip("The textbox containing the username.")] private Textbox username;
        [SerializeField][Tooltip("The textbox containing the password.")] private Textbox password;
        [SerializeField][Tooltip("The panel overlay to tell people to wait during login.")] private GameObject waitPanel;
        [SerializeField][Tooltip("The area where error messages are shown.")] private GameObject errorArea;
        [SerializeField][Tooltip("The text field where error messages are shown.")] private TMP_Text errorText;

        private Dictionary<LoginFailureReason, string> failureReasonToTextMap = new Dictionary<LoginFailureReason, string>()
        {
            {LoginFailureReason.AuthenticationFailed, "Login failed. Check your credentials and try again."},
            {
                LoginFailureReason.ConnectionError,
                "Could not connect to server. Please try again later or check the website for server status."
            },
            {
                LoginFailureReason.EmailNotVerified,
                "E-mail address not verified. Check your e-mail or go to https://www.tabletop-theatre.com/verify-email/"
            }
        };

        void OnEnable()
        {
            _button = GetComponent<Button>();
            _window = GetComponentInParent<UIWindow>();

            _button.onClick.AddListener(OnButtonClick);
        }

        private void OnDisable()
        {
            if (_button)
                _button.onClick.RemoveListener(OnButtonClick);
        }
        
        private void OnButtonClick()
        {
            // Show the wait panel
            waitPanel.SetActive(true);

            // Listen for auth events
            Helpers.Comms.User.OnLoginSuccess += HandleLoginSuccess;
            Helpers.Comms.User.OnLoginFailed += HandleLoginFailed;

            // Request the sign in
            Helpers.Comms.User.LoginAsync(username.text, password.text);
        }

        private void HandleLoginFailed(LoginFailureReason reason)
        {
            // Stop listening
            Helpers.Comms.User.OnLoginSuccess -= HandleLoginSuccess;
            Helpers.Comms.User.OnLoginFailed -= HandleLoginFailed;

            // Show the error
            errorText.text = failureReasonToTextMap[reason];
            errorArea.SetActive(true);
            
            // Hide wait panel
            waitPanel.SetActive(false);
        }

        private void HandleLoginSuccess()
        {
            _window.Hide();
        }
    }
}