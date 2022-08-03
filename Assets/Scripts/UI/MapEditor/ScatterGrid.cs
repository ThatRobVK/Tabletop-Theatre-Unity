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
using UnityEngine;
using UnityEngine.UI;
using TT.Data;

namespace TT.UI.MapEditor
{
    /// <summary>
    /// Attached to the grid that contains scatter items. Populates the grid whenever content is updated.
    /// </summary>
    [RequireComponent(typeof(VerticalLayoutGroup))]
    public class ScatterGrid : MonoBehaviour
    {

        #region Editor fields

        [SerializeField][Tooltip("A prefab for the item replace button.")] private ScatterCategory scatterCategoryPrefab;
        [SerializeField][Tooltip("The parent transform for inactive UI elements.")] private Transform inactiveUIElements;

        #endregion

        
        #region Public properties

        public List<ScatterCategory> ScatterCategories { get; private set; }

        #endregion


        #region Lifecycle events

        private void Start()
        {
            ScatterCategories = new List<ScatterCategory>();
            
            // Listen for content changes and if it's already loaded, initialise
            Content.OnContentChanged += HandleContentChanged;
            if (Content.ContentLoaded) HandleContentChanged();
        }

        #endregion


        #region Event handlers

        /// <summary>
        /// Called when the loaded content has changed. Clear and re-populate the list of categories.
        /// </summary>
        private void HandleContentChanged()
        {
            // Clear the list first
            while (transform.childCount > 0)
            {
                var child = transform.GetChild(0);
                child.gameObject.SetActive(false);
                child.SetParent(inactiveUIElements);
            }

            // Then re-populate
            foreach (var category in Content.Current.Combined.Nature)
            {
                var scatterCategory =
                    Helpers.GetAvailableButton(scatterCategoryPrefab, ScatterCategories, inactiveUIElements);
                scatterCategory.Initialise(category);
                scatterCategory.gameObject.SetActive(true);
                scatterCategory.transform.SetParent(transform);
                ScatterCategories.Add(scatterCategory);
            }
        }

        #endregion

    }
}