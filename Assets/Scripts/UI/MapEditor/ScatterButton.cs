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
using TT.State;
using TT.World;

namespace TT.UI.MapEditor
{
    [RequireComponent(typeof(Button))]
    public class ScatterButton : MonoBehaviour
    {
        #region Editor fields

        [SerializeField][Tooltip("The GameObject that contains the ScatterCategory objects to use.")] private ScatterGrid scatterGrid;

        #endregion


        #region Private fields

        private Button _button;

        #endregion


        #region Lifecycle events

        void Start()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(HandleButtonClicked);
        }

        #endregion


        #region Event handlers

        /// <summary>
        /// Called when the scatter button is clicked. Tell the polygon to scatter the selected items.
        /// </summary>
        private void HandleButtonClicked()
        {
            var polygon = WorldObjectBase.Current as PolygonObject;

            if (polygon)
            {
                // Cancel placement on scattering
                if (StateController.CurrentState.IsPlacementState) StateController.CurrentState.ToIdle();

                polygon.DestroyChildren();

                foreach (var scatterCategory in scatterGrid.ScatterCategories)
                {
                    // For each category, scatter its goodness
                    var categories = scatterCategory.GetCategories();
                    if (categories.Count > 0)
                    {
                        polygon.PlaceRandom(categories, scatterCategory.Density);
                    }
                }
            }
        }

        #endregion


    }
}