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
    /// Exposes properties indicating whether key combinations are being pressed related to world object manipulation.
    /// Access this through InputMapper.Current.
    /// </summary>
    public class WorldObjectInputMapper : InputMapperBase
    {
        
        #region Constructors
        
        public WorldObjectInputMapper(InputMapperBase parent) : base(parent)
        { }
        
        #endregion
        

        #region Public properties

        /// <summary>
        /// Left mouse button press
        /// </summary>
        public bool PlaceObject => GetMouseButtonDown(0);

        /// <summary>
        /// Esc press
        /// </summary>
        public bool Cancel => GetKeyDown(KeyCode.Escape);

        /// <summary>
        /// Del press
        /// </summary>
        public bool Delete => GetKeyDown(KeyCode.Delete);

        /// <summary>
        /// Ctrl + Z
        /// </summary>
        public bool Undo => IsControlDown() && GetKeyDown(KeyCode.Z) && !IsShiftDown();

        /// <summary>
        /// Ctrl + Shift + Z
        /// </summary>
        public bool Redo => IsControlDown() && GetKeyDown(KeyCode.Z) && IsShiftDown();

        /// <summary>
        /// True on the frame that a rotation key is pressed down (T or R)
        /// </summary>
        public bool StartRotate => GetKeyDown(KeyCode.T) || GetKeyDown(KeyCode.R);

        /// <summary>
        /// True on the frame that a rotation key is released (T or R)
        /// </summary>
        public bool StopRotate => GetKeyUp(KeyCode.T) || GetKeyUp(KeyCode.R);

        /// <summary>
        /// True on the frame that a elevation key is pressed down (B or V)
        /// </summary>
        public bool StartElevate => GetKeyDown(KeyCode.B) || GetKeyDown(KeyCode.V);

        /// <summary>
        /// True on the frame that a elevation key is released (B or V)
        /// </summary>
        public bool StopElevate => GetKeyUp(KeyCode.B) || GetKeyUp(KeyCode.V);

        /// <summary>
        /// True on the frame that a scale key is pressed down (G or F)
        /// </summary>
        public bool StartScale => GetKeyDown(KeyCode.G) || GetKeyDown(KeyCode.F);

        /// <summary>
        /// True on the frame that a scale key is released (G or F)
        /// </summary>
        public bool StopScale => GetKeyUp(KeyCode.G) || GetKeyUp(KeyCode.F);

        /// <summary>
        /// True on the frame that a move key is pressed (left, right, up or down)
        /// </summary>
        public bool StartMove => GetKeyDown(KeyCode.LeftArrow) || GetKeyDown(KeyCode.RightArrow) || GetKeyDown(KeyCode.UpArrow) || GetKeyDown(KeyCode.DownArrow);

        /// <summary>
        /// True on the frame that a move key is released (left, right, up or down)
        /// </summary>
        public bool StopMove => GetKeyUp(KeyCode.LeftArrow) || GetKeyUp(KeyCode.RightArrow) || GetKeyUp(KeyCode.UpArrow) || GetKeyUp(KeyCode.DownArrow);
        
        #endregion
 
        
        #region Public methods

        /// <summary>
        /// Calculates the appropriate amount of scaling for the selected object based on keyboard input.
        /// </summary>
        /// <returns>A float indicating the scaling required. This is positive for scaling up, and negative for
        ///     scaling down.</returns>
        public float GetScaleAmount()
        {
            if (!Active) return 0;

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

        /// <summary>
        /// Calculates the amount to rotate the selected object by, based on keyboard input and the specified snap
        /// to grid value.
        /// </summary>
        /// <param name="snapToGrid">When true, rotation will snap to 90 degree rotations. When false, a smooth rotation
        ///     will be returned.</param>
        /// <returns>A float indicating the amount to rotate by, positive for clockwise, negative for counter-clockwise.
        ///     </returns>
        public float GetRotationAmount(bool snapToGrid)
        {
            if (!Active) return 0f;

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
        
        /// <summary>
        /// Calculates the amount of elevation change for the selected object based on the keyboard input and specified
        /// snap to grid setting.
        /// </summary>
        /// <param name="snapToGrid">When true, the elevation will snap to whole levels on the grid.</param>
        /// <returns>A float indicating the amount to raise (positive) or lower (negative) the object by.</returns>
        public float GetElevateAmount(bool snapToGrid)
        {
            if (!Active) return 0f;

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

        /// <summary>
        /// Calculates the amount to move the selected object by based on keyboard input.
        /// </summary>
        /// <param name="snapToGrid">When true, the movement will snap to whole grid sections.</param>
        /// <returns>A Vector3 to apply to the object's position, with the X and Z values containing the amount to move
        ///     the object in each direction, or zero for no movement. The Y value is always zero.</returns>
        public Vector3 GetMoveAmount(bool snapToGrid)
        {
            if (!Active) return Vector3.zero;

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
        
        #endregion
    }
}