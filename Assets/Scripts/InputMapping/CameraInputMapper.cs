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

namespace TT.InputMapping
{
    /// <summary>
    /// Exposes properties indicating whether certain key combinations are being pressed relating to camera control.
    /// Access this through InputMapper.Current.
    /// </summary>
    public class CameraInputMapper : InputMapperBase
    {
        
        #region Constructors
        
        public CameraInputMapper(InputMapperBase parent) : base(parent)
        {
        }
        
        #endregion
        
        
        #region Public properties

        /// <summary>
        /// Q - rotate camera counter clockwise
        /// </summary>
        public bool RotateLeft => GetKey(KeyCode.Q);

        /// <summary>
        /// E - rotate camera clockwise
        /// </summary>
        public bool RotateRight => GetKey(KeyCode.E);

        /// <summary>
        /// Shift - increase speed of camera movements
        /// </summary>
        public bool Fast => IsShiftDown();

        /// <summary>
        /// O pressed - toggle between top-down lock and tilt-able camera.
        /// </summary>
        public bool TopDownToggle => GetKeyDown(KeyCode.O);

        /// <summary>
        /// Space - Let user drag camera by clicking and dragging the map.
        /// </summary>
        public bool Drag => GetKey(KeyCode.Space);
        
        #endregion

        
        #region Public methods
        
        /// <summary>
        /// Returns a vector indicating which directions the user is pressing for camera movement.
        /// </summary>
        /// <returns>
        /// A Vector3 indicating which direction the camera should move along each axis. The Y axis is always zero.
        /// Note this only returns 1, 0 or -1 for each axis and does not take into consideration movement speed.
        /// </returns>
        public Vector3 GetCameraMovementVector()
        {
            // Return if not active or if control is down (e.g. Ctrl+S should not move camera)
            if (!Active || IsControlDown()) return Vector3.zero;

            var x = GetKey(KeyCode.A) ? -1 : 0;
            if (GetKey(KeyCode.D)) x++;

            var z = GetKey(KeyCode.W) ? 1 : 0;
            if (GetKey(KeyCode.S)) z--;

            return new Vector3(x, 0, z);
        }

        #endregion
        
    }
}