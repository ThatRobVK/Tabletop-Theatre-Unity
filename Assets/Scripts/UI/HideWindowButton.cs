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
    /// <summary>
    /// Attached to a button on a window, will close the window when clicked.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class HideWindowButton : MonoBehaviour
    {
        
        #region Private fields
        
        private Button _button;
        
        #endregion
        
        
        #region Lifecycle events
        
        void Start()
        {
            // Attach event handler
            _button = GetComponent<Button>();
            _button.onClick.AddListener(HandleButtonClicked);
        }

        private void OnDestroy()
        {
            // Remove event handler
            if (_button) _button.onClick.RemoveListener(HandleButtonClicked);
        }
        
        #endregion
        
        
        #region Event handlers

        /// <summary>
        /// Called when the attached button is clicked. Hide the window is button belongs to. 
        /// </summary>
        private void HandleButtonClicked()
        {
            var window = GetComponentInParent<UIWindow>();
            if (window)
                window.Hide();
        }
        
        #endregion
        
    }
}
