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

using TT.Data;
using TT.UI.GameContent;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TT.UI.Load
{
    /// <summary>
    /// Attached to the Load button in the Load window. Loads the selected map when clicked.
    /// </summary>
    [RequireComponent(typeof(Button))]
    [RequireComponent(typeof(ToggledButton))]
    public class LoadMapButton : MonoBehaviour
    {
        
        #region Editor fields
        
        [SerializeField][Tooltip("The list exposing the selected map.")] private MapList mapList;
        [SerializeField][Tooltip("The loading screen to show.")] private LoadingScreen loadingScreen;
        
        #endregion
        
        
        #region Private fields

        private Button _button;
        private ToggledButton _toggledButton;
        
        #endregion
        
        
        #region Lifecycle events

        private void Start()
        {
            // Listen for button click events
            _button = GetComponent<Button>();
            _button.onClick.AddListener(HandleButtonClick);

            _toggledButton = GetComponent<ToggledButton>();
        }
        
        

        private void OnDestroy()
        {
            // Stop listening for button click events
            if (_button != null)
                _button.onClick.RemoveListener(HandleButtonClick);
        }
        
        #endregion
        
        
        #region Private methods

        /// <summary>
        /// Called when the attached button is clicked. Load the selected map.
        /// </summary>
        private void HandleButtonClick()
        {
            // Don't act if the button is toggled off
            if (!_toggledButton.Enabled)
                return;
            
            // Load the selected map
            loadingScreen.LoadAndRender(true, mapList.SelectedMap.id, Helpers.MapEditorSceneName, true);
        }
        
        #endregion
        
    }
}