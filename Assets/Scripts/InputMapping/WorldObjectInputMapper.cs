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
    public class WorldObjectInputMapper : InputMapperBase
    {
        public WorldObjectInputMapper(InputMapperBase parent) : base(parent)
        { }

        public float GetScaleAmount()
        {
            if (!IsActive()) return 0;

            var snap = IsControlDown();
            var fast = IsShiftDown();

            var amount = 0f;
            if (GetKey(KeyCode.G, snap)) amount += 1;
            if (GetKey(KeyCode.F, snap)) amount -= 1;

            if (amount == 0) return 0;

            if (snap && fast) amount *= Helpers.Settings.editorSettings.scaleStepSpeedFast;
            else if (snap && !fast) amount *= Helpers.Settings.editorSettings.scaleStepSpeed;
            else if (fast && !snap) amount *= Helpers.Settings.editorSettings.scaleSpeedFast * Time.deltaTime;
            else if (!fast && !fast) amount *= Helpers.Settings.editorSettings.scaleSpeed * Time.deltaTime;

            return amount;
        }


        public float GetRotationAmount(bool snapToGrid)
        {
            if (!IsActive()) return 0f;

            var snap = snapToGrid || IsControlDown();
            var fast = IsShiftDown();

            float amount = 0f;
            if (GetKey(KeyCode.T, snap)) amount += 1f;
            if (GetKey(KeyCode.R, snap)) amount -= 1f;

            if (amount == 0) return 0;

            if (snapToGrid) amount *= Helpers.Settings.editorSettings.rotateSnapToGridSpeed;
            else if (snap && fast) amount *= Helpers.Settings.editorSettings.rotateStepSpeedFast;
            else if (snap && !fast) amount *= Helpers.Settings.editorSettings.rotateStepSpeed;
            else if (fast && !snap) amount *= Helpers.Settings.editorSettings.rotateSpeedFast * Time.deltaTime;
            else if (!fast && !fast) amount *= Helpers.Settings.editorSettings.rotateSpeed * Time.deltaTime;

            return amount;
        }


        public float GetElevateAmount(bool snapToGrid)
        {
            if (!IsActive()) return 0f;

            // Snap on holding control
            var snap = snapToGrid || IsControlDown();
            var fast = IsShiftDown();

            float amount = 0;
            if (GetKey(KeyCode.B, snap)) amount += 1;
            if (GetKey(KeyCode.V, snap)) amount -= 1;

            if (amount == 0) return 0;

            if (snapToGrid) amount *= Helpers.Settings.editorSettings.elevateSnapToGridSpeed;
            else if (snap && fast) amount *= Helpers.Settings.editorSettings.elevateStepSpeedFast;
            else if (snap && !fast) amount *= Helpers.Settings.editorSettings.elevateStepSpeed;
            else if (fast && !snap) amount = amount * Helpers.Settings.editorSettings.elevateSpeedFast * Time.deltaTime;
            else if (!fast && !fast) amount = amount * Helpers.Settings.editorSettings.elevateSpeed * Time.deltaTime;

            return amount;
        }

        public Vector3 GetMoveAmount(bool snapToGrid)
        {
            if (!IsActive()) return Vector3.zero;

            // Snap on holding control
            var snap = snapToGrid || IsControlDown();
            var fast = IsShiftDown();

            var amount = Vector3.zero;
            if (GetKey(KeyCode.RightArrow, snap)) amount.x += 1;
            if (GetKey(KeyCode.LeftArrow, snap)) amount.x -= 1;
            if (GetKey(KeyCode.UpArrow, snap)) amount.z += 1;
            if (GetKey(KeyCode.DownArrow, snap)) amount.z -= 1;

            if (snapToGrid) amount *= Helpers.Settings.editorSettings.moveSnapToGridSpeed;
            else if (snap && fast) amount *= Helpers.Settings.editorSettings.moveStepSpeedFast;
            else if (snap) amount *= Helpers.Settings.editorSettings.moveStepSpeed;
            else if (fast) amount = Helpers.Settings.editorSettings.moveSpeedFast * Time.deltaTime * amount;
            else amount = Helpers.Settings.editorSettings.moveSpeed * Time.deltaTime * amount;

            return amount;
        }

        // Left mouse button pressed
        public bool PlaceObject { get => IsActive() && Input.GetMouseButtonDown(0); }

        // Escape pressed
        public bool Cancel { get => IsActive() && Input.GetKeyDown(KeyCode.Escape); }

        // Delete key pressed
        public bool Delete { get => IsActive() && Input.GetKeyDown(KeyCode.Delete); }
        
        // Either control held down and Z pressed (AltGr in editor to avoid clashes with Unity keybindings)
        public bool Undo { get => IsActive() && IsControlDown() && Input.GetKeyDown(KeyCode.Z) && !IsShiftDown(); }

        // Either control held down and Z pressed (AltGr in editor to avoid clashes with Unity keybindings)
        public bool Redo { get => IsActive() && IsControlDown() && Input.GetKeyDown(KeyCode.Z) && IsShiftDown(); }

        /// <summary>
        /// True on the frame that the rotation key is pressed down (T or R)
        /// </summary>
        public bool StartRotate { get => IsActive() && (Input.GetKeyDown(KeyCode.T) || Input.GetKeyDown(KeyCode.R)); }
        
        /// <summary>
        /// True on the frame that the rotation key is released (T or R)
        /// </summary>
        public bool StopRotate { get => IsActive() && (Input.GetKeyUp(KeyCode.T) || Input.GetKeyUp(KeyCode.R)); }

        /// <summary>
        /// True on the frame that the rotation key is pressed down (T or R)
        /// </summary>
        public bool StartElevate { get => IsActive() && (Input.GetKeyDown(KeyCode.B) || Input.GetKeyDown(KeyCode.V)); }
        
        /// <summary>
        /// True on the frame that the rotation key is released (T or R)
        /// </summary>
        public bool StopElevate { get => IsActive() && (Input.GetKeyUp(KeyCode.B) || Input.GetKeyUp(KeyCode.V)); }

        /// <summary>
        /// True on the frame that the rotation key is pressed down (T or R)
        /// </summary>
        public bool StartScale { get => IsActive() && (Input.GetKeyDown(KeyCode.G) || Input.GetKeyDown(KeyCode.F)); }
        
        /// <summary>
        /// True on the frame that the rotation key is released (T or R)
        /// </summary>
        public bool StopScale { get => IsActive() && (Input.GetKeyUp(KeyCode.G) || Input.GetKeyUp(KeyCode.F)); }

        public bool StartMove { get => IsActive() && (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow)); }
        public bool StopMove { get => IsActive() && (Input.GetKeyUp(KeyCode.LeftArrow) || Input.GetKeyUp(KeyCode.RightArrow) || Input.GetKeyUp(KeyCode.UpArrow) || Input.GetKeyUp(KeyCode.DownArrow)); }

        /// <summary>
        /// True on the frame that the up arrow is released
        /// </summary>
        public bool MoveRight { get => IsActive() && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKeyDown(KeyCode.RightArrow); }
        public bool MoveLeft { get => IsActive() && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKeyDown(KeyCode.LeftArrow); }
        public bool MoveUp { get => IsActive() && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKeyDown(KeyCode.UpArrow); }
        public bool MoveDown { get => IsActive() && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKeyDown(KeyCode.DownArrow); }

    }
}