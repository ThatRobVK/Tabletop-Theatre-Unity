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
using TT.InputMapping;
using UnityEngine;
using UnityEngine.UI;

namespace TT.UI.MapEditor.GameMenu
{
    /// <summary>
    /// Attached to the Save As button in the map editor game menu. Shows the Save As window.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class SaveAsButton : MonoBehaviour
    {
        
        #region Editor fields
        
        [SerializeField][Tooltip("The window to show for saving as.")] private UIWindow saveAsWindow;
        
        #endregion
        
        
        #region Private fields

        private Button _button;
        
        #endregion
        
        
        #region Lifecycle events
        
        private void Start()
        {
            // Listen for events
            _button = GetComponent<Button>();
            _button.onClick.AddListener(HandleButtonClick);
        }

        private void Update()
        {
            // Call save based on key binding
            if (InputMapper.Current.GeneralInput.SaveAs) HandleButtonClick();
        }

        private void OnDestroy()
        {
            // Stop listening for events
            if (_button != null)
                _button.onClick.RemoveListener(HandleButtonClick);
        }
        
        #endregion
        
        
        #region Event handlers

        /// <summary>
        /// Called when the button is clicked or the key binding is pressed. Show the save as window.
        /// </summary>
        private void HandleButtonClick()
        {
            // Show the save as window
            if (!saveAsWindow.IsOpen)
                saveAsWindow.Show();
        }
        
        #endregion
        
    }
}
