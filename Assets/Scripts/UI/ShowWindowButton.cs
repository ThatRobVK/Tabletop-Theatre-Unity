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

namespace TT.UI
{
    [RequireComponent(typeof(Button))]
    public class ShowWindowButton : MonoBehaviour
    {
        
        #region Editor fields
        
        [SerializeField][Tooltip("The window to show when clicked.")] private UIWindow window;
        
        #endregion
        
        
        #region Private fields
        
        private Button _button;
        
        #endregion

        
        #region Lifecycle events

        private void Start()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(HandleButtonClick);
        }

        private void OnDestroy()
        {
            if (_button != null)
                _button.onClick.RemoveListener(HandleButtonClick);
        }

        #endregion


        #region Event handlers

        /// <summary>
        /// Called when the attached button is clicked. Show the window.
        /// </summary>
        private void HandleButtonClick()
        {
            window.Show();
        }

        #endregion

    }
}