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
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace TT.UI.MapEditor
{
    [RequireComponent(typeof(VerticalLayoutGroup))]
    public class ScatterGrid : MonoBehaviour
    {

        #region Editor fields
#pragma warning disable IDE0044 // Make fields read-only

        [SerializeField][Tooltip("A prefab for the item replace button.")] private ScatterCategory scatterCategoryPrefab;

#pragma warning restore IDE0044
        #endregion


        #region Private fields

        private bool _initialised;

        #endregion


        #region Public properties

        [FormerlySerializedAs("ScatterCategories")] public List<ScatterCategory> scatterCategories = new List<ScatterCategory>();

        #endregion


        #region Lifecycle events
#pragma warning disable IDE0051 // Unused members

        void Start()
        {
            Content.OnContentChanged += HandleContentChanged;

            if (Content.ContentLoaded) HandleContentChanged();
        }

#pragma warning restore IDE0051 // Unused members
        #endregion


        #region Event handlers

        private void HandleContentChanged()
        {
            if (_initialised) return;

            foreach (var category in Content.Current.Combined.Nature)
            {
                var scatterCategory = Instantiate(scatterCategoryPrefab, transform);
                scatterCategory.Initialise(category);
                scatterCategories.Add(scatterCategory);
            }

            _initialised = true;
        }

        #endregion

    }
}