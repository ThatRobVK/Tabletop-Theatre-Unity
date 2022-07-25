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
using TT.CommsLib;
using TT.Shared;
using TT.Shared.UserContent;

namespace TT.Data
{
    /// <summary>
    /// Object used to access classes in the external CommsLib. Exposed through the SettingsObject.
    /// </summary>
    public class CommsObject : MonoBehaviour
    {
        
        #region Private fields
        
        private IUser _user;
        private IUserContent _userContent;
        private IGameContent _gameContent;

        #endregion

        
        #region Lifecycle events

        private void OnApplicationQuit()
        {
            // Log out before quitting
            _user?.Logout();
        }
        
        #endregion


        #region Public properties

        /// <summary>
        /// Authentication 
        /// </summary>
        public IUser User => _user ??= new User(Application.version);

        /// <summary>
        /// User generated content
        /// </summary>
        public IUserContent UserContent => _userContent ??= new UserContent(Application.version);

        /// <summary>
        /// Content items provided by TT
        /// </summary>
        public IGameContent GameContent => _gameContent ??= new GameContent(Application.version);

        #endregion

    }
}