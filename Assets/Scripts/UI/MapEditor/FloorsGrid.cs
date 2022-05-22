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

#pragma warning disable IDE0090 // "Simplify new expression" - implicit object creation is not supported in the .NET version used by Unity 2020.3

using System.Collections.Generic;
using TT.Data;
using UnityEngine;
using UnityEngine.UI;

namespace TT.UI.MapEditor
{
    [RequireComponent(typeof(GridLayoutGroup))]
    [RequireComponent(typeof(ToggleGroup))]
    public class FloorsGrid : MonoBehaviour
    {

        #region Editor fields
#pragma warning disable IDE0044 // Make fields read-only

        [SerializeField][Tooltip("A prefab for the item replace button.")] private ItemReplaceButton replaceButtonPrefab;
        [SerializeField][Tooltip("The parent for UI objects that are temporarily not required.")] private Transform inactiveUIElementParent;

#pragma warning restore IDE0044
        #endregion


        #region Private fields

        private readonly List<ItemReplaceButton> _itemButtons = new List<ItemReplaceButton>();

        #endregion


        #region Lifecycle events
#pragma warning disable IDE0051 // Unused members

        void Start()
        {
            var itemGrid = GetComponent<GridLayoutGroup>().transform;
            var toggleGroup = GetComponent<ToggleGroup>();

            // Remove all buttons
            while (itemGrid.childCount > 0)
            {
                var button = itemGrid.GetChild(0);
                button.SetParent(inactiveUIElementParent);
            }

            // Add item buttons based on the content item and the items within it
            for (int i = 0; i < Content.Current.Combined.Construction.Floors.Length; i++)
            {
                // Set up the button
                var button = Helpers.GetAvailableButton(replaceButtonPrefab, _itemButtons, inactiveUIElementParent);
                button.Initialise(Content.Current.Combined.Construction.Floors[i], toggleGroup, itemGrid.transform);
            }
        }

#pragma warning restore IDE0051 // Unused members
        #endregion

    }
}