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
    /// Base class for all InputMapper classes. Do not use this class directly.
    /// </summary>
    public abstract class InputMapperBase
    {
        
        #region Private fields
        
        private readonly InputMapperBase _parent;
        private bool _active = true;
        
        #endregion
        
        
        #region Constructors

        /// <summary>
        /// Initialises this mapper. This must be called in the constructor of all inheriting classes.
        /// </summary>
        /// <param name="parent">The parent InputMapper for chaining of active states.</param>
        protected InputMapperBase(InputMapperBase parent)
        {
            _parent = parent;
        }
        
        #endregion

        #region Public properties

        /// <summary>
        /// True if this class is active in itself. It may still be inactive due to parents being set to inactive.
        /// </summary>
        public bool Active
        {
            get => (_parent == null || _parent.Active) && _active;
            set => _active = value;
        }

        #endregion
        
        /// <summary>
        /// Returns whether this mapper is active and the specified key is pressed or held down.
        /// </summary>
        /// <param name="keyCode">The key to check for.</param>
        /// <param name="keyDown">When true, this only returns true on the frame the key was pressed down. When false,
        /// this will also return true on every frame the key is held down for.</param>
        /// <returns>True when the mapper is active, and either the key was pressed down during this frame (keyDown =
        /// true) or the key is being held down (keyDown = false). Otherwise false.</returns>
        protected bool GetKey(KeyCode keyCode, bool keyDown = false)
        {
            if (!Active) return false;

            return keyDown ? Input.GetKeyDown(keyCode) : Input.GetKey(keyCode);
        }

        /// <summary>
        /// Returns whether the mapper is active and the specified key was pressed down in the current frame.
        /// </summary>
        /// <param name="keyCode">The key to check for.</param>
        /// <returns>True when the mapper is active and the key was pressed down during this frame. False otherwise.
        ///     </returns>
        protected bool GetKeyDown(KeyCode keyCode)
        {
            return Active && Input.GetKeyDown(keyCode);
        }

        /// <summary>
        /// Returns whether the mapper is active and the specified key was released in the current frame.
        /// </summary>
        /// <param name="keyCode">The key to check for.</param>
        /// <returns>True when the mapper is active and the key was released during this frame. False otherwise.
        ///     </returns>
        protected bool GetKeyUp(KeyCode keyCode)
        {
            return Active && Input.GetKeyUp(keyCode);
        }

        /// <summary>
        /// Returns whether this mapper is active and the specified mouse button is pressed or held down.
        /// </summary>
        /// <param name="mouseButton">The mouse button index to check for.</param>
        /// <param name="buttonDown">When true, this only returns true on the frame the mouse button was pressed down.
        ///     When false, this will also return true on every frame the mouse button is held down for.</param>
        /// <returns>True when the mapper is active, and either the button was pressed down during this frame
        ///     (buttonDown = true) or the button is being held down (buttonDown = false). Otherwise false.</returns>
        protected bool GetMouseButton(int mouseButton, bool buttonDown = false)
        {
            if (!Active) return false;
            return buttonDown ? Input.GetMouseButtonDown(mouseButton) : Input.GetMouseButton(mouseButton);
        }

        /// <summary>
        /// Returns whether the mapper is active and the specified mouse button was pressed down during the current
        /// frame.
        /// </summary>
        /// <param name="mouseButton">The mouse button to check for.</param>
        /// <returns>True if the mouse button was pressed down during the current frame. False otherwise.</returns>
        protected bool GetMouseButtonDown(int mouseButton)
        {
            return Active && Input.GetMouseButtonDown(mouseButton);
        }

        /// <summary>
        /// Checks if a control key is down.
        /// </summary>
        /// <returns>True if either control key is held down.</returns>
        /// <remarks>When running in Unity, this returns true when the Alt GR key is down to avoid clashes with Unity
        ///     keybindings.</remarks>
        protected bool IsControlDown()
        {
#if UNITY_EDITOR
            return Input.GetKey(KeyCode.AltGr);
#else
            return Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
#endif
        }

        /// <summary>
        /// Checks if a shift key is down.
        /// </summary>
        /// <returns>True if either left or right shift is held down.</returns>
        protected bool IsShiftDown()
        {
            return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        }

    }
}