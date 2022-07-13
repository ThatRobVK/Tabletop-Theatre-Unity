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
using System.Collections.Generic;
using DuloGames.UI;

namespace TT.UI.MainMenu
{
    /// <summary>
    /// Attached to main menu buttons. Shows a window when clicked, hiding all other open windows.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class MainMenuButton : MonoBehaviour
    {
        
        #region Editor fields
        
        [SerializeField][Tooltip("The window to show when this button is clicked.")] private UIWindow window;
        
        #endregion
        
        
        #region Private fields

        private Button _button;
        private static List<UIWindow> _allWindows; // Static so it's common across all instances
        
        #endregion
        
        
        #region Lifecycle events

        void Start()
        {
            // If this is the first button to initialise, get
            if (_allWindows == null)
            {
                _allWindows = UIWindow.GetWindows();
            }

            _button = GetComponent<Button>();
            _button.onClick.AddListener(HandleButtonClick);
        }

        private void OnDestroy()
        {
            if (_button)
                _button.onClick.RemoveListener(HandleButtonClick);
        }
        
        #endregion
        
        
        #region Event handlers

        /// <summary>
        /// Called when the button is clicked. Hide all windows and show the one configured.
        /// </summary>
        private void HandleButtonClick()
        {
            foreach (var loopWindow in _allWindows)
            {
                if (loopWindow == window && !window.IsOpen)
                    loopWindow.Show();
                else if (loopWindow.ID != UIWindowID.GameMenu)
                    loopWindow.Hide();
            }
        }
        
        #endregion
        
    }
}
