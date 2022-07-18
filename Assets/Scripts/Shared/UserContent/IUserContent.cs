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

using System.Collections.Generic;
using System.Threading.Tasks;

namespace TT.Shared.UserContent
{
    /// <summary>
    /// Interface exposing methods for interacting with user generated content, e.g. saving and loading maps.
    /// </summary>
    public interface IUserContent
    {
        /// <summary>
        /// Saves the specified map to cloud storage.
        /// </summary>
        /// <param name="mapData">The map to save.</param>
        /// <returns>A boolean indicating whether saving the map was successful.</returns>
        Task<bool> SaveMap(MapData mapData);

        /// <summary>
        /// Loads a map from cloud storage and returns it as a MapData object. 
        /// </summary>
        /// <param name="mapId">The ID of the map to load.</param>
        /// <returns>A MapData object representing the loaded map, or null if the map could not be loaded.</returns>
        Task<MapData> LoadMap(string mapId);

        /// <summary>
        /// Deletes a map from cloud storage. This can not be undone.
        /// </summary>
        /// <param name="mapId">The ID of the map to delete.</param>
        /// <returns>A boolean indicating whether the map was deleted.</returns>
        /// <remarks>If the map wasn't there to start with, this method returns true.</remarks>
        Task<bool> DeleteMap(string mapId);

        /// <summary>
        /// Gets a list of maps stored on cloud storage for the current user. 
        /// </summary>
        /// <returns>A list of metadata objects, each representing a map in cloud storage.</returns>
        Task<List<MapMetadata>> GetMapList();
    }
}