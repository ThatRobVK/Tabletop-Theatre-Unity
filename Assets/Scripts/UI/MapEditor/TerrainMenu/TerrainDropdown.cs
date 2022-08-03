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
using DuloGames.UI;
using TT.Data;
using TT.World;

namespace TT.UI.MapEditor.TerrainMenu
{
    /// <summary>
    /// Attached to the terrain texture dropdown in the terrain menu. Changes the terrain texture.
    /// </summary>
    [RequireComponent(typeof(UISelectField))]
    public class TerrainDropdown : MonoBehaviour
    {
        #region Private fields
        
        private UIWindow _window;
        private UISelectField _selectField;
        
        #endregion
        
        
        #region Lifecycle events

        private void Start()
        {
            _selectField = GetComponent<UISelectField>();
            _window = GetComponentInParent<UIWindow>();
         
            // Listen for window events
            _window.onTransitionBegin.AddListener(HandleWindowTransitionBegin);
        }

        private void OnDestroy()
        {
            // Stop listening for window events
            if (_window != null)
                _window.onTransitionBegin.RemoveListener(HandleWindowTransitionBegin);
        }
        
        #endregion
        
        
        #region Event handlers

        /// <summary>
        /// Called when the window starts to show or hide. Load the terrain layers and select the current one.
        /// </summary>
        /// <param name="window">The window that is transitioning.</param>
        /// <param name="targetState">The state it is transitioning to.</param>
        /// <param name="instant">Whether the transition is instant.</param>
        private void HandleWindowTransitionBegin(UIWindow window, UIWindow.VisualState targetState, bool instant)
        {
            if (targetState == UIWindow.VisualState.Shown)
            {
                // Add each terrain to the drop down
                _selectField.options.Clear();
                var selectedIndex = 0;
                for (int i = 0; i < Content.Current.Combined.TerrainLayers.Length; i++)
                {
                    var terrainAddress = Content.Current.Combined.TerrainLayers[i].ID;
                    _selectField.AddOption(Content.Current.Combined.TerrainLayers[i].Name);
                    // Track which index is the currently selected terrain
                    if (GameTerrain.Current.TerrainTextureAddress.Equals(terrainAddress)) selectedIndex = i;
                }

                // Select the option that relates to the current terrain
                _selectField.SelectOptionByIndex(selectedIndex);
                _selectField.onChange.AddListener(HandleChange);
            }
            else
            {
                _selectField.onChange.RemoveListener(HandleChange);
            }
        }

        /// <summary>
        /// Called when the dropdown selection changes. Update the terrain.
        /// </summary>
        /// <param name="selectedIndex">The index of the new selection.</param>
        /// <param name="selectedValue">The display value of the selection.</param>
        private void HandleChange(int selectedIndex, string selectedValue)
        {
            var selectedAddress = Content.Current.Combined.TerrainLayers[selectedIndex].ID;
            if (!GameTerrain.Current.TerrainTextureAddress.Equals(selectedAddress))
            {
                GameTerrain.Current.ReplaceTerrainTexture(selectedAddress, 0).ConfigureAwait(false);
            }
        }
        
        #endregion

    }
}