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

namespace TT.Shared.UserContent
{
    /// <summary>
    /// Data about a general object placed in the world.
    /// </summary>
    [Serializable]
    public class WorldObjectData : BaseObjectData
    {
        /// <summary>
        /// For objects that can be opened, their current state, true for open, false for closed.
        /// </summary>
        public bool openCloseState;
        
        /// <summary>
        /// Whether the object should snap to the highest surface underneath, or stay at the same elevation when moved.
        /// </summary>
        public bool automaticElevation;
    }
}