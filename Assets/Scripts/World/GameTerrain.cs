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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TT.Data;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace TT.World
{
    [RequireComponent(typeof(Terrain))]
    public class GameTerrain : MonoBehaviour
    {
        #region Static members

        // Singleton
        public static GameTerrain Current { get; private set; }

        #endregion


        #region Public properties

        public string TerrainTextureAddress { get; private set; }
        public Vector3 TerrainSize => _terrain.terrainData.size;
        public float MinPlacementElevation { get; private set; }

        #endregion


        #region Private fields

        // The altitude scale at which the terrain is levelled
        private const float TERRAINLEVEL = 0.1f;

        // Private fields
        private readonly List<string> _terrainLayers = new List<string>();
        private Terrain _terrain;

        #endregion


        #region Event handlers

        // Set up the class
        void Awake()
        {
            _terrain = GetComponent<Terrain>();
            Current = this;

            // Initialise the terrain at 0.1 times the height scale
            SetTerrainHeights(TERRAINLEVEL);

            MinPlacementElevation = OffsetAltitude(0) + 0.01f;
        }

        #endregion


        #region Public methods

        // Resets the terrain textures and loads the default texture specified
        public void LoadDefaultTexture(string defaultTextureAddress)
        {
            TerrainTextureAddress = defaultTextureAddress;

            StartCoroutine(LoadTextureIntoTerrain(defaultTextureAddress, true));
            ResetSplatMaps();
        }

        // Loads the specified texture and optionally replaces an already loaded texture
        public void LoadTerrainTexture(string textureAddress, int replaceLayer = -1)
        {
            if (replaceLayer == 0)
            {
                TerrainTextureAddress = textureAddress;
            }

            StartCoroutine(LoadTextureIntoTerrain(textureAddress, false, replaceLayer));
        }

        // Returns the terrain index of the specified texture address, if present
        public int GetTextureIndex(string textureAddress)
        {
            return _terrainLayers.IndexOf(textureAddress);
        }

        // Koads the specified terrain texture and calls back once finished
        public void LoadTextureWithCallback(string textureAddress, Action<string> callback)
        {
            StartCoroutine(LoadTextureWithCallbackCoroutine(textureAddress, callback));
        }

        // Offsets a given altitude to be that altitude above the terrain
        public float OffsetAltitude(float altitude)
        {
            return altitude + (_terrain.terrainData.heightmapScale.y * TERRAINLEVEL);
        }

        public MapTerrain ToMapObject()
        {
            var terrainData = _terrain.terrainData;
            return new MapTerrain()
            {
                terrainLayers = _terrainLayers,
                splatMaps = SaveSplatMaps(),
                splatWidth = terrainData.alphamapWidth,
                splatHeight = terrainData.alphamapHeight
            };
        }

        /// <summary>
        /// Loads the terrain from a saved map object.
        /// </summary>
        /// <param name="mapTerrain">The saved map object to load the terrain from.</param>
        /// <returns>An awaitable task.</returns>
        public async Task FromMapObject(MapTerrain mapTerrain)
        {
            Debug.Log("GameTerrain :: FromMapObject");
            
            await LoadTerrainTextures(mapTerrain.terrainLayers.ToArray());
            LoadSplatMaps(mapTerrain.splatWidth, mapTerrain.splatHeight, mapTerrain.splatMaps);
        }

        /// <summary>
        /// Takes a list of alphamap textures and replaces the current terrain's alphamaps with them.
        /// </summary>
        /// <param name="undoData">The data to load into the terrain alpha maps.</param>
        public void LoadAlphamapTextures(PaintUndoData undoData)
        {
            var width = undoData.MaxX - undoData.MinX;
            var height = undoData.MaxY - undoData.MinY;

            // Reset the terrain
            float[,,] splatMaps = Terrain.activeTerrain.terrainData.GetAlphamaps(undoData.MinX, undoData.MinY, width, height);
            int layerCount = splatMaps.GetLength(2);
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int z = 0; z < layerCount; z++)
                    {
                        // Texture index is incremented every 4 items
                        // Each splat contains one channel within the pixel
                        if (undoData.AlphamapTextures.Length > z / 4)
                        {
                            splatMaps[y, x, z] = undoData.AlphamapTextures[z / 4].GetPixel(x + undoData.MinX, y + undoData.MinY)[z % 4];
                        }
                    }
                }
            }
            Terrain.activeTerrain.terrainData.SetAlphamaps(undoData.MinX, undoData.MinY, splatMaps);
        }

        /// <summary>
        /// Resets the terrain heights to their default level, and calls on all RAM objects to carve the terrain again.
        /// </summary>
        /// <remarks>WARNING this can be slow with a large terrain with many RAM objects.</remarks>
        public void ResetMapWithRamRecarve()
        {
            Debug.Log("GameTerrain :: ResetHeightswithRamCarve");

            SetTerrainHeights(TERRAINLEVEL);

            // Tell all RAM objects to carve the terrain
            WorldObjectBase.All.Where(x => x is RamObject).Cast<RamObject>().ToList().ForEach(x => { x.CarveTerrain(); x.PaintTerrain(true); x.PaintTerrain(); });
        }

        #endregion


        #region Private methods

        // Loads a terrain texture, adds it to the list of textures, and calls the specified callback method
        private IEnumerator LoadTextureWithCallbackCoroutine(string textureAddress, Action<string> callback)
        {
            // Do not load the same texture twice
            if (_terrainLayers.Contains(textureAddress))
            {
                callback?.Invoke(textureAddress);
                yield break;
            }

            var handle = Addressables.LoadAssetAsync<TerrainLayer>(textureAddress);

            // Wait until done
            if (!handle.IsDone)
            {
                yield return handle;
            }

            // Break out if failed
            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogFormat("Failed to load terrain texture. Error returned: [{0}]", handle.OperationException.Message);
                yield break;
            }

            // Create new array, copying existing layers
            TerrainLayer[] newLayers = new TerrainLayer[_terrain.terrainData.terrainLayers.Length + 1];
            _terrain.terrainData.terrainLayers.CopyTo(newLayers, 0);
            var index = newLayers.Length - 1;
            _terrainLayers.Add(textureAddress);

            // Set up the new layer and apply the changes to the terrain
            newLayers[index] = handle.Result;

            // Apply the changes
            _terrain.terrainData.terrainLayers = newLayers;

            // Call the method that was passed in
            callback?.Invoke(textureAddress);
        }

        // Loads a texture into the terrain - optionally clears any existing terrains (clearLayers)
        private IEnumerator LoadTextureIntoTerrain(string textureAddress, bool clearLayers = false, int replaceLayer = -1)
        {
            var handle = Addressables.LoadAssetAsync<TerrainLayer>(textureAddress);

            // Wait until done
            if (!handle.IsDone)
            {
                yield return handle;
            }

            // Break out if failed
            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogFormat("Failed to load terrain texture. Error returned: [{0}]", handle.OperationException.Message);
                yield break;
            }

            // Create new array, copying existing layers if not clearing
            var index = -1;
            TerrainLayer[] newLayers;

            if (clearLayers)
            {
                // Clear all existing layers and add in the new texture
                newLayers = new TerrainLayer[1];
                index = 0;
                _terrainLayers.Clear();
                _terrainLayers.Add(textureAddress);
            }
            else if (replaceLayer >= 0)
            {
                // Replace an existing layer
                newLayers = new TerrainLayer[_terrain.terrainData.terrainLayers.Length];
                _terrain.terrainData.terrainLayers.CopyTo(newLayers, 0);
                index = replaceLayer;
                _terrainLayers[index] = textureAddress;
            }
            else
            {
                // Add a new layer
                newLayers = new TerrainLayer[_terrain.terrainData.terrainLayers.Length + 1];
                _terrain.terrainData.terrainLayers.CopyTo(newLayers, 0);
                index = newLayers.Length - 1;
                _terrainLayers.Add(textureAddress);
            }

            // Set up the new layer and apply the changes to the terrain
            newLayers[index] = handle.Result;

            // Apply the changes
            _terrain.terrainData.terrainLayers = newLayers;
        }

        // Resets the terrain heights across the entire map
        private void SetTerrainHeights(float level)
        {
            Debug.Log("GameTerrain :: SetTerrainHeights");
            
            var terrainData = _terrain.terrainData;
            var heightmapResolution = terrainData.heightmapResolution;
            float[,] heights = new float[heightmapResolution, heightmapResolution];

            for (int x = 0; x < heightmapResolution; x++)
            {
                for (int z = 0; z < heightmapResolution; z++)
                {
                    heights[z, x] = level;
                }
            }
            terrainData.SetHeights(0, 0, heights);
        }

        // Resets the terrain splat maps to show only the base layer
        private void ResetSplatMaps()
        {
            Debug.Log("GameTerrain :: ResetSplatMaps");
            
            var terrainData = _terrain.terrainData;

            float[,,] alpha = new float[terrainData.alphamapWidth, terrainData.alphamapHeight, terrainData.terrainLayers.Length];

            // Reset terrain splat maps
            for (int x = 0; x < terrainData.alphamapWidth; x++)
            {
                for (int y = 0; y < terrainData.alphamapHeight; y++)
                {
                    alpha[x, y, 0] = 1;
                    for (int z = 1; z < terrainData.alphamapTextureCount; z++)
                    {
                        alpha[x, y, z] = 0;
                    }
                }
            }
            terrainData.SetAlphamaps(0, 0, alpha);
        }

        /// <summary>
        /// Loads a set of addresses into the terrain in the given order. This replaces all existing layers.
        /// </summary>
        /// <param name="addresses"></param>
        /// <returns></returns>
        public async Task LoadTerrainTextures(string[] addresses)
        {
            Debug.Log("GameTerrain :: LoadTerrainTextures");

            TerrainLayer[] newLayers = new TerrainLayer[addresses.Length];
            _terrainLayers.Clear();

            for (int i = 0; i < addresses.Length; i++)
            {
                var handle = Addressables.LoadAssetAsync<TerrainLayer>(addresses[i]);
                await handle.Task;

                // Break out if failed
                if (handle.Status != AsyncOperationStatus.Succeeded)
                {
                    Debug.LogFormat("GameTerrain :: LoadTextures :: Failed to load terrain texture at [{0}]. Error returned: [{1}]", addresses[i], handle.OperationException.Message);
                }

                // Create new array, copying existing layers if not clearing
                _terrainLayers.Add(addresses[i]);
                newLayers[i] = handle.Result;
            }

            // Apply the changes
            _terrain.terrainData.terrainLayers = newLayers;
        }

        /// <summary>
        /// Loads the splat values provided and paints them onto the terrain.
        /// </summary>
        /// <param name="width">The width of the splat map.</param>
        /// <param name="height">The height of the splat map.</param>
        /// <param name="splatValues">A list of values to paint.</param>
        public void LoadSplatMaps(int width, int height, List<Vector4> splatValues)
        {
            Debug.Log("GameTerrain :: LoadSplatMaps");

            // First reset back to normal
            ResetSplatMaps();

            float[,,] splatArray = _terrain.terrainData.GetAlphamaps(0, 0, width, height);

            foreach (var splatValue in splatValues)
            {
                // Set the cell's splat value, and reduce the base layer by the same amount
                splatArray[(int)splatValue.x, (int)splatValue.y, (int)splatValue.z] = splatValue.w;
                splatArray[(int)splatValue.x, (int)splatValue.y, 0] -= splatValue.w;
            }

            _terrain.terrainData.SetAlphamaps(0, 0, splatArray);
        }

        /// <summary>
        /// Transforms the current terrain's splat maps to a list of Vector4 values ready for saving.
        /// </summary>
        /// <returns>A list of splat values that have been painted on the current terrain.</returns>
        private List<Vector4> SaveSplatMaps()
        {
            Debug.Log("GameTerrain :: SaveSplatMaps");

            var terrainData = _terrain.terrainData;
            int width = terrainData.alphamapWidth;
            int height = terrainData.alphamapHeight;
            int layers = terrainData.terrainLayers.Length;
            List<Vector4> outputArray = new List<Vector4>();
            float[,,] splatArray = _terrain.terrainData.GetAlphamaps(0, 0, width, height);

            // Reset terrain splat maps
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int z = 1; z < layers; z++)
                    {
                        if (splatArray[x, y, z] > 0f) outputArray.Add(new Vector4(x, y, z, splatArray[x, y, z]));
                    }
                }
            }

            return outputArray;
        }
        #endregion
    }
}