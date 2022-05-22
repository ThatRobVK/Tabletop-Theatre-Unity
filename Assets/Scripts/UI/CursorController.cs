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

namespace TT.UI
{
    public class CursorController : MonoBehaviour
    {

        #region Editor fields
#pragma warning disable IDE0044 // Make fields read-only

        [Header("General cursors")]
        [SerializeField][Tooltip("Standard cursor used when none other apply.")] private Texture2D defaultCursor;
        [SerializeField][Tooltip("Wait for something to complete.")] private Texture2D waitCursor;
        
        [Header("Object interaction cursors")]
        [SerializeField][Tooltip("Hover over an unselected object.")] private Texture2D selectObjectCursor;
        [SerializeField][Tooltip("Hover over a selected and movable object or handle.")] private Texture2D moveObjectCursor;
        [SerializeField][Tooltip("Scale on the X axis, e.g. a ScalableObject handle.")] private Texture2D scaleHorizontalCursor;
        [SerializeField][Tooltip("Scale on the Z axis, e.g. a ScalableObject handle.")] private Texture2D scaleVerticalCursor;
        [SerializeField][Tooltip("Unable to interact with the object.")] private Texture2D deniedCursor;

        [Header("Camera control cursors")]
        [SerializeField][Tooltip("Rotate and pan camera.")] private Texture2D rotateCameraCursor;
        [SerializeField][Tooltip("Move the camera over the map.")] private Texture2D moveCameraCursor;
        [SerializeField][Tooltip("Move the camera over the map.")] private Texture2D moveCamera2Cursor;

#pragma warning restore IDE0044
        #endregion

        #region Private fields

        private Vector2 _centreHotspot;

        #endregion


        #region Public properties

        /// <summary>
        /// Singleton instance.
        /// </summary>
        public static CursorController Current { get; private set; }

        /// <summary>
        /// Set to true when the user has to wait for an operation to complete.
        /// </summary>
        public bool Wait { get; set; }

        /// <summary>
        /// Set to true when hovering over a selectable object.
        /// </summary>
        public bool SelectObject { get; set; }

        /// <summary>
        /// Set to true when hovering over a selected, movable object or handle.
        /// </summary>
        public bool MoveObject { get; set; }
        
        /// <summary>
        /// Set to true when hovering over an object that can be moved along the X axis, e.g. a ScalableObjectHandle.
        /// </summary>
        public bool ScaleHorizontal { get; set; }

        /// <summary>
        /// Set to true when hovering over an object that can be moved along the Z axis, e.g. a ScalableObjectHandle.
        /// </summary>
        public bool ScaleVertical { get; set; }

        /// <summary>
        /// Set to true when the hotkey is pressed to rotate and pan the camera.
        /// </summary>
        public bool RotateCamera { get; set; }

        /// <summary>
        /// Set to true when the hotkey is pressed to move the camera along the map.
        /// </summary>
        public bool MoveCamera { get; set; }

        /// <summary>
        /// Set to true when the user is dragging the camera.
        /// </summary>
        public bool MoveCamera2 { get; internal set; }

        /// <summary>
        /// Set to true when the user can't interact with the hovered object.
        /// </summary>
        public bool Denied { get; set; }

        #endregion


        #region Lifecycle events
#pragma warning disable IDE0051 // Unused members

        void Start()
        {
            Current = this;

            // Get the centre hotspot from one of the cursors - this assumes all cursors are the same size!!
            _centreHotspot = new Vector2(moveObjectCursor.width / 2f, moveObjectCursor.height / 2f);
        }

        void LateUpdate()
        {
            // Set cursor in LateUpdate so that all other objects get time to Update and set their cursor preferences
            Texture2D cursor = null;
            Vector2 hotspot = Vector2.zero;

            // Pick the new cursor in priority order
            if (Wait)
            {
                cursor = waitCursor;
                hotspot = _centreHotspot;
            }
            else if (MoveCamera2)
            {
                cursor = moveCamera2Cursor;
            }
            else if (MoveCamera)
            {
                cursor = moveCameraCursor;
            }
            else if (RotateCamera)
            {
                cursor = rotateCameraCursor;
                hotspot = _centreHotspot;
            }
            else if (Denied)
            {
                cursor = deniedCursor;
            }
            else if (MoveObject)
            {
                cursor = moveObjectCursor;
                hotspot = _centreHotspot;
            }
            else if (ScaleHorizontal)
            {
                cursor = scaleHorizontalCursor;
                hotspot = _centreHotspot;
            }
            else if (ScaleVertical)
            {
                cursor = scaleVerticalCursor;
                hotspot = _centreHotspot;
            }
            else if (SelectObject)
            {
                cursor = selectObjectCursor;
            }
            else
            {
                cursor = defaultCursor;
            }

            // Set the cursor
            Cursor.SetCursor(cursor, hotspot, CursorMode.Auto);

            // Reset for next frame so cursors don't linger around
            Wait = false;
            Denied = false;
            MoveCamera = false;
            MoveCamera2 = false;
            RotateCamera = false;
            MoveObject = false;
            ScaleHorizontal = false;
            ScaleVertical = false;
            SelectObject = false;
        }
 
 #pragma warning restore IDE0051 // Unused members
        #endregion

    }
}