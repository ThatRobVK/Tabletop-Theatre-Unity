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
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using TT.World;
using TT.Shared.UserContent;

namespace TT.Data
{
    public class Map
    {
        #region Events

        /// <summary>
        /// Invoked when a map load has completed. The boolean parameter indicates whether loading was successful.
        /// </summary>
        public static Action<bool> OnMapLoaded;

        /// <summary>
        /// Invoked when a map save has completed. The boolean parameter indicates whether loading was successful.
        /// </summary>
        public static Action<bool> OnMapSaved;

        /// <summary>
        /// Invoked when a map rendering has completed.
        /// </summary>
        public static Action OnMapRendered;

        #endregion


        #region Public properties

        /// <summary>
        /// The unique identifier of this map.
        /// </summary>
        public Guid Id => Guid.Parse(_mapData.id ?? string.Empty);

        /// <summary>
        /// A user-defined name for the map.
        /// </summary>
        public string Name { get => _mapData.name; set => _mapData.name = value; }

        /// <summary>
        /// A user-defined description for the map.
        /// </summary>
        public string Description { get => _mapData.description; set => _mapData.description = value; }

        /// <summary>
        /// The user who originally created the map.
        /// </summary>
        public string Author => _mapData.authorDisplayname;

        /// <summary>
        /// The last person who has modified the map.
        /// </summary>
        public string ModifiedBy => _mapData.modifiedByDisplayname;

        /// <summary>
        /// The date and time on which this map was originally created.
        /// </summary>
        public DateTime DateCreated => DateTime.FromFileTimeUtc(_mapData.dateCreated);

        /// <summary>
        /// The date and time on which this map was last saved.
        /// </summary>
        public DateTime DateSaved => DateTime.FromFileTimeUtc(_mapData.dateSaved);

        /// <summary>
        /// The currently loaded map. This may be null until a map is loaded or the New method is called.
        /// </summary>
        public static Map Current { get; private set; }

        #endregion


        #region Private fields

        private readonly MapData _mapData;

        #endregion


        #region Constructors / destructors

        /// <summary>
        /// Creates a new map
        /// </summary>
        /// <param name="mapData"></param>
        private Map(MapData mapData)
        {
            _mapData = mapData;
        }

        #endregion


        #region Public methods

        /// <summary>
        /// Creates a new map and sets it to the current map.
        /// </summary>
        /// <param name="name">A user defined name for the map. This can be null and set later through the Name property.</param>
        /// <param name="description">A user defined description for the map. This can be null and set later through the Description property.</param>
        /// <param name="terrainId"></param>
        /// <remarks>This method requires that the user is logged in.</remarks>
        public static void New(string name, string description, string terrainId = "")
        {
            if (!Helpers.Comms.User.IsLoggedIn)
            {
                Debug.LogError("Map :: New :: User not logged in. Unable to create new map.");
                Current = null;
                return;
            }

            var id = Helpers.Comms.User.Id;
            var user = Helpers.Comms.User.Username;
            var mapData = new MapData()
            {
                id = Guid.NewGuid().ToString(),
                name = name,
                description = description,
                authorUUID = id,
                authorDisplayname = user,
                modifiedByUUID = id,
                modifiedByDisplayname = user,
                dateCreated = DateTime.Now.ToFileTimeUtc()
            };

            if (!string.IsNullOrEmpty(terrainId))
            {
                mapData.terrainTextureAddress = terrainId;
                mapData.terrain = new Shared.UserContent.TerrainData
                {
                    splatMaps = new List<Vector4Data>(),
                    splatHeight = 1,
                    splatWidth = 1,
                    terrainLayers = new List<string> { terrainId }
                };
            }
            Current = new Map(mapData);
        }

        /// <summary>
        /// Loads the map represented by the specified MapData and instantiates all of its objects. This method is
        /// asynchronous and will raise the OnMapLoaded event when completed.
        /// </summary>
        /// <param name="mapId">The ID of the map to load. This can be found in the list returned by the GetMapIndex
        /// method.</param>
        /// <returns>A boolean value indicating whether the load was successful.</returns>
        /// <remarks>The loaded map is set as the static Current property from where it can be accessed.</remarks>
        public static async Task<bool> Load(string mapId)
        {
            if (!Helpers.Comms.User.IsLoggedIn)
            {
                Debug.LogError("Map :: New :: User not logged in. Unable to create new map.");
                Current = null;
                return false;
            }
            
            try
            {
                var mapData = await Helpers.Comms.UserContent.LoadMap(mapId);

                if (Current != null)
                    Current.Unload();

                Current = new Map(mapData);

                OnMapLoaded?.Invoke(true);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogWarningFormat("Map :: Load :: Failed to load map. {0} : {1}", e.GetType().FullName, e.Message);
                OnMapLoaded?.Invoke(false);
                return false;
            }
        }

        public async Task Render()
        {
            if (_mapData == null)
            {
                Debug.LogError("Map :: Render :: Unable to render map as no map data has been set. " +
                               "Call New or Load first.");
                return;
            }

            try
            {
                GameTerrain.Current.LoadDefaultTexture(_mapData.terrainTextureAddress);
                await GameTerrain.Current.LoadTerrainTextures(_mapData.terrain.terrainLayers.ToArray());

                TimeController.Current.LightingMode = (LightingMode) _mapData.lightingMode;
                TimeController.Current.CurrentTime = _mapData.time;
                WindController.Current.CurrentWind = _mapData.wind;
                WindController.Current.Rotation = _mapData.windDirection;

                foreach (var x in _mapData.worldObjects) await WorldObjectFactory.CreateFromMapObject(x);

                foreach (var x in _mapData.splineObjects.Where(x =>
                             (WorldObjectType) x.objectType == WorldObjectType.River))
                {
                    // Load rivers before roads as they create more terrain height variations
                    await WorldObjectFactory.CreateFromMapObject(x);
                }

                foreach (var x in _mapData.splineObjects.Where(x =>
                             (WorldObjectType) x.objectType == WorldObjectType.Road))
                {
                    // Load roads after rivers so they correctly adapt to the carved terrain
                    await WorldObjectFactory.CreateFromMapObject(x);
                }

                foreach (var x in _mapData.scatterAreas) await WorldObjectFactory.CreateFromMapObject(x);

                GameTerrain.Current.LoadSplatMaps(_mapData.terrain.splatWidth, _mapData.terrain.splatHeight,
                    _mapData.terrain.splatMaps);
            }
            catch (Exception e)
            {
                Debug.LogErrorFormat("Map :: Render :: Error rendering: {0}: {1}", e.GetType().FullName, e.Message);
            }
            
            OnMapRendered?.Invoke();
        }

        /// <summary>
        /// Serializes this map into data objects and saves it.
        /// </summary>
        /// <returns>A boolean value indicating whether saving was successful.</returns>
        public async Task<bool> Save()
        {
            // Update the basic properties in the map data
            _mapData.dateSaved = DateTime.Now.ToFileTimeUtc();
            _mapData.terrainTextureAddress = GameTerrain.Current.TerrainTextureAddress;
            _mapData.time = TimeController.Current.CurrentTime;
            _mapData.wind = WindController.Current.CurrentWind;
            _mapData.windDirection = WindController.Current.Rotation;
            _mapData.lightingMode = (int) TimeController.Current.LightingMode;
            _mapData.terrain = GameTerrain.Current.ToDataObject();

            // Serialize World Objects
            _mapData.worldObjects.Clear();
            var worldObjects = UnityEngine.Object.FindObjectsOfType<WorldObject>();
            Array.ForEach(worldObjects, x => _mapData.worldObjects.Add(x.ToDataObject() as WorldObjectData));

            // Serialize Ram Objects
            _mapData.splineObjects.Clear();
            var ramObjects = UnityEngine.Object.FindObjectsOfType<RamObject>();
            Array.ForEach(ramObjects, x => _mapData.splineObjects.Add(x.ToDataObject() as SplineObjectData));

            // Serialize Scalable Objects
            _mapData.scatterAreas.Clear();
            var scatterObjects = UnityEngine.Object.FindObjectsOfType<PolygonObject>();
            Array.ForEach(scatterObjects, x => _mapData.scatterAreas.Add(x.ToDataObject() as ScatterAreaData));

            // Save the map data
            var result = await Helpers.Comms.UserContent.SaveMap(_mapData);

            OnMapSaved?.Invoke(result);
            return result;
        }

        /// <summary>
        /// Saves this map as a copy of the original.
        /// </summary>
        /// <param name="newName">A name for the new map.</param>
        /// <returns>A boolean value indicating whether the save was successful.</returns>
        public async Task<bool> SaveCopy(string newName)
        {
            // Generate new ID so this is treated as a new map
            _mapData.id = Guid.NewGuid().ToString();
            
            // Set the new name
            _mapData.name = newName;
            
            // Set other map properties
            _mapData.dateCreated = DateTime.Now.ToFileTimeUtc();
            _mapData.modifiedByUUID = Helpers.Comms.User.Id;
            _mapData.modifiedByDisplayname = Helpers.Comms.User.Username;

            return await Save();
        }

        /// <summary>
        /// Destroys all world objects on the current map.
        /// </summary>
        public void Unload()
        {
            var worldObjects = UnityEngine.Object.FindObjectsOfType<WorldObjectBase>();
            foreach (var worldObject in worldObjects) worldObject.Destroy();
        }

        #endregion
    }
}