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
    /// Metadata about a map for saving and loading. This class is used as the root of the map, containing collections
    /// of data classes representing objects on the map.
    /// </summary>
    [Serializable]
    public class MapData
    {
        /// <summary>
        /// Metadata about the map itself.
        /// </summary>
        public MapMetadata metadata;
        
        /// <summary>
        /// The Addressables address of the default texture to be loaded on the game terrain.
        /// </summary>
        public string terrainTextureAddress;
        
        /// <summary>
        /// The time of day the map defaults to when loaded.
        /// </summary>
        public float time = 12;
        
        /// <summary>
        /// The wind speed.
        /// </summary>
        public float wind = 0.1f;
        
        /// <summary>
        /// The wind direction, expressed as a rotation.
        /// </summary>
        public float windDirection;
        
        /// <summary>
        /// An integer representation of the LightingMode enum value used by the map.
        /// </summary>
        public int lightingMode;
        
        /// <summary>
        /// Information about the game terrain.
        /// </summary>
        public TerrainData terrain;
        
        /// <summary>
        /// A list of general world objects populating the map.
        /// </summary>
        public List<WorldObjectData> worldObjects = new List<WorldObjectData>();
        
        /// <summary>
        /// A list of objects based around splines on the map (roads, rivers).
        /// </summary>
        public List<SplineObjectData> splineObjects = new List<SplineObjectData>();
        
        /// <summary>
        /// A list of scatter areas on the map.
        /// </summary>
        public List<ScatterAreaData> scatterAreas = new List<ScatterAreaData>();
    }
}