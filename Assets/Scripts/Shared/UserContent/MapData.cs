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
using UnityEngine;

namespace TT.Shared.UserContent
{
    [Serializable]
    public class MapData
    {
        public MapMetadata metadata;
        public string terrainTextureAddress;
        public float time = 12;
        public float wind = 0.1f;
        public float windDirection;
        public int lightingMode;
        public TerrainData terrain;
        public List<WorldObjectData> worldObjects = new List<WorldObjectData>();
        public List<SplineObjectData> splineObjects = new List<SplineObjectData>();
        public List<ScatterAreaData> scatterAreas = new List<ScatterAreaData>();
    }
}