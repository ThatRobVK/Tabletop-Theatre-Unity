using System.Collections.Generic;
using System.Threading.Tasks;

namespace TT.Shared.UserContent
{
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
        /// <returns>A list of MapIndexItem objects, each representing a map in cloud storage.</returns>
        Task<List<MapIndexItem>> GetMapIndex();
    }
}