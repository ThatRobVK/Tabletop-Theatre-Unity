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

namespace TT.Shared.GameContent
{
    public class ContentConstruction
    {
        /// <summary>
        /// ID's of floor materials.
        /// </summary>
        public string[] Floors = { };

        /// <summary>
        /// IDs of ceiling materials.
        /// </summary>
        public string[] Ceilings = { };

        /// <summary>
        /// IDs of wall materials.
        /// </summary>
        public string[] Walls = { };

        /// <summary>
        /// Categories with full buildings.
        /// </summary>
        public ContentItemCategory[] Buildings = { };

        /// <summary>
        /// Categories with building props.
        /// </summary>
        public ContentItemCategory[] Props = { };
    }

}