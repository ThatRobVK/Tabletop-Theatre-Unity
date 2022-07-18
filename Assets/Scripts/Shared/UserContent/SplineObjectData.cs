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
using System.Collections.Generic;

namespace TT.Shared.UserContent
{
    /// <summary>
    /// Data about an object that is based on a spline (e.g. road, river).
    /// </summary>
    [Serializable]
    public class SplineObjectData : BaseObjectData
    {
        /// <summary>
        /// The Addressables address of the texture to use for the terrain under the spline.
        /// </summary>
        public string primaryTerrainAddress;
        
        /// <summary>
        /// The Addressables address of the texture to use for detailing around the spline.
        /// </summary>
        public string secondaryTerrainAddress;
        
        /// <summary>
        /// A list of points in world space along which the spline runs. This list must be in the correct order.
        /// </summary>
        public List<VectorData> points = new List<VectorData>();
    }
}