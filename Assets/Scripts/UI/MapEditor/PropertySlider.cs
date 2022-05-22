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

using System;
using System.Globalization;
using TT.Data;
using TT.World;
using UnityEngine;

namespace TT.UI.MapEditor
{
    public abstract class PropertySlider : MonoBehaviour
    {
        #region Editor fields
#pragma warning disable IDE0044 // Make fields read-only

        [SerializeField][Tooltip("The slider handle. Used for detecting drag events to register undo actions.")] private SliderHandle handle;
        [SerializeField][Tooltip("If true, the slider will snap to whole numbers if snap to grid is on. If false, the slider will ignore snap to grid.")] protected bool snapToGridEnabled;
        [SerializeField][Tooltip("An input field that allows manual keyboard input.")] protected Textbox manualInput;
        [SerializeField][Tooltip("The number of decimal places to show in the manual input field.")] protected int decimalPlaces = 2;
        [SerializeField][Tooltip("Minimum value to clamp user input to.")] protected float minValue;
        [SerializeField][Tooltip("Maximum value to clamp user input to.")] protected float maxValue;
        [SerializeField][Tooltip("If ticked, the slider value is shown, otherwise the WorldObject value is.")] protected bool showSliderValue;

#pragma warning restore IDE0044
        #endregion


        protected bool UpdatedThisFrame;
        private SettingsObject _settings;
        private bool _snapToGrid;
        protected float UndoValue;
        private bool _dragging;


        #region Lifecycle events
#pragma warning disable IDE0051 // Unused members

        protected virtual void Start()
        {
            if (manualInput != null)
            {
                manualInput.onValueChanged.AddListener(UpdateFromInput);
            }

            // Init the snap to grid setting
            _settings = FindObjectOfType<SettingsObject>();
            _snapToGrid = _settings.editorSettings.snapToGrid;
            SetWholeNumbers(_snapToGrid);

            if (handle)
            {
                // When dragging starts, register the current value, when it finishes register an undo
                handle.BeginDrag += () => { UndoValue = GetWorldObjectValue(); _dragging = true; };
            }
        }


        protected virtual void Update()
        {
            if (!UpdatedThisFrame && WorldObjectBase.Current != null)
            {
                UpdateFromWorldObject(GetWorldObjectValue());
            }

            if (_snapToGrid != _settings.editorSettings.snapToGrid)
            {
                // If snap to grid has changed, update the slider
                _snapToGrid = _settings.editorSettings.snapToGrid;
                SetWholeNumbers(_snapToGrid);
            }

            if (_dragging && !Input.GetMouseButton(0))
            {
                // If previously dragging and mouse button has been released, register an undo
                if (Math.Abs(UndoValue - GetWorldObjectValue()) > 0) RegisterUndo(UndoValue);
                _dragging = false;
            }
        }

        void LateUpdate()
        {
            UpdatedThisFrame = false;
        }

#pragma warning restore IDE0051 // Unused members
        #endregion


        #region Protected methods

        /// <summary>
        /// Updates the display based on the selected WorldObject.
        /// </summary>
        /// <param name="worldObjectValue">The value of the property on the WorldObject.</param>
        protected void UpdateFromWorldObject(float worldObjectValue)
        {
            // Do not update if another update has run this frame
            if (UpdatedThisFrame) return;
            UpdatedThisFrame = true;

            var sliderValue = ConvertWorldObjectToSlider(worldObjectValue);
            UpdateSlider(sliderValue);

            if (manualInput != null && !manualInput.IsSelected)
            {
                // Update the input field if set and not being edited by the user
                manualInput.text = Math.Round(showSliderValue ? sliderValue : worldObjectValue, decimalPlaces).ToString(CultureInfo.CurrentCulture);
            }
        }

        /// <summary>
        /// Updates the WorldObject and input based on the slider position.
        /// </summary>
        /// <param name="sliderValue">The slider's Position.</param>
        protected void UpdateFromSlider(float sliderValue)
        {
            // Do not update if another upate has run this frame
            if (UpdatedThisFrame) return;
            UpdatedThisFrame = true;

            if (!_dragging)
            {
                // Updated from slider without dragging, this can happen if the user clicks on the handle track
                RegisterUndo(GetWorldObjectValue());
            }

            var worldObjectValue = ConvertSliderToWorldObject(sliderValue);
            UpdateWorldObject(worldObjectValue);

            if (manualInput != null && !manualInput.IsSelected)
            {
                // Update the input field if set and not being edited by the user
                manualInput.text = Math.Round(showSliderValue ? sliderValue : worldObjectValue, decimalPlaces).ToString(CultureInfo.CurrentCulture);
            }
        }

        /// <summary>
        /// Update the slider and WorldObject based on the value entered into the input field.
        /// </summary>
        /// <param name="inputValue">The string in the input field.</param>
        protected void UpdateFromInput(string inputValue)
        {
            // Do not update if another upate has run this frame
            if (UpdatedThisFrame) return;

            if (float.TryParse(inputValue, out float parsedValue))
            {
                UpdatedThisFrame = true;

                RegisterUndo(GetWorldObjectValue());

                // Clamp to a valid value
                parsedValue = Mathf.Clamp(parsedValue, minValue, maxValue);

                // Get the WorldObject and slider values depending on whether the input is slider or WorldObject based
                var worldObjectValue = showSliderValue ? ConvertSliderToWorldObject(parsedValue) : parsedValue;
                var sliderValue = showSliderValue ? parsedValue : ConvertWorldObjectToSlider(parsedValue);

                UpdateWorldObject(worldObjectValue);
                UpdateSlider(sliderValue);
            }
        }

        /// <summary>
        /// Can be overridden in inheriting classes to register an undo action before a change is made to the world.
        /// </summary>
        protected virtual void RegisterUndo(float undoValue)
        { }

        #endregion


        #region Abstract members

        /// <summary>
        /// Return the property value from the WorldObject.
        /// </summary>
        /// <returns>The value of the WorldObject property being controlled.</returns>
        protected abstract float GetWorldObjectValue();

        /// <summary>
        /// Return the world object property value for the given slider position.
        /// </summary>
        /// <param name="sliderValue">The value of the slider.</param>
        /// <returns>A value to be set on the WorldObject property.</returns>
        protected abstract float ConvertSliderToWorldObject(float sliderValue);

        /// <summary>
        /// Return the slider value for the given property value of the WorldObject.
        /// </summary>
        /// <param name="worldObjectValue">The WorldObject's property value.</param>
        /// <returns>The slider value to be set.</returns>
        protected abstract float ConvertWorldObjectToSlider(float worldObjectValue);

        /// <summary>
        /// Set the slider to a specified value.
        /// </summary>
        /// <param name="sliderValue">The value to set the slider to.</param>
        protected abstract void UpdateSlider(float sliderValue);

        /// <summary>
        /// Set the WorldObject property to a specified value.
        /// </summary>
        /// <param name="worldObjectValue">The value to set the property to.</param>
        protected abstract void UpdateWorldObject(float worldObjectValue);

        /// <summary>
        /// Update the slider to use whole numbers or not, based on the given Snap To Grid value.
        /// </summary>
        /// <param name="snapToGrid">A boolean indicating whether Snap To Grid is on or off.</param>
        protected abstract void SetWholeNumbers(bool snapToGrid);

        #endregion

    }
}