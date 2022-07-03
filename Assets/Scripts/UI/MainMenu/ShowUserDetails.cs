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
    /// Shows the username and e-mail address in two text elements.
    /// </summary>
    public class ShowUserDetails : MonoBehaviour
    {
        
        #region  Editor fields

        [SerializeField][Tooltip("The label to show the username in.")] private TMP_Text usernameLabel;
        [SerializeField][Tooltip("The label to show the email in.")] private TMP_Text emailLabel;
        
        #endregion
        
        
        #region Lifecycle events

        private void OnEnable()
        {
            ShowText();

            Helpers.Comms.User.OnLoginSuccess += ShowText;
        }

        private void OnDisable()
        {
            if (Helpers.Comms != null && Helpers.Comms.User != null)
            {
                Helpers.Comms.User.OnLoginSuccess -= ShowText;
            }
        }
        
        #endregion
        
        
        #region Private methods

        private void ShowText()
        {
            var user = Helpers.Comms.User;
            usernameLabel.text = user.Username;
            emailLabel.text = user.Email;
        }
        
        #endregion
        
    }
}