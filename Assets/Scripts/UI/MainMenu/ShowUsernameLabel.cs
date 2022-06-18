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

using TMPro;
using UnityEngine;

namespace TT.UI.MainMenu
{
    /// <summary>
    /// Label that shows the currently signed in user name
    /// </summary>
    [RequireComponent(typeof(TMP_Text))]
    public class ShowUsernameLabel : MonoBehaviour
    {
        
        #region  Editor fields
        
        [SerializeField][Tooltip("The text to show in the label when the user is not logged in.")] private string notLoggedInText = "Not Logged In";
        
        #endregion
        
        
        #region Private fields
        
        private TMP_Text _text;
        
        #endregion
        
        
        #region Lifecycle events

        private void OnEnable()
        {
            _text = GetComponent<TMP_Text>();
            ShowText();

            Helpers.Comms.User.OnLoginSuccess += ShowText;
            Helpers.Comms.User.OnLogout += ShowText;
        }

        private void OnDisable()
        {
            if (Helpers.Comms != null && Helpers.Comms.User != null)
            {
                Helpers.Comms.User.OnLoginSuccess -= ShowText;
                Helpers.Comms.User.OnLogout -= ShowText;
            }
        }
        
        #endregion
        
        
        #region Private methods

        private void ShowText()
        {
            var user = Helpers.Comms.User;
            if (user.IsLoggedIn && user.Email.Equals(user.Username)) // User logged in, no username set
            {
                _text.text = user.Email;
            }
            else if (user.IsLoggedIn) // User logged in, username set, show both
            {
                _text.text = string.Format("{0} ({1})", user.Username, user.Email);
            }
            else // User not logged in
            {
                _text.text = notLoggedInText;
            }
        }
        
        #endregion
        
    }
}