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

using TT.World;
using UnityEngine;

namespace TT.UI.MapEditor.ActionBar
{
    [RequireComponent(typeof(CyclingSlider))]
    public class TimeOfDaySlider : MonoBehaviour
    {

        #region Private fields

        private const float DEGREES_PER_PIXEL = 0.0344827586206897f;
        private CyclingSlider _slider;

        #endregion


        #region Lifecycle events
#pragma warning disable IDE0051 // Unused members

        void Start()
        {
            _slider = GetComponent<CyclingSlider>();
            _slider.OnValueChanged += HandleValueChanged;
        }

        void Update()
        {
            UpdateFromTime();
        }

#pragma warning restore IDE0051 // Unused members
        #endregion


        #region Event handlers

        /// <summary>
        /// Called when the value of the slider changes.
        /// </summary>
        /// <param name="value">The new value of the slider.</param>
        private void HandleValueChanged(float value)
        {
            UpdateFromSlider(value);
        }

        #endregion


        #region Private methods

        /// <summary>
        /// Updates the time based on the slider position.
        /// </summary>
        /// <param name="sliderValue">The slider's Position.</param>
        private void UpdateFromSlider(float sliderValue)
        {
            TimeController.Current.CurrentTime = 12 + sliderValue * DEGREES_PER_PIXEL;
        }

        /// <summary>
        /// Updates the slider based on the current time.
        /// </summary>
        private void UpdateFromTime()
        {
            _slider.Position = (TimeController.Current.CurrentTime - 12) / DEGREES_PER_PIXEL;
        }

        #endregion
    }
}