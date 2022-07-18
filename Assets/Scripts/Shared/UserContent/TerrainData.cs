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
    /// Data about the game terrain.
    /// </summary>
    [Serializable]
    public class TerrainData
    {
        /// <summary>
        /// A list of Addressables addresses of the textures loaded into the terrain. This list must be ordered. 
        /// </summary>
        public List<string> terrainLayers = new List<string>();
        
        /// <summary>
        /// The width of the splat maps used for this map.
        /// </summary>
        public int splatWidth;
        
        /// <summary>
        /// The height of the splat maps used for this map.
        /// </summary>
        public int splatHeight;
        
        /// <summary>
        /// A list of splat map locations with alpha values with the X and Y vertices indicating the location on the
        /// game terrain, the Z vertex being the index of the terrain layer, and the W vertex being the alpha. This list
        /// should not contain alpha values for the base terrain as the alpha for this will start at 1 and be reduced
        /// by the alpha of each layer above it. For storage size, only vertices with an alpha (W) value above 0 should
        /// be included in the list.
        /// </summary>
        public List<VectorData> splatMaps = new List<VectorData>();
    }
}