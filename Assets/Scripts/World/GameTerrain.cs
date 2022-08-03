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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using TT.Shared.UserContent;
using TerrainData = TT.Shared.UserContent.TerrainData;

namespace TT.World
{
    /// <summary>
    /// Attached to the Terrain object. Methods and properties for the game and editor to interact with the terrain.
    /// </summary>
    [RequireComponent(typeof(Terrain))]
    public class GameTerrain : MonoBehaviour
    {
        #region Static members

        /// <summary>
        /// Singleton instance of the GameTerrain.
        /// </summary>
        public static GameTerrain Current { get; private set; }

        #endregion


        #region Public properties

        /// <summary>
        /// The Addressables address of the main terrain texture.
        /// </summary>
        public string TerrainTextureAddress { get; private set; }

        /// <summary>
        /// The minimum elevation at which objects should be placed.
        /// </summary>
        public float MinPlacementElevation { get; private set; }

        #endregion


        #region Private fields

        private const float TERRAIN_LEVEL = 0.1f;
        private readonly List<string> _terrainLayers = new List<string>();
        private Terrain _terrain;

        #endregion


        #region Lifecycle events

        void Awake()
        {
            _terrain = GetComponent<Terrain>();
            Current = this;

            // Initialise the terrain at 0.1 times the height scale
            SetTerrainHeights(TERRAIN_LEVEL);

            MinPlacementElevation = OffsetAltitude(0) + 0.01f;
        }

        #endregion


        #region Public methods

        /// <summary>
        /// Resets the terrain textures and loads the default texture specified
        /// </summary>
        /// <param name="defaultTextureAddress">The Addressables address of the texture to load.</param>
        public void LoadDefaultTexture(string defaultTextureAddress)
        {
            TerrainTextureAddress = defaultTextureAddress;

            StartCoroutine(LoadTextureIntoTerrain(defaultTextureAddress, true));
            ResetSplatMaps();
        }

        /// <summary>
        /// Loads the specified texture and optionally replaces an already loaded texture.
        /// </summary>
        /// <param name="textureAddress">The addressables address of the texture to load.</param>
        /// <param name="replaceLayer">If 0 or higher, the texture at this index will be replaced by the texture
        ///     specified.</param>
        public void LoadTerrainTexture(string textureAddress, int replaceLayer = -1)
        {
            if (replaceLayer == 0)
            {
                TerrainTextureAddress = textureAddress;
            }

            StartCoroutine(LoadTextureIntoTerrain(textureAddress, false, replaceLayer));
        }

        /// <summary>
        /// Finds a texture with the specified address in the terrain returning its index if found.
        /// </summary>
        /// <param name="textureAddress">The addressables address of the texture to find.</param>
        /// <returns>The index of the address, or -1 if not found.</returns>
        /// <remarks>This method can be used while waiting for a terrain texture to load, -1 indicating it hasn't yet
        ///     loaded, and another value indicating loading has completed.</remarks>
        public int GetTextureIndex(string textureAddress)
        {
            return _terrainLayers.IndexOf(textureAddress);
        }

        /// <summary>
        /// Loads the specified terrain texture and calls back once finished.
        /// </summary>
        /// <param name="textureAddress">The Addressables address of the texture to load.</param>
        /// <param name="callback">The method to call once loading has completed. The string parameter of the callback
        ///     will be populated with the Addressables address to allow consuming code to match callbacks to requests.
        ///     </param>
        public void LoadTextureWithCallback(string textureAddress, Action<string> callback)
        {
            StartCoroutine(LoadTextureWithCallbackCoroutine(textureAddress, callback));
        }

        /// <summary>
        /// Converts an altitude relative to the game terrain to an altitude in world space. 
        /// </summary>
        /// <param name="altitude">An altitude above the game terrain to transform.</param>
        /// <returns>A Y coordinate in world space representing the specified altitude above the game terrain.</returns>
        public float OffsetAltitude(float altitude)
        {
            return altitude + (_terrain.terrainData.heightmapScale.y * TERRAIN_LEVEL);
        }

        /// <summary>
        /// Saves the current state into a data object and returns it.
        /// </summary>
        /// <returns>A TerrainData object representing the current state of the terrain.</returns>
        public TerrainData ToDataObject()
        {
            var terrainData = _terrain.terrainData;
            return new TerrainData()
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
        /// <param name="terrainData">The saved map object to load the terrain from.</param>
        /// <returns>An awaitable task.</returns>
        public async Task FromMapObject(TerrainData terrainData)
        {
            //TODO: Check loading of terrain - this method is unused, does this even need the root terrain field, etc?
            Debug.Log("GameTerrain :: FromMapObject");
            
            await LoadTerrainTextures(terrainData.terrainLayers.ToArray());
            LoadSplatMaps(terrainData.splatWidth, terrainData.splatHeight, terrainData.splatMaps);
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
            //TODO: Optimise the working with RAM objects, this is expensive and needs to be optimised.
            
            Debug.Log("GameTerrain :: ResetHeightswithRamCarve");

            SetTerrainHeights(TERRAIN_LEVEL);

            // Tell all RAM objects to carve the terrain
            WorldObjectBase.All.Where(x => x is RamObject).Cast<RamObject>().ToList().ForEach(x => { x.CarveTerrain(); x.PaintTerrain(true); x.PaintTerrain(); });
        }

        #endregion


        #region Private methods

        /// <summary>
        /// Loads a terrain texture, adds it to the list of textures, and calls the specified callback method.
        /// </summary>
        /// <param name="textureAddress">The Addressables address of the texture to load.</param>
        /// <param name="callback">The callback to call when finished.</param>
        /// <returns>A coroutine enumerator.</returns>
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

        /// <summary>
        /// Loads a texture into the terrain - optionally clears any existing terrains (clearLayers).
        /// </summary>
        /// <param name="textureAddress">The Addressables address of the texture to load.</param>
        /// <param name="clearLayers">Whether to remove all textures first.</param>
        /// <param name="replaceLayer">A layer index to be replaced by the specified textureAddress.</param>
        /// <returns>A coroutine enumerator.</returns>
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

        /// <summary>
        /// Resets the terrain heights across the entire map.
        /// </summary>
        /// <param name="level">The level to set the terrain to.</param>
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

        /// <summary>
        /// Resets the terrain splat maps to show only the base layer.
        /// </summary>
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
        /// <param name="addresses">A list of Addressables addresses to load into the terrain.</param>
        /// <returns>An awaitable task.</returns>
        public async Task LoadTerrainTextures(string[] addresses)
        {
            Debug.Log("GameTerrain :: LoadTerrainTextures");

            TerrainLayer[] newLayers = new TerrainLayer[addresses.Length];
            _terrainLayers.Clear();

            for (int i = 0; i < addresses.Length; i++)
            {
                // Skip loading of the base texture
                if (addresses[i].Equals(TerrainTextureAddress))
                    continue;
                
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
        public void LoadSplatMaps(int width, int height, List<VectorData> splatValues)
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
        private List<VectorData> SaveSplatMaps()
        {
            Debug.Log("GameTerrain :: SaveSplatMaps");

            var terrainData = _terrain.terrainData;
            int width = terrainData.alphamapWidth;
            int height = terrainData.alphamapHeight;
            int layers = terrainData.terrainLayers.Length;
            List<VectorData> outputArray = new List<VectorData>();
            float[,,] splatArray = _terrain.terrainData.GetAlphamaps(0, 0, width, height);

            // Reset terrain splat maps
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int z = 1; z < layers; z++)
                    {
                        if (splatArray[x, y, z] > 0f) outputArray.Add(new VectorData(x, y, z, splatArray[x, y, z]));
                    }
                }
            }

            return outputArray;
        }
        #endregion
    }
}