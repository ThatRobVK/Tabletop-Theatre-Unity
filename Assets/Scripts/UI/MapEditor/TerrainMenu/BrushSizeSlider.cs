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
using TT.State;
using UnityEngine;

namespace TT.UI.MapEditor.TerrainMenu
{
    public class BrushSizeSlider : PropertySlider
    {

        #region Editor fields
#pragma warning disable IDE0044 // Make fields read-only

        [SerializeField][Tooltip("The slider controlled by this option.")] private UISliderExtended slider;

#pragma warning restore IDE0044
        #endregion


        #region Public properties

        public int BrushSize { get => (int)slider.value; }

        #endregion


        #region Lifecycle events
#pragma warning disable IDE0051 // Unused members

        protected override void Start()
        {
            slider.onValueChanged.AddListener(UpdateFromSlider);

            base.Start();
        }

        protected override void Update()
        {
            if (manualInput != null && !manualInput.IsSelected)
            {
                // Update the input field if set and not being edited by the user
                manualInput.text = BrushSize.ToString();
            }
        }

#pragma warning restore IDE0051 // Unused members
        #endregion


        #region Private methods

        protected override float ConvertSliderToWorldObject(float sliderValue)
        {
            return sliderValue;
        }

        protected override float ConvertWorldObjectToSlider(float worldObjectValue)
        {
            return worldObjectValue;
        }

        protected override float GetWorldObjectValue()
        {
            if (StateController.CurrentState is TerrainPaintState state)
            {
                // If in terrain paint state, return the current value
                return state.BrushRadius;
            }

            // Return current slider value if state isn't right
            return slider.value;
        }

        protected override void SetWholeNumbers(bool snapToGrid)
        { }

        protected override void UpdateSlider(float sliderValue)
        {
            slider.value = sliderValue;
        }

        protected override void UpdateWorldObject(float worldObjectValue)
        {
            if (StateController.CurrentState is TerrainPaintState state)
            {
                // If in terrain paint state, return the current value
                state.BrushRadius = (int)worldObjectValue;
            }
        }

        #endregion
    }
}