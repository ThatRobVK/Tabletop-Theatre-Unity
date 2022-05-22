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
using TT.World;
using UnityEngine;

namespace TT.UI.MapEditor.TerrainMenu
{
    [RequireComponent(typeof(UISelectField))]
    public class TerrainDropdown : MonoBehaviour
    {
        private UISelectField _selectField;

        // Start is called before the first frame update
        private void Start()
        {
            _selectField = GetComponent<UISelectField>();

            // Add each terrain to the drop down
            _selectField.options.Clear();
            var selectedIndex = 0;
            for (int i = 0; i < Content.Current.Combined.TerrainLayers.Length; i++)
            {
                var terrainAddress = Content.Current.Combined.TerrainLayers[i].ID;
                _selectField.AddOption(Content.Current.Combined.TerrainLayers[i].Name);
                // Track which index is the currently selected terrain
                if (!string.IsNullOrEmpty(GameTerrain.Current.TerrainTextureAddress) && GameTerrain.Current.TerrainTextureAddress.Equals(terrainAddress)) selectedIndex = i;
            }

            // Select the option that relates to the current terrain
            _selectField.SelectOptionByIndex(selectedIndex);

            _selectField.onChange.AddListener(HandleChange);
        }

        private void HandleChange(int selectedIndex, string selectedValue)
        {
            var selectedAddress = Content.Current.Combined.TerrainLayers[selectedIndex].ID;
            if (!GameTerrain.Current.TerrainTextureAddress.Equals(selectedAddress))
            {
                GameTerrain.Current.LoadTerrainTexture(selectedAddress, 0);
            }
        }

    }
}