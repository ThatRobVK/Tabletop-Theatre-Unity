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

namespace TT.InputMapping
{
    /// <summary>
    /// Utility class with boolean properties indicating whether specific keybindings are pressed in the current frame.
    /// </summary>
    public class InputMapper : InputMapperBase
    {
        
        #region Private fields
        
        private static InputMapper _current;
        private WorldObjectInputMapper _worldObjectInput;
        private CameraInputMapper _cameraInput;
        private GeneralInputMapper _generalInput;
        
        #endregion
        
        
        #region Constructors

        private InputMapper() : base(null)
        {
        }
        
        #endregion
        
        
        #region Public properties

        /// <summary>
        /// A singleton instance of the InputMapper class.
        /// </summary>
        public static InputMapper Current => _current ??= new InputMapper();

        /// <summary>
        /// Keybindings for interacting with world objects. 
        /// </summary>
        public WorldObjectInputMapper WorldObjectInput => _worldObjectInput ??= new WorldObjectInputMapper(this);

        /// <summary>
        /// Keybindings for controlling the camera.
        /// </summary>
        public CameraInputMapper CameraInput => _cameraInput ??= new CameraInputMapper(this);

        /// <summary>
        /// Keybindings for general application control.
        /// </summary>
        public GeneralInputMapper GeneralInput => _generalInput ??= new GeneralInputMapper(this);
        
        #endregion
        
    }
}