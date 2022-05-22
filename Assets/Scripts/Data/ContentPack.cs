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

#pragma warning disable IDE0090 // "Simplify new expression" - implicit object creation is not supported in the .NET version used by Unity 2020.3

namespace TT.Data
{
    
    public class ContentPack
    {
        /// <summary>
        /// A display name for this content pack.
        /// </summary>
        public string Name = string.Empty;

        /// <summary>
        /// True if the user has selected to include this pack in the map, false otherwise.
        /// </summary>
        public bool Selected = true;

        /// <summary>
        /// Lightsources in this pack.
        /// </summary>
        public ContentItemCategory[] Lightsources = new ContentItemCategory[] { };

        /// <summary>
        /// Construction items in this pack.
        /// </summary>
        public ContentConstruction Construction = new ContentConstruction();

        /// <summary>
        /// Terrain layers in this pack.
        /// </summary>
        public ContentTerrainLayer[] TerrainLayers = new ContentTerrainLayer[] { };

        /// <summary>
        /// Rivers, roads and bridges in this pack.
        /// </summary>
        public ContentRiversRoads RiversRoads = new ContentRiversRoads();

        /// <summary>
        /// Items in this pack.
        /// </summary>
        public ContentItemCategory[] Items = new ContentItemCategory[] { };

        /// <summary>
        /// Nature items in this pack.
        /// </summary>
        public ContentItemCategory[] Nature = new ContentItemCategory[] { };
    }

}