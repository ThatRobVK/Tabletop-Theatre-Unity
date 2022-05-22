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
using DuloGames.UI;
using TT.InputMapping;
using TT.MapEditor;
using TT.World;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TT.UI.MapEditor.ObjectProperties
{
    [RequireComponent(typeof(CyclingSlider))]
    public class RotationSlider : MonoBehaviour, IBeginDragHandler, IEndDragHandler
    {

        #region Editor fields
#pragma warning disable IDE0044 // Make fields read-only

        [SerializeField][Tooltip("An input field that allows manual keyboard input.")] private Textbox manualInput;
        [SerializeField][Tooltip("The axis this rotates around.")] private SnapAxis axis = SnapAxis.None;
        [SerializeField] [Tooltip("Whether this slider should handle keyboard input. Only one should be active at a time.")] private bool handleKeyboardInput;

#pragma warning restore IDE0044
        #endregion


        #region Private fields

        private const float DEGREES_PER_PIXEL = .9f;
        private CyclingSlider _slider;
        private bool _automaticUpdate;
        private Vector3 _undoValue = Vector3.zero;
        private UIWindow _window;

        #endregion


        #region Lifecycle events
#pragma warning disable IDE0051 // Unused members

        void Start()
        {
            _window = GetComponentInParent<UIWindow>();

            _slider = GetComponent<CyclingSlider>();
            _slider.OnValueChanged += HandleValueChanged;

            if (manualInput != null)
            {
                manualInput.onValueChanged.AddListener(UpdateFromInput);
            }
        }

        void Update()
        {
            if (WorldObjectBase.Current && !_slider.IsDragging)
            {
                _automaticUpdate = true;
                UpdateFromWorldObject(GetWorldObjectValue());
                _automaticUpdate = false;
            }

            if (WorldObjectBase.Current && handleKeyboardInput && _window.IsOpen)
            {
                // Hook keyboard controls into undo
                if (InputMapper.Current.WorldObjectInput.StartRotate) OnBeginDrag(null);
                if (InputMapper.Current.WorldObjectInput.StopRotate) OnEndDrag(null);

                var rotation = InputMapper.Current.WorldObjectInput.GetRotationAmount(Helpers.Settings.editorSettings.snapToGrid);
                if (rotation != 0) WorldObjectBase.Current.Rotate(rotation);
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

        /// <summary>
        /// Called when the user starts dragging the slider.
        /// </summary>
        /// <param name="eventData">Ignored.</param>
        public void OnBeginDrag(PointerEventData eventData)
        {
            // Store the current object's rotation value
            if (WorldObjectBase.Current == null) return;
            _undoValue = WorldObjectBase.Current.LocalRotation;
        }

        /// <summary>
        /// Called when the user finishes dragging the slider.
        /// </summary>
        /// <param name="eventData">Ignored.</param>
        public void OnEndDrag(PointerEventData eventData)
        {
            // If the rotation has changed, store the previous value for undo
            if (_undoValue != WorldObjectBase.Current.LocalRotation)
            {
                UndoController.RegisterAction(ActionType.Rotation, WorldObjectBase.Current.ObjectId, _undoValue);
            }
        }

        #endregion


        #region Private methods

        private float GetWorldObjectValue()
        {
            switch (axis)
            {
                case SnapAxis.X:
                    return WorldObjectBase.Current.LocalRotation.x;
                case SnapAxis.Y:
                    return WorldObjectBase.Current.LocalRotation.y;
                case SnapAxis.Z:
                    return WorldObjectBase.Current.LocalRotation.z;
            }

            return 0;
        }

        /// <summary>
        /// Updates the display based on the selected WorldObject.
        /// </summary>
        /// <param name="eulerAngle">The euler angle of the WorldObject.</param>
        private void UpdateFromWorldObject(float eulerAngle)
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
            if (_automaticUpdate) return;

            var angle = ConvertSliderToEuler(sliderValue);
            if (WorldObjectBase.Current)
                WorldObjectBase.Current.RotateTo(angle, axis);

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
            if (_automaticUpdate) return;
            
            if (float.TryParse(inputValue, out float angle))
            {
                // Clamp to a valid angle
                angle = Mathf.Round(Mathf.Clamp(angle, 0, 360));

                if (WorldObjectBase.Current) 
                    WorldObjectBase.Current.RotateTo(angle, axis);
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