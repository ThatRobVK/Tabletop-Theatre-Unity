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

using System.Globalization;
using TT.World;
using UnityEngine;

namespace TT.UI.MapEditor.ActionBar
{
    [RequireComponent(typeof(CyclingSlider))]
    public class WindRotationSlider : MonoBehaviour
    {

        #region Editor fields
#pragma warning disable IDE0044 // Make fields read-only

        [SerializeField][Tooltip("An input field that allows manual keyboard input.")] private Textbox manualInput;

#pragma warning restore IDE0044
        #endregion


        #region Private fields

        private const float DEGREES_PER_PIXEL = .9f;
        private CyclingSlider _slider;

        #endregion


        #region Lifecycle events
#pragma warning disable IDE0051 // Unused members

        void Start()
        {
            _slider = GetComponent<CyclingSlider>();
            _slider.OnValueChanged += HandleValueChanged;

            if (manualInput != null)
            {
                manualInput.onValueChanged.AddListener(UpdateFromInput);
            }
        }

        void Update()
        {
            if (!_slider.IsDragging)
            {
                UpdateFromWorld(WindController.Current.Rotation);
            }
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
        /// Updates the display based on the selected WorldObject.
        /// </summary>
        /// <param name="eulerAngle">The euler angle of the WorldObject.</param>
        private void UpdateFromWorld(float eulerAngle)
        {
            // Update the slider position
            var sliderPosition = ConvertEulerToSlider(eulerAngle);
            _slider.Position = sliderPosition;

            if (manualInput != null && !manualInput.IsSelected)
            {
                // Update the input field if set and not being edited by the user
                manualInput.text = Mathf.Round(eulerAngle).ToString(CultureInfo.CurrentCulture);
            }
        }

        /// <summary>
        /// Updates the WorldObject and input based on the slider position.
        /// </summary>
        /// <param name="sliderValue">The slider's Position.</param>
        private void UpdateFromSlider(float sliderValue)
        {
            var angle = ConvertSliderToEuler(sliderValue);
            WindController.Current.Rotation = angle;

            if (manualInput != null && !manualInput.IsSelected)
            {
                // Update the input field if set and not being edited by the user
                manualInput.text = Mathf.Round(angle).ToString(CultureInfo.CurrentCulture);
            }
        }

        /// <summary>
        /// Update the slider and WorldObject based on the value entered into the input field.
        /// </summary>
        /// <param name="inputValue">The string in the input field.</param>
        private void UpdateFromInput(string inputValue)
        {
            if (float.TryParse(inputValue, out float angle))
            {
                // Clamp to a valid angle
                angle = Mathf.Round(Mathf.Clamp(angle, 0, 360));

                WindController.Current.Rotation = angle;
                _slider.Position = ConvertEulerToSlider(angle);
            }            
        }

        /// <summary>
        /// Returns the slider position for the given euler angle.
        /// </summary>
        /// <param name="eulerValue">The euler angle in degrees, 0 - 360.</param>
        /// <returns>A slider position on a scale of 0 to 4.</returns>
        private float ConvertEulerToSlider(float eulerValue)
        {
            if (eulerValue <= 180)
            {
                return eulerValue / DEGREES_PER_PIXEL;
            }
            else
            {
                return (eulerValue - 360) / DEGREES_PER_PIXEL;
            }
        }

        /// <summary>
        /// Returns the euler angles for a given slider position.
        /// </summary>
        /// <param name="sliderValue">The slider position on a scale of 0 - 4.</param>
        /// <returns>The euler angle corresponding to the slider value.</returns>
        private float ConvertSliderToEuler(float sliderValue)
        {
            if (sliderValue >= 0)
            {
                return sliderValue * DEGREES_PER_PIXEL;
            }
            else
            {
                return 360 + (sliderValue * DEGREES_PER_PIXEL);
            }
        }

        #endregion
    }
}