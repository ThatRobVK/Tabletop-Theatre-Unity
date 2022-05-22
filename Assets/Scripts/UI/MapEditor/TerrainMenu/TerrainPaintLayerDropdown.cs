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
using TT.Data;
using TT.State;
using TT.World;
using UnityEngine;

namespace TT.UI.MapEditor.TerrainMenu
{
    [RequireComponent(typeof(UISelectField))]
    public class TerrainPaintLayerDropdown : MonoBehaviour
    {
         #region Editor fields
#pragma warning disable IDE0044 // Make fields read-only

        [SerializeField][Tooltip("The dropdown to control")] private UISelectField selectField;

#pragma warning restore IDE0044
        #endregion


        #region Public properties

        /// <summary>
        /// Returns the currently selected terrain index.
        /// </summary>
        public int SelectedIndex { get => selectField.selectedOptionIndex; }

        #endregion


        #region Lifecycle events
#pragma warning disable IDE0051 // Unused members

        void Start()
        {
            selectField.options.Clear();
            var currentTerrainTexture = GameTerrain.Current.TerrainTextureAddress;
            var currentTerrainIndex = 0;
            for (int i = 0; i < Content.Current.Combined.TerrainLayers.Length; i++)
            {
                selectField.options.Add(Content.Current.Combined.TerrainLayers[i].Name);
                if (Content.Current.Combined.TerrainLayers[i].ID.Equals(currentTerrainTexture))
                {
                    // Find the current terrain texture and select it
                    currentTerrainIndex = i;
                }
            }
            selectField.SelectOptionByIndex(currentTerrainIndex);
            selectField.onChange.AddListener(HandleChange);
        }

#pragma warning restore IDE0051 // Unused members
        #endregion


        #region Event handlers

        /// <summary>
        /// Called when the selection is changed on the dropdown. Change the terrain if currently painting.
        /// </summary>
        /// <param name="index">The selected index.</param>
        /// <param name="text">The display text of the selected item.</param>
        private void HandleChange(int index, string text)
        {
            if (StateController.CurrentStateType == StateType.TerrainPaint)
            {
                // Update if in the paint state (otherwise it will be set when entering paint state)
                ((TerrainPaintState)StateController.CurrentState).PaintLayer = index;
            }
        }

        #endregion

    }
}