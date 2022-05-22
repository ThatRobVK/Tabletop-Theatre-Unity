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
using System.Collections.Generic;
using TT.Data;
using TT.InputMapping;
using TT.MapEditor;
using TT.UI;
using TT.World;
using UnityEngine;

namespace TT.State
{
    public class TerrainPaintState : StateBase
    {

        #region Private fields

        private float _widthMultiplier;
        private float _heightMultiplier;
        private TerrainPaintMarker _marker;
        private Vector3 _lastMousePosition;
        private int _layerIndex;
        private readonly List<Texture2D> _undoAlphaMaps = new List<Texture2D>();
        private int _minX, _minY, _maxX, _maxY;
        private bool _isDirty;

        #endregion


        #region Public properties

        /// <summary>
        /// The radius of the brush.
        /// </summary>
        public int BrushRadius = 20;
        /// <summary>
        /// How far from the edge of the brush the smoothing starts. 0 is no smoothing, 1 is smooth from the centre.
        /// </summary>
        public float Smoothness = .5f;
        /// <summary>
        /// The opacity of the strongest part of the brush (the centre).
        /// </summary>
        public float Opacity = 1f;

        private int _paintLayer;
        /// <summary>
        /// The layer index in the terrainData to paint on.
        /// </summary>
        public int PaintLayer
        {
            get => _paintLayer;
            set
            {
                _paintLayer = value;
                
                // Set a holder until the texture is loaded
                _layerIndex = -1;
                GameTerrain.Current.LoadTextureWithCallback(Content.Current.Combined.TerrainLayers[value].ID, HandleTerrainTextureLoaded);
            }
        }

        public override bool IsPlacementState => true;

        #endregion


        #region Event handlers

        /// <summary>
        /// Called when a new terrain texture has been loaded. Set the terrain layer.
        /// </summary>
        /// <param name="address">The address that was loaded.</param>
        private void HandleTerrainTextureLoaded(string address)
        {
            _layerIndex = GameTerrain.Current.GetTextureIndex(address);
        }

        #endregion


        #region Public methods

        public TerrainPaintState(StateController stateController) : base(stateController)
        { }


        public override void Enable()
        {
            Debug.Log("TerrainPaintState :: Enable");

            _marker = UnityEngine.Object.FindObjectOfType<TerrainPaintMarker>(true);

            // Conversion from world point to splat map - world point * multiplier = splat map
            var terrainData = Terrain.activeTerrain.terrainData; 
            _widthMultiplier = terrainData.alphamapWidth / terrainData.size.x;
            _heightMultiplier = terrainData.alphamapHeight / terrainData.size.z;

            // Show the marker
            _marker.gameObject.SetActive(true);
        }

        public override void Disable()
        {
            Debug.Log("TerrainPaintState :: Disable");

            // Hide the marker
            _marker.gameObject.SetActive(false);
        }

        public override void Update()
        {
            if (_layerIndex == -1)
            {
                CursorController.Current.Wait = true;
                return;
            }
            
            var mousePositionOnMap = Helpers.GetWorldPointFromMouse();
            _marker.PositionAndScale(mousePositionOnMap, BrushRadius);


            if (Input.GetMouseButtonDown(0))
            {
                var terrainData = Terrain.activeTerrain.terrainData;
                var alphamapHeight = terrainData.alphamapHeight;
                var alphamapWidth = terrainData.alphamapWidth;

                // Invert the bounds to guarantee we grab the painted bounds
                _minX = alphamapWidth;
                _minY = alphamapWidth;
                _maxX = 0;
                _maxY = 0;

                // On mouse down, store the current alpha map textures for undo
                _undoAlphaMaps.Clear();
                for (int i = 0; i < terrainData.alphamapTextureCount; i++)
                {
                    var newTexture = new Texture2D(alphamapWidth, alphamapHeight);
                    Graphics.CopyTexture(terrainData.GetAlphamapTexture(i), newTexture);
                    _undoAlphaMaps.Add(newTexture);
                }
                _isDirty = false;
            }

            if (Input.GetMouseButton(0) && mousePositionOnMap != _lastMousePosition && !Helpers.IsPointerOverUIElement())
            {
                PaintTerrainAtPoint(mousePositionOnMap);
                _lastMousePosition = mousePositionOnMap;
                _isDirty = true;
            }

            if (Input.GetMouseButtonUp(0) && _isDirty)
            {
                // If terrain was painted, store the alpha maps for undo
                var data = new PaintUndoData()
                {
                    AlphamapTextures = _undoAlphaMaps.ToArray(),
                    MinX = _minX,
                    MinY = _minY,
                    MaxX = _maxX,
                    MaxY = _maxY
                };
                UndoController.RegisterAction(ActionType.TerrainPaint, Guid.Empty, data);
            }

            if (InputMapper.Current.WorldObjectInput.Cancel)
            {
                // On escape cancel and return to idle state
                ToIdle();
            }
        }

        #endregion


        #region Private methods

        /// <summary>
        /// Paints the terrain at the given position, based on the brush size, smoothness and terrain layer set through properties.
        /// </summary>
        /// <param name="position">The position in world space where to paint. The height (Y) coordinate is ignored.</param>
        private void PaintTerrainAtPoint(Vector3 position)
        {
            // Get clicked location on splat map
            var splatX = (int)(position.x * _widthMultiplier);
            var splatY = (int)(Mathf.Abs(position.z) * _heightMultiplier);

            // Set brush size variables
            int diameter = 2 * BrushRadius;

            // Start position from where we read/write the splat
            int splatStartX = splatX - BrushRadius;
            int splatStartY = splatY - BrushRadius;

            // Read the splat data
            float[,,] splatMaps = Terrain.activeTerrain.terrainData.GetAlphamaps(splatStartX, splatStartY, diameter + 1, diameter + 1);
            int layerCount = splatMaps.GetLength(2);


            for (int x = 0; x < diameter; x++)
            {
                for (int y = 0; y < diameter; y++)
                {
                    // Calculate the current position in the splat map
                    var currentPosRealX = splatStartX + x;
                    var currentPosRealY = splatStartY + y;

                    // Get the distance using Pythagoras, don't do anything if the distance is further than the radius
                    var distance = Mathf.Sqrt(Mathf.Pow(splatX - currentPosRealX, 2) + Mathf.Pow(splatY - currentPosRealY, 2));
                    if (distance > BrushRadius) continue;

                    // Calculate the intensity based on smoothness and distance
                    var smoothedDistance = Mathf.Max(distance - ((1 - Smoothness) * BrushRadius), 0);
                    var intensity = Mathf.Lerp(Opacity, 0, smoothedDistance / BrushRadius);
                    var currentIntensityOtherLayers = 1 - splatMaps[x, y, _layerIndex];
                    var newIntensityOtherLayers = 1 - intensity;

                    // Only increase the painted layer's intensity, don't reduce it
                    if (intensity > splatMaps[x, y, _layerIndex])
                    {
                        for (int z = 0; z < layerCount; z++)
                        {
                            if (z == _layerIndex)
                            {
                                // Set the paint layer's intensity
                                splatMaps[x, y, z] = intensity;
                            }
                            else
                            {
                                // Calculate what proportion of other layers is taken up by this layer, and then calculate
                                // the new proportional value
                                var proportionalIntensity = splatMaps[x, y, z] / currentIntensityOtherLayers;
                                splatMaps[x, y, z] = newIntensityOtherLayers * proportionalIntensity;
                            }
                        }
                    }
                }
            }

            Terrain.activeTerrain.terrainData.SetAlphamaps(splatStartX, splatStartY, splatMaps);

            // Maintain reference to the bounds of the area we've painted
            _minX = Math.Min(_minX, splatStartX);
            _minY = Math.Min(_minY, splatStartY);
            _maxX = Math.Max(_maxX, splatStartX + diameter);
            _maxY = Math.Max(_maxY, splatStartY + diameter);
        }

        public override void ToIdle()
        { 
            StateController.ChangeState(StateType.RamObjectIdle);
        }

        public override void ToPlacement()
        { }

        #endregion

    }
}