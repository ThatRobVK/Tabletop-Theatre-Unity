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
    public abstract class InputMapperBase
    {
        // Constructor taking in a parent (or null)
        private InputMapperBase parent;
        public InputMapperBase(InputMapperBase parent)
        {
            this.parent = parent;
            Active = true;
        }

        // Returns whether controls within this controller is active
        public bool Active { get; private set; }

        // Activates the controls within this controller
        public void SetActive(bool active)
        {
            Active = active;
        }

        // Returns whether this control set is active, taking into account the parent's Active state
        public bool IsActive()
        {
            return (parent == null || parent.IsActive()) && Active;
        }

        // Returns true if the key is pressed down in snap to grid mode, or held in freeform mode
        protected bool GetKey(KeyCode keyCode, bool snapToGrid)
        {
            if (!IsActive()) return false;

            return snapToGrid ? Input.GetKeyDown(keyCode) : Input.GetKey(keyCode);
        }

        /// <summary>
        /// Checks if a control key is down.
        /// </summary>
        /// <returns>True if either control key is held down.</returns>
        /// <remarks>When running in Unity, this returns true when the Alt GR key is down to avoid clashes with Unity keybindings.</remarks>
        protected bool IsControlDown()
        {
#if UNITY_EDITOR
            return Input.GetKey(KeyCode.AltGr);
#else
            return Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
#endif
        }

        protected bool IsShiftDown()
        {
            return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        }

    }
}