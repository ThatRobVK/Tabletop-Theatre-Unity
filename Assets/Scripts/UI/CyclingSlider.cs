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
using UnityEngine;
using UnityEngine.EventSystems;

namespace TT.UI
{
    public class CyclingSlider : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
    {

        #region Events

        public event Action<float> OnValueChanged;

        #endregion

        #region Editor fields
#pragma warning disable IDE0044 // Make fields read-only

        [SerializeField][Tooltip("RectTransform of the draggable image.")] private RectTransform imageRect;
        [SerializeField][Tooltip("How many pixels from the centre should the image loop?")] private float rotationPixels = 200;

#pragma warning restore IDE0044
        #endregion


        #region Private fields

        private float _realPosition;
        private Vector2 _lastPosition;

        #endregion


        #region Public properties

        /// <summary>
        /// The position from the centre, positive moving to the right and negative moving to the left.
        /// </summary>
        public float Position
        {
            get
            {
                return -_realPosition;
            }
            set
            {
                _realPosition = -value;

                // Position the image
                imageRect.anchoredPosition = new Vector2(_realPosition, imageRect.anchoredPosition.y);
            }
        }

        /// <summary>
        /// Indicates whether dragging is currently happening.
        /// </summary>
        public bool IsDragging { get; private set; }

        #endregion



        #region Event handlers

        /// <summary>
        /// Called when the user starts to drag on the slider.
        /// </summary>
        /// <param name="eventData">Mouse data.</param>
        public void OnBeginDrag(PointerEventData eventData)
        {
            // Store mouse position to calculate drag amount next frame
            _lastPosition = eventData.position;

            IsDragging = true;
        }

        /// <summary>
        /// Called each frame while the user is dragging on the slider.
        /// </summary>
        /// <param name="eventData">Mouse data.</param>
        public void OnDrag(PointerEventData eventData)
        {
            if (imageRect != null)
            {
                // Calculate new position based on the amount dragged
                var dragAmount = eventData.position.x - _lastPosition.x;
                _realPosition += dragAmount;

                if (_realPosition < -1 * rotationPixels)
                {
                    // If scrolled past the left edge, cycle to the right
                    _realPosition = rotationPixels - (Mathf.Abs(_realPosition) - rotationPixels);
                }
                else if (_realPosition > rotationPixels)
                {
                    // If scrolled past the right edge, cycle to the left
                    _realPosition = -1 * rotationPixels + _realPosition - rotationPixels;
                }

                // Position the image
                imageRect.anchoredPosition = new Vector2(_realPosition, imageRect.anchoredPosition.y);

                // Store mouse position to calculate drag amount next frame
                _lastPosition = eventData.position;

                // Raise event
                OnValueChanged?.Invoke(Position);
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            IsDragging = false;
        }

        #endregion

    }
}