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

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DuloGames.UI;
using TT.CommsLib;
using TT.Shared;
using TT.UI.MapEditor.MainMenu;

namespace TT.UI.Login
{
    /// <summary>
    /// Attached to the login screen. Handles username and password input and interaction with the User object.
    /// </summary>
    [RequireComponent(typeof(UIWindow))]
    public class LoginWindow : MonoBehaviour
    {

        #region Editor fields
        
        [SerializeField] [Tooltip("The message to show to the user explaining why they're being asked to login.")]
        private string guidanceMessage;
        [SerializeField][Tooltip("The textbox to show the guidance message in.")]
        private TMP_Text guidanceTextbox;
        [SerializeField][Tooltip("The textbox containing the username.")] private Textbox username;
        [SerializeField][Tooltip("The textbox containing the password.")] private Textbox password;
        [SerializeField][Tooltip("The panel overlay to tell people to wait during login.")] private GameObject waitPanel;
        [SerializeField][Tooltip("The area where error messages are shown.")] private GameObject errorArea;
        [SerializeField][Tooltip("The text field where error messages are shown.")] private TMP_Text errorTextbox;
        [SerializeField][Tooltip("The cancel button that hides this window.")] private Button cancelButton;
        [SerializeField]
        [Tooltip("When true, the user will be warned they're going back to the main menu when they click cancel.")]
        private bool exitOnCancel;
        [SerializeField][Tooltip("The exit button to use to take the user back to the main menu")]
        private ExitToMenu exitToMenu;
        [SerializeField][Tooltip("When true, this box shows when the user is logged out.")] private bool showOnLogout;
        [SerializeField] [Tooltip("A Toggle that when ticked stores the user's refresh token.")]
        private Toggle rememberMeToggle; 
        
        #endregion
        
        
        #region Private fields

        private UIWindow _window;
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

        #endregion


        #region Lifecycle events
        
        /// <summary>
        /// Initialise the window.
        /// </summary>
        private void OnEnable()
        {
            // Handle window events
            _window = GetComponent<UIWindow>();
            _window.onTransitionBegin.AddListener(HandleWindowTransitionBegin);

            // Handle cancel button clicks
            if (cancelButton) cancelButton.onClick.AddListener(HandleCancelClicked);
            
            // Listen for auth events
            Helpers.Comms.User.OnLoginSuccess += HandleLoginSuccess;
            Helpers.Comms.User.OnLoginFailed += HandleLoginFailed;
            if (showOnLogout) Helpers.Comms.User.OnLogout += ShowWindow;

            // Add a newline and space to the message for spacing reasons
            guidanceMessage = string.IsNullOrEmpty(guidanceMessage)
                ? string.Empty
                : string.Concat(guidanceMessage, "\n ");
            guidanceTextbox.text = guidanceMessage;
            
            // Set the remember me toggle based on prefs
            rememberMeToggle.isOn = !PlayerPrefs.HasKey(Helpers.PrefsRememberMeKey) 
                                    || PlayerPrefs.GetInt(Helpers.PrefsRememberMeKey) != 0;
        }

        private void Start()
        {
            // Don't do anything if the user is already logged in
            if (Helpers.Comms.User.IsLoggedIn)
                return;
            
            // If a refresh token exists in player prefs, attempt to refresh the login
            if (PlayerPrefs.HasKey(Helpers.PrefsEmailKey) && PlayerPrefs.HasKey(Helpers.PrefsRefreshTokenKey))
                RefreshLogin();
        }

        private void OnDisable()
        {
            // Remove window events
            if (_window)
                _window.onTransitionBegin.RemoveListener(HandleWindowTransitionBegin);

            // Remove auth events
            if (Helpers.Comms != null && Helpers.Comms.User != null)
            {
                Helpers.Comms.User.OnLoginSuccess -= HandleLoginSuccess;
                Helpers.Comms.User.OnLoginFailed -= HandleLoginFailed;
                Helpers.Comms.User.OnLogout -= ShowWindow;
            }
        }

        #endregion


        #region Event handlers

        /// <summary>
        /// Called when the Cancel button is clicked on the main login screen. Either prompt the user to exit, or just
        /// hide the login screen.
        /// </summary>
        private void HandleCancelClicked()
        {
            if (exitOnCancel)
            {
                var modalBox = UIModalBoxManager.Instance.Create(this.gameObject);
                modalBox.SetText1("Login required");
                modalBox.SetText2("You must log in to use this feature. You can either log in or return to the main " +
                                    "menu. Warning: Any unsaved data will be lost.");
                modalBox.SetConfirmButtonText("Login");
                modalBox.SetCancelButtonText("Exit");
                modalBox.onCancel.AddListener(HandleExitClicked);
                modalBox.Show();
            }
            else
            {
                // Not exiting, just hide the window
                HideWindow();
            }
        }

        /// <summary>
        /// Called when the Exit button on the modal box is clicked after clicking Cancel. Exit to the main menu.
        /// </summary>
        private void HandleExitClicked()
        {
            exitToMenu.Exit();
        }

        /// <summary>
        /// Called when the window shows or hides. Clear the form on show.
        /// </summary>
        /// <param name="window">The window that is transitioning.</param>
        /// <param name="newState">The new state (shown or hidden).</param>
        /// <param name="instant">Whether the transition is instant or gradual.</param>
        private void HandleWindowTransitionBegin(UIWindow window, UIWindow.VisualState newState, bool instant)
        {
            if (newState == UIWindow.VisualState.Shown)
            {
                // Reset the panel
                username.text = string.Empty;
                password.text = string.Empty;
                waitPanel.SetActive(false);
                errorArea.SetActive(false);
                errorTextbox.text = string.Empty;
            }
        }

        /// <summary>
        /// Called when the user successfully logs in. Hide the login window.
        /// </summary>
        private void HandleLoginSuccess()
        {
            if (rememberMeToggle && rememberMeToggle.isOn)
            {
                // Remember the user's refresh token
                PlayerPrefs.SetString(Helpers.PrefsEmailKey, Helpers.Comms.User.Email);
                PlayerPrefs.SetString(Helpers.PrefsRefreshTokenKey, Helpers.Comms.User.RefreshToken);
            }
            else
            {
                // Remove any stored refresh token
                PlayerPrefs.DeleteKey(Helpers.PrefsEmailKey);
                PlayerPrefs.DeleteKey(Helpers.PrefsRefreshTokenKey);
            }
            // Remember whether the user wanted to be remembered
            PlayerPrefs.SetInt(Helpers.PrefsRememberMeKey, rememberMeToggle.isOn ? 1 : 0);
            PlayerPrefs.Save();

            HideWindow();
        }

        /// <summary>
        /// Called when login fails. Show the failure reason.
        /// </summary>
        /// <param name="reason">The reason the login has failed.</param>
        private void HandleLoginFailed(LoginFailureReason reason)
        {
            // Show the error
            errorTextbox.text = failureReasonToTextMap[reason];
            errorArea.SetActive(true);
            
            // Hide wait panel
            waitPanel.SetActive(false);
        }

        #endregion


        #region Private methods

        /// <summary>
        /// Attempts to refresh the user's login based on the player prefs.
        /// </summary>
        private async void RefreshLogin()
        {
            // Ensure the remember me toggle is on as we're refreshing so want to store the new refresh token
            if (rememberMeToggle) rememberMeToggle.isOn = true;
            
            // Show the window with the wait overlay
            _window.Show();
            waitPanel.SetActive(true);
            
            // Refresh
            await Helpers.Comms.User.RefreshLoginAsync(PlayerPrefs.GetString(Helpers.PrefsEmailKey),
                PlayerPrefs.GetString(Helpers.PrefsRefreshTokenKey));
        }

        /// <summary>
        /// Show the login window.
        /// </summary>
        private void ShowWindow()
        {
            _window.Show();
        }

        /// <summary>
        /// Hide the login window.
        /// </summary>
        private void HideWindow()
        {
            _window.Hide();
        }

        /// <summary>
        /// Shows or hides the wait panel blocking user interaction.
        /// </summary>
        /// <param name="show">True to show, false to hide.</param>
        public void ToggleWaitPanel(bool show)
        {
            // Show the wait panel
            waitPanel.SetActive(show);
        }
        
        #endregion
        
    }
}