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
using System.Collections;
using TT.UI;
using UnityEngine;

namespace TT.World
{
    public class DraggableObject : MonoBehaviour
    {

        #region Events

        public event Action<DraggableObject> OnStartDrag;
        public event Action<DraggableObject> OnDrag;
        public event Action<DraggableObject> OnStopDrag;

        #endregion


        #region Editor fields
#pragma warning disable IDE0044 // Make fields read-only

        [SerializeField][Tooltip("The time in seconds that the mouse must be held down before dragging starts.")] private float dragStartDelay;

#pragma warning restore IDE0044
        #endregion


        #region Private fields

        private bool _isDragging;
        private Vector3 _lastMousePosition;

        #endregion


        public bool IsDragging { get => _isDragging; }


        #region Lifecycle events
#pragma warning disable IDE0051 // Unused members

        void OnDisable()
        {
            // Cancel any current dragging when disabled
            _isDragging = false;
            _lastMousePosition = Vector3.zero;
        }

        void Update()
        {
            // Not dragging - do nothing
            if (!_isDragging) return;

            // While dragging, keep the move cursor
            SetCursor();

            if (Input.mousePosition != _lastMousePosition)
            {
                OnDrag?.Invoke(this);
            }

            if (!Input.GetMouseButton(0))
            {
                ToggleDrag(false);
                OnStopDrag?.Invoke(this);
            }
        }

#pragma warning restore IDE0051 // Unused members
        #endregion


        #region Public methods

        public void StartDrag(float dragDelay = 0.1f)
        {
            dragStartDelay = dragDelay;
            ToggleDrag(true);
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Start or stop dragging.
        /// </summary>
        /// <param name="drag">True to start dragging, false to stop.</param>
        private void ToggleDrag(bool drag)
        {
            if (drag && dragStartDelay > 0f)
            {
                // Delay before dragging
                StartCoroutine(DelayBeforeDragging());
            }
            else if (drag)
            {
                _isDragging = true;
                _lastMousePosition = Input.mousePosition;
                OnStartDrag?.Invoke(this);
            }
            else
            {
                _isDragging = false;
            }
        }

        /// <summary>
        /// Wait for the configured delay and then start dragging.
        /// </summary>
        /// <returns>An IEnumerator that allows this to run as a Coroutine.</returns>
        private IEnumerator DelayBeforeDragging()
        {
            // Wait for the delay
            yield return new WaitForSeconds(dragStartDelay);

            // If mouse is still down, start dragging
            if (Input.GetMouseButton(0))
            {
                _isDragging = true;
                _lastMousePosition = Input.mousePosition;
                OnStartDrag?.Invoke(this);
            }
            else
            {
                _isDragging = false;
                _lastMousePosition = Vector3.zero;
            }
        }

        /// <summary>
        /// Sets the cursor to move.
        /// </summary>
        protected virtual void SetCursor()
        {
            CursorController.Current.MoveObject = true;
        }

        #endregion
    }
}