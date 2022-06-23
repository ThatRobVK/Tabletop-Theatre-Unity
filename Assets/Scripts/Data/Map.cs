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
using TT.Shared.UserContent;
using TT.World;
using UnityEngine;
using UnityEngine.Serialization;

namespace TT.Data
{
    public class Map
    {
        public static Action<bool> OnMapLoaded;
        public static Action<bool> OnMapSaved;
        

        /// <summary>
        /// The unique identifier of this map.
        /// </summary>
        public Guid Id { get; private set; }
        /// <summary>
        /// The name 
        /// </summary>
        public string Name { get; set; }
        public string Description { get; set; }
        public string Author { get; private set; }
        public string ModifiedBy { get; private set; }
        public DateTime DateCreated { get; private set; }
        public DateTime DateSaved { get; private set; }

        public static Map Current { get; private set; }

        /// <summary>
        /// Creates a new map and sets it to the current map.
        /// </summary>
        /// <param name="name">A user defined name for the map. This can be null and set later through the Name property.</param>
        /// <param name="description">A user defined description for the map. This can be null and set later through the Description property.</param>
        /// <remarks>This method requires that the user is logged in.</remarks>
        public static void New(string name, string description)
        {
            if (!Helpers.Comms.User.IsLoggedIn)
            {
                Debug.LogError("Map :: New :: User not logged in. Unable to create new map.");
                Current = null;
                return;
            }

            var user = Helpers.Comms.User.Username;
            Current = new Map
            {
                Id = Guid.NewGuid(),
                Name = name,
                Description = description,
                Author = user,
                ModifiedBy = user,
                DateCreated = DateTime.Now
            };
        }

        /// <summary>
        /// Loads the map represented by the specified MapData and instantiates all of its objects. This method is
        /// asynchronous and will raise the OnMapLoaded event when completed.
        /// </summary>
        /// <param name="mapData">The map data to load.</param>
        public static async void Load(string mapId)
        {
            try
            {
                var mapData = await Helpers.Comms.UserContent.LoadMap(mapId);
                
                await GameTerrain.Current.LoadTerrainTextures(mapData.terrain.terrainLayers.ToArray());

                TimeController.Current.LightingMode = (LightingMode)mapData.lightingMode;
                TimeController.Current.CurrentTime = mapData.time;
                WindController.Current.CurrentWind = mapData.wind;
                WindController.Current.Rotation = mapData.windDirection;
            
                foreach (var x in mapData.worldObjects)
                {
                    await WorldObjectFactory.CreateFromMapObject(x);
                }
                foreach (var x in mapData.splineObjects.Where(x => (WorldObjectType)x.objectType == WorldObjectType.River))
                {
                    // Load rivers before roads as they create more terrain height variations
                    await WorldObjectFactory.CreateFromMapObject(x);
                }
                foreach (var x in mapData.splineObjects.Where(x => (WorldObjectType)x.objectType == WorldObjectType.Road))
                {
                    // Load roads after rivers so they correctly adapt to the carved terrain
                    await WorldObjectFactory.CreateFromMapObject(x);
                }
                foreach (var x in mapData.scatterAreas)
                {
                    await WorldObjectFactory.CreateFromMapObject(x);
                }

                GameTerrain.Current.LoadSplatMaps(mapData.terrain.splatWidth, mapData.terrain.splatHeight, mapData.terrain.splatMaps);
                
                OnMapLoaded?.Invoke(true);
            }
            catch (Exception e)
            {
                Debug.LogWarningFormat("Map :: Load :: Failed to load map. {0} : {1}", e.GetType().FullName, e.Message);
                OnMapLoaded?.Invoke(false);
            }
        }


        // Serialise the map and all World Objects to json
        public async Task<bool> Save()
        {
            var mapData = new MapData
            {
                id = Id.ToString(),
                name = Name,
                description = Description,
                author = Author,
                modifiedBy = ModifiedBy,
                dateCreated = DateCreated.ToFileTimeUtc(),
                dateSaved = DateTime.Now.ToFileTimeUtc(),
                terrainTextureAddress = GameTerrain.Current.TerrainTextureAddress,
                time = TimeController.Current.CurrentTime,
                wind = WindController.Current.CurrentWind,
                windDirection = WindController.Current.Rotation,
                lightingMode = (int) TimeController.Current.LightingMode,
                terrain = GameTerrain.Current.ToDataObject()
            };

            // Serialize World Objects
            var worldObjects = UnityEngine.Object.FindObjectsOfType<WorldObject>();
            Array.ForEach(worldObjects, x => mapData.worldObjects.Add(x.ToDataObject() as WorldObjectData));

            // Serialize Ram Objects
            var ramObjects = UnityEngine.Object.FindObjectsOfType<RamObject>();
            Array.ForEach(ramObjects, x => mapData.splineObjects.Add(x.ToDataObject() as SplineObjectData));
            
            // Serialize Scalable Objects
            var scatterObjects = UnityEngine.Object.FindObjectsOfType<PolygonObject>();
            Array.ForEach(scatterObjects, x => mapData.scatterAreas.Add(x.ToDataObject() as ScatterAreaData));

            // Save the map data
            var result = await Helpers.Comms.UserContent.SaveMap(mapData);

            if (result) DateSaved = DateTime.FromFileTimeUtc(mapData.dateSaved);
            
            OnMapSaved?.Invoke(result);
            return result;
        }

        // Destroys all World Objects on the current map
        public static void Unload()
        {
            var worldObjects = UnityEngine.Object.FindObjectsOfType<WorldObjectBase>();
            foreach (var worldObject in worldObjects)
            {
                worldObject.Destroy();
            }
        }
    }
}