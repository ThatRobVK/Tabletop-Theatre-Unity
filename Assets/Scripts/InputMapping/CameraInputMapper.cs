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
    public class CameraInputMapper : InputMapperBase
    {
        public CameraInputMapper(InputMapperBase parent) : base(parent)
        {
        }

        // Camera movement
        public bool RotateLeft { get => IsActive() && Input.GetKey(KeyCode.Q); }
        public bool RotateRight { get => IsActive() && Input.GetKey(KeyCode.E); }
        public bool Fast { get => GetKey(KeyCode.LeftShift, false) || GetKey(KeyCode.RightShift, false); }
        public bool TopDownToggle { get => GetKey(KeyCode.O, true) && !IsControlDown(); }
        public bool Drag { get => GetKey(KeyCode.Space, false); }

        public Vector3 GetCameraMovementVector()
        {
            // Return if not active or if control is down (e.g. Ctrl+S should not move camera)
            if (!IsActive() || IsControlDown()) return Vector3.zero;

            var x = GetKey(KeyCode.A, false) ? -1 : 0;
            if (GetKey(KeyCode.D, false)) x++;

            var z = GetKey(KeyCode.W, false) ? 1 : 0;
            if (GetKey(KeyCode.S, false)) z--;

            return new Vector3(x, 0, z);
        }

    }
}