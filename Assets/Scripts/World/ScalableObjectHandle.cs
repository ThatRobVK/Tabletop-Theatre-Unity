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
using TT.UI;
using UnityEngine;
using UnityEngine.Serialization;

namespace TT.World
{
    public class ScalableObjectHandle : DraggableObject
    {
        // Public fields
        [FormerlySerializedAs("Opposite")] public ScalableObjectHandle opposite;
        [FormerlySerializedAs("Parent")] public ScalableObject parent;

        // Gets a vector indicating which axis this moves along
        public Vector3 GetMovementVector()
        {
            return transform.position - opposite.transform.position;
        }

        // Returns the opposing handle's position (e.g. left handle if this is the right handle and vice versa)
        public Vector3 GetOpposingHandlePosition()
        {
            return opposite.transform.position;
        }

        // Moves this handle to the target position and tells the parent to stretch
        public void ResizeTo(Vector3 position)
        {
            transform.position = position;
            parent.MoveAndScale(handlePosition: position, oppositeHandlePosition: opposite.transform.position, movementVector: GetMovementVector());
        }

        // Moves with the parent to stay on the centre of its axis
        public void MoveToCentreOfParent(Vector3 parentPosition, Vector3 triggeringMovementVector)
        {
            var movementVector = GetMovementVector();

            // This handle moves the same axis as the triggering vector - no need to update
            if (Math.Abs(triggeringMovementVector.x - movementVector.x) < 0.01 || Math.Abs(triggeringMovementVector.z - movementVector.z) < 0.01)
            {
                return;
            }

            // For X and Z: If the triggering vector moved along this axis and this handle isn't for that axis, then adopt the parent's position (i.e. centre of the object along that axis)
            // Don't change Y
            var targetPosition = new Vector3(
                (triggeringMovementVector.x != 0) ? parentPosition.x : transform.position.x,
                transform.position.y,
                (triggeringMovementVector.z != 0) ? parentPosition.z : transform.position.z
            );

            transform.position = targetPosition;
        }

        /// <summary>
        /// Sets the cursor to scale depending on the movement vector of this handle.
        /// </summary>
        protected override void SetCursor()
        {
            if (GetMovementVector().x != 0)
            {
                CursorController.Current.ScaleHorizontal = true;
            }
            else
            {
                CursorController.Current.ScaleVertical = true;
            }
        }
    }
}