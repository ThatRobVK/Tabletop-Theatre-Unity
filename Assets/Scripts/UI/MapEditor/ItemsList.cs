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

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TT.Shared.World;
using TT.World;

namespace TT.UI.MapEditor
{
    public class ItemsList : MonoBehaviour
    {

        #region Editor fields

        [SerializeField][Tooltip("The Transform that is the parent of the spawned buttons.")] private Transform buttonParentTransform;
        [SerializeField][Tooltip("The prefab to spawn for each of the object buttons.")] private WorldObjectButton buttonPrefab;
        [SerializeField][Tooltip("The Transform that is the parent for unused UI elements.")] private Transform inactiveUIElements;
        [SerializeField][Tooltip("The Textbox the user uses to do a text search.")] private Textbox searchBox;
        [SerializeField][Tooltip("The types this list searches for.")] private List<WorldObjectType> objectTypes = new List<WorldObjectType>();


        #endregion


        #region Private fields

        private readonly List<WorldObjectButton> _buttons = new List<WorldObjectButton>();

        #endregion


        #region Lifecycle events

        void OnEnable()
        {
            // Update every 5 seconds
            UpdateButtons();

            searchBox.onValueChanged.AddListener(UpdateFromSearch);
        }

        void OnDisable()
        {
            searchBox.onValueChanged.RemoveListener(UpdateFromSearch);
        }

        #endregion


        #region Private methods

        /// <summary>
        /// Re-do all the buttons based on the current list.
        /// </summary>
        private void UpdateButtons()
        {
            _buttons.ForEach(x => x.transform.SetParent(inactiveUIElements));


            // Filter based on the toggle
            var filteredObjects = WorldObjectBase.All.Where(x => objectTypes.Contains(x.Type));

            // Filter further based on text input
            if (!string.IsNullOrEmpty(searchBox.text.Trim()))
            {
                filteredObjects = filteredObjects.Where(x => x.name.ToLowerInvariant().Contains(searchBox.text.Trim().ToLowerInvariant()));
            }

            // Sort by name alphabetically
            filteredObjects = filteredObjects.OrderBy(x => x.name).ThenByDescending(x => x.Starred);

            // Show all the filtered, ordered objects in the grid
            foreach (var worldObject in filteredObjects)
            {
                var button = Helpers.GetAvailableButton(buttonPrefab, _buttons, inactiveUIElements);
                button.Initialise(worldObject);
                button.transform.SetParent(buttonParentTransform);
            }
        }

        /// <summary>
        /// Calls the update function to apply the current search filter.
        /// </summary>
        /// <param name="input">Ignored.</param>
        private void UpdateFromSearch(string input)
        {
            UpdateButtons();
        }

       #endregion

    }
}