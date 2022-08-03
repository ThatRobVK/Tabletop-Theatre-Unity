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
using TT.Shared;

namespace TT.UI.MainMenu
{
    /// <summary>
    /// Controls the authentication in the main menu to centralise the code and reduce dependencies.
    /// </summary>
    public class AuthController : MonoBehaviour
    {
        #region Editor fields
        
        [SerializeField][Tooltip("The login form.")] private GameObject loggedOutPanel;
        [SerializeField][Tooltip("The user details.")] private GameObject loggedInPanel;
        [SerializeField][Tooltip("The please wait, logging in panel.")] private GameObject loggingInPanel;
        [SerializeField][Tooltip("If toggle is on, refresh token will be stored.")] private Toggle rememberMeToggle;
        [SerializeField][Tooltip("Objects to hide when logged out, and show when logged in.")] private CanvasGroup[] objectsToShowOnLogin;
        
        #endregion


        #region Lifecycle events
        
        private void Start()
        {
            // Listen for auth events
            Helpers.Comms.User.OnLoginStart += HandleLoginStart;
            Helpers.Comms.User.OnLoginSuccess += HandleLoginSuccess;
            Helpers.Comms.User.OnLoginFailed += HandleLoginFailed;
            Helpers.Comms.User.OnLogout += HandleLogout;
            
            // Initialise UI
            ToggleObjects(Helpers.Comms.User.IsLoggedIn, false);
            
            // Set the remember me toggle based on prefs, default to on
            rememberMeToggle.isOn = !PlayerPrefs.HasKey(Helpers.PrefsRememberMeKey) 
                                    || PlayerPrefs.GetInt(Helpers.PrefsRememberMeKey) != 0;

            // If a refresh token exists in player prefs, attempt to refresh the login
            if (PlayerPrefs.HasKey(Helpers.PrefsEmailKey) && PlayerPrefs.HasKey(Helpers.PrefsRefreshTokenKey))
                RefreshLogin();
        }

        private void OnDestroy()
        {
            if (Helpers.Comms != null && Helpers.Comms.User != null)
            {
                // Stop listening for auth events
                Helpers.Comms.User.OnLoginStart -= HandleLoginStart;
                Helpers.Comms.User.OnLoginSuccess -= HandleLoginSuccess;
                Helpers.Comms.User.OnLoginFailed -= HandleLoginFailed;
                Helpers.Comms.User.OnLogout -= HandleLogout;
            }
        }
        
        #endregion
        
        
        #region Event handlers

        /// <summary>
        /// Called when authentication is starting. Show logging in UI and store remember me preference.
        /// </summary>
        private void HandleLoginStart()
        {
            ToggleObjects(false, true);
            
            // Save whether the user wanted to be remembered
            StoreRefreshPreferenceInPlayerPrefs();
        }

        /// <summary>
        /// Called when the user is successfully logged in. Show the logged in UI and store refresh tokens.
        /// </summary>
        private void HandleLoginSuccess()
        {
            Debug.Log("AuthController :: HandleLoginSuccess :: Successfully logged in");

            ToggleObjects(true, false);

            if (rememberMeToggle.isOn)
            {
                StoreRefreshTokensInPlayerPrefs();
            }
            else
            {
                // If user chose not to be remembered, clear previous refresh tokens
                RemoveRefreshTokensFromPlayerPrefs();
            }
        }

        /// <summary>
        /// Called when the user is logged out. Show the login UI.
        /// </summary>
        private void HandleLogout()
        {
            Debug.Log("AuthController :: HandleLogout :: User logged out");

            ToggleObjects(false, false);
        }
        
        /// <summary>
        /// Called when authentication has failed. Show the login UI.
        /// </summary>
        /// <param name="obj"></param>
        private void HandleLoginFailed(LoginFailureReason obj)
        {
            Debug.Log($"AuthController :: HandleLoginFailed :: Login failed: {obj.ToString()}");

            ToggleObjects(false, false);
        }

        #endregion
        
        
        #region Private methods

        /// <summary>
        /// Sets the UI appropriate to the current login state. 
        /// </summary>
        /// <param name="loggedIn">Whether the user is logged in.</param>
        /// <param name="loggingIn">Whether authentication is currently happening.</param>
        private void ToggleObjects(bool loggedIn, bool loggingIn)
        {
            loggedOutPanel.SetActive(!loggedIn && !loggingIn);
            loggingInPanel.SetActive(loggingIn);
            loggedInPanel.SetActive(loggedIn && !loggingIn);

            foreach (var objectToShowOnLogin in objectsToShowOnLogin)
            {
                objectToShowOnLogin.alpha = loggedIn ? 1 : 0;
                objectToShowOnLogin.blocksRaycasts = loggedIn;
            }
        }
        
        /// <summary>
        /// Attempts to refresh the user's login based on the player prefs.
        /// </summary>
        private async void RefreshLogin()
        {
            Debug.Log("AuthController :: RefreshLogin :: Logging in from stored refresh token");
            
            // Ensure the remember me toggle is on as we're refreshing so want to store the new refresh tokens
            if (rememberMeToggle) rememberMeToggle.isOn = true;
            
            // Attempt to login using refresh tokens
            await Helpers.Comms.User.RefreshLoginAsync(PlayerPrefs.GetString(Helpers.PrefsEmailKey),
                PlayerPrefs.GetString(Helpers.PrefsRefreshTokenKey));
        }

        /// <summary>
        /// Stores whether the user ticked the 'remember me' toggle.
        /// </summary>
        private void StoreRefreshPreferenceInPlayerPrefs()
        {
            PlayerPrefs.SetInt(Helpers.PrefsRememberMeKey, rememberMeToggle.isOn ? 1 : 0);
            PlayerPrefs.Save();
        }
        
        /// <summary>
        /// Stores the current refresh tokens in the player prefs.
        /// </summary>
        private void StoreRefreshTokensInPlayerPrefs()
        {
            // Remember the user's refresh token
            PlayerPrefs.SetString(Helpers.PrefsEmailKey, Helpers.Comms.User.Email);
            PlayerPrefs.SetString(Helpers.PrefsRefreshTokenKey, Helpers.Comms.User.RefreshToken);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Removes refresh tokens from player prefs to prevent a refresh on next start.
        /// </summary>
        private void RemoveRefreshTokensFromPlayerPrefs()
        {
            // Remove any stored refresh token
            PlayerPrefs.DeleteKey(Helpers.PrefsEmailKey);
            PlayerPrefs.DeleteKey(Helpers.PrefsRefreshTokenKey);
            PlayerPrefs.Save();
        }
        
        #endregion
        
    }
}
