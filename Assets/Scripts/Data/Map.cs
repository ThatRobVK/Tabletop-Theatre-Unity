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
using TT.World;
using UnityEngine;
using UnityEngine.Serialization;

namespace TT.Data
{
    [Serializable]
    public class Map
    {
        public static Action OnMapLoaded;

        [NonSerialized] public string Json;

        [FormerlySerializedAs("Name")] public string name;
        [FormerlySerializedAs("Author")] public string author;
        [FormerlySerializedAs("DateCreated")] public long dateCreated;
        [FormerlySerializedAs("DateSaved")] public long dateSaved;
        [FormerlySerializedAs("TerrainTextureAddress")] public string terrainTextureAddress;
        [FormerlySerializedAs("Time")] public float time = 12;
        [FormerlySerializedAs("Wind")] public float wind = 0.1f;
        [FormerlySerializedAs("WindDirection")] public float windDirection;
        [FormerlySerializedAs("LightingMode")] public LightingMode lightingMode;
        [FormerlySerializedAs("Terrain")] public MapTerrain terrain;
        [FormerlySerializedAs("WorldObjects")] public List<MapWorldObject> worldObjects;
        [FormerlySerializedAs("RamObjects")] public List<MapRamObject> ramObjects;
        [FormerlySerializedAs("ScalableObjects")] public List<MapScalableObject> scalableObjects;
        [FormerlySerializedAs("ScatterObjects")] public List<MapScatterObject> scatterObjects;

        public static Map Current { get; private set; }

        // Create a new map object
        public static void Create(string name, string author, string terrainTextureAddress)
        {
            Current = new Map();
            Current.name = name;
            Current.author = author;
            Current.dateCreated = DateTime.Now.ToFileTimeUtc();
            Current.dateSaved = 0;
            Current.terrainTextureAddress = terrainTextureAddress;
            Current.time = 12;
            Current.wind = 0.1f;
            Current.worldObjects = new List<MapWorldObject>();
            Current.ramObjects = new List<MapRamObject>();
            Current.scalableObjects = new List<MapScalableObject>();
            Current.scatterObjects = new List<MapScatterObject>();
            Current.lightingMode = LightingMode.Ambient;
        }

        // Load a map from json
        public static async void Load(string json)
        {
            Current = JsonUtility.FromJson<Map>(json);
            Current.Json = json;

            await GameTerrain.Current.LoadTerrainTextures(Current.terrain.terrainLayers.ToArray());

            TimeController.Current.LightingMode = Current.lightingMode;
            TimeController.Current.CurrentTime = Current.time;
            WindController.Current.CurrentWind = Current.wind;
            WindController.Current.Rotation = Current.windDirection;

            // Load all objects synchronously
            foreach (var x in Current.scalableObjects)
            {
                await WorldObjectFactory.CreateFromMapObject(x);
            }
            foreach (var x in Current.worldObjects)
            {
                await WorldObjectFactory.CreateFromMapObject(x);
            }
            foreach (var x in Current.ramObjects.Where(x => x.type == WorldObjectType.River))
            {
                // Load rivers before roads as they create more terrain height variations
                await WorldObjectFactory.CreateFromMapObject(x);
            }
            foreach (var x in Current.ramObjects.Where(x => x.type == WorldObjectType.Road))
            {
                // Load roads after rivers so they correctly adapt to the carved terrain
                await WorldObjectFactory.CreateFromMapObject(x);
            }
            foreach (var x in Current.scatterObjects)
            {
                await WorldObjectFactory.CreateFromMapObject(x);
            }

            GameTerrain.Current.LoadSplatMaps(Current.terrain.splatWidth, Current.terrain.splatHeight, Current.terrain.splatMaps);

            OnMapLoaded?.Invoke();
        }

        // Serialise the map and all World Objects to json
        public string Save()
        {
            terrain = GameTerrain.Current.ToMapObject();

            // Serialize World Objects
            var worldObjects = UnityEngine.Object.FindObjectsOfType<WorldObject>();
            this.worldObjects.Clear();
            foreach (var worldObject in worldObjects)
            {
                this.worldObjects.Add(worldObject.ToMapObject() as MapWorldObject);
            }

            // Serialize Ram Objects
            var ramObjects = UnityEngine.Object.FindObjectsOfType<RamObject>();
            this.ramObjects.Clear();
            foreach (var ramObject in ramObjects)
            {
                this.ramObjects.Add(ramObject.ToMapObject() as MapRamObject);
            }

            // Serialize Scalable Objects
            var scalableObjects = UnityEngine.Object.FindObjectsOfType<ScalableObject>();
            this.scalableObjects.Clear();
            foreach (var scalableObject in scalableObjects)
            {
                this.scalableObjects.Add(scalableObject.ToMapObject() as MapScalableObject);
            }

            // Serialize Scalable Objects
            var scatterObjects = UnityEngine.Object.FindObjectsOfType<PolygonObject>();
            this.scatterObjects.Clear();
            foreach (var scatterObject in scatterObjects)
            {
                this.scatterObjects.Add(scatterObject.ToMapObject() as MapScatterObject);
            }

            dateSaved = DateTime.Now.ToFileTimeUtc();
            lightingMode = TimeController.Current.LightingMode;
            time = TimeController.Current.CurrentTime;
            wind = WindController.Current.CurrentWind;
            windDirection = WindController.Current.Rotation;

            return JsonUtility.ToJson(Current);
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