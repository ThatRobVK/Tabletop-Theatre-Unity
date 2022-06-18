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
using System.Threading.Tasks;

namespace TT.Shared
{
    public interface IUser
    {
        
        #region Events
        
        /// <summary>
        /// Invoked when the user is successfully authenticated.
        /// </summary>
        event Action OnLoginSuccess;
        
        /// <summary>
        /// Invoked when the authentication attempt was unsuccessful. 
        /// </summary>
        event Action<LoginFailureReason> OnLoginFailed;
        
        /// <summary>
        /// Invoked when the user is logged out.
        /// </summary>
        event Action OnLogout;
        
        #endregion
        
        
        #region Properties
        
        /// <summary>
        /// A boolean indicating whether the user is currently logged in.
        /// </summary>
        bool IsLoggedIn { get; }

        /// <summary>
        /// The username of the currently logged in user.
        /// </summary>
        string Username { get; }

        /// <summary>
        /// The email address of the currently logged in user.
        /// </summary>
        string Email { get; }
        
        /// <summary>
        /// A refresh token for the currently logged in user.
        /// </summary>
        string RefreshToken { get; }

        #endregion
        
        
        #region Methods
        
        /// <summary>
        /// Attempts to log the user in to the server using the specified credentials. This call is asynchronous and invokes either the OnLoginSuccess or OnLoginFailed event on completion.
        /// </summary>
        /// <param name="email">The e-mail address to sign the user in with.</param>
        /// <param name="password">The password to sign the user in with.</param>
        Task<bool> LoginAsync(string email, string password);

        /// <summary>
        /// Attempts to log the user in to the server using the specified refresh token. This call is asynchronous and invokes either the OnLoginSuccess or OnLoginFailed event on completion.
        /// </summary>
        /// <param name="email">The e-mail address to sign the user in with.</param>
        /// <param name="refreshToken">A refresh token from this user's previous session.</param>
        /// <returns></returns>
        Task<bool> RefreshLoginAsync(string email, string refreshToken);
        
        /// <summary>
        /// Logs the user out. This call will call the OnLogout event on completion.
        /// </summary>
        void Logout();
        
        #endregion
        
    }
}