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
using TT.World;
using UnityEngine;

namespace TT.UI.MapEditor.ActionBar
{
    [RequireComponent(typeof(UISliderExtended))]
    public class WindSpeedSlider : PropertySlider
    {

        #region Private fields

        private UISliderExtended _slider;

        #endregion


        #region Lifecycle events
#pragma warning disable IDE0051 // Unused members

        protected override void Start()
        {
            _slider = GetComponent<UISliderExtended>();
            _slider.onValueChanged.AddListener(UpdateFromSlider);

            base.Start();
        }

        protected override void Update()
        {
            if (!UpdatedThisFrame)
            {
                UpdatedThisFrame = true;
                UpdateSlider(ConvertWorldObjectToSlider(GetWorldObjectValue()));
            }
        }

#pragma warning restore IDE0051 // Unused members
        #endregion


        #region Override methods

        protected override float GetWorldObjectValue()
        {
            return WindController.Current.CurrentWind;
        }

        protected override float ConvertSliderToWorldObject(float sliderValue)
        {
            return sliderValue;
        }

        protected override float ConvertWorldObjectToSlider(float worldObjectValue)
        {
            return worldObjectValue;
        }

        protected override void UpdateSlider(float sliderValue)
        {
            _slider.value = sliderValue;
        }

        protected override void UpdateWorldObject(float worldObjectValue)
        {
            WindController.Current.CurrentWind = worldObjectValue;
        }

        protected override void SetWholeNumbers(bool snapToGrid)
        {
            // Snapping to grid for scale is not done via the slider
            _slider.wholeNumbers = false;
        }

        #endregion

    }
}