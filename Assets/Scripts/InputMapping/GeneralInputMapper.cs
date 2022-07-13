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
    /// Exposes properties indicating whether certain key combinations are being pressed relating to general app
    /// control. Access this through InputMapper.Current.
    /// </summary>
    public class GeneralInputMapper : InputMapperBase
    {
        
        #region Constructors
        
        public GeneralInputMapper(InputMapperBase parent) : base(parent)
        { }
        
        #endregion
        
        
        #region Public properties

        /// <summary>
        /// Ctrl + S
        /// </summary>
        public bool Save => Active && IsControlDown() && GetKeyDown(KeyCode.S) && !IsShiftDown();

        /// <summary>
        /// Ctrl + Shift + S
        /// </summary>
        public bool SaveAs => Active && IsControlDown() && GetKeyDown(KeyCode.S) && IsShiftDown();

        /// <summary>
        /// Ctrl + L
        /// </summary>
        public bool Load => Active && IsControlDown() && GetKeyDown(KeyCode.L);
        
        #endregion
        
    }
}