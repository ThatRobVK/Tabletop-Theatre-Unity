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
using TT.Data;
using TT.MapEditor;
using UnityEngine;

namespace TT.World
{
    public class WorldObject : WorldObjectBase
    {

        #region Private fields

        private string _openAnimation;
        private string _closeAnimation;
        private OpenCloseState _openCloseState = OpenCloseState.Close;
        private Vector3 _undoPosition;
        private Vector3 _dragPositionOffset;

        #endregion


        #region Event handlers

        /// <summary>
        /// Called when dragging is started. Ignore raycasts on this object.
        /// </summary>
        /// <param name="obj"></param>
        private void HandleStartDrag(DraggableObject obj)
        {
            _undoPosition = Position;
            SetLayerRecursive(gameObject, Helpers.IgnoreRaycastLayer);
        }

        /// <summary>
        /// Called when dragging is finished. Return to normal game layer.
        /// </summary>
        /// <param name="obj"></param>
        private void HandleStopDrag(DraggableObject obj)
        {
            if (Position != _undoPosition) UndoController.RegisterAction(ActionType.Move, ObjectId, _undoPosition);

            SetLayerRecursive(gameObject, GameLayer);
        }

        /// <summary>
        /// Called when this object is dragged. Move to the position of the mouse.
        /// </summary>
        /// <param name="arg2"></param>
        private void HandleDrag(DraggableObject arg2)
        {
            var newPosition = AutomaticElevation ? Helpers.GetWorldPointFromMouse(Helpers.StackableMask) : Helpers.GetWorldPointFromMouse(transform.position.y);
            newPosition -= _dragPositionOffset;

            MoveTo(newPosition);
        }

        /// <summary>
        /// Called when the time on the map has changed. Update the lights.
        /// </summary>
        /// <param name="time">Ignored.</param>
        private void HandleTimeChanged(float time)
        {
            UpdateLights();
        }

        #endregion


        #region Public methods

        public WorldObject()
        {
            OptionValues = new Dictionary<WorldObjectOption, object>();
        }

        public override void Initialise(ContentItem contentItem, int itemIndex)
        {
            base.Initialise(contentItem, itemIndex);
            
            gameObject.name = name;
            var thisTransform = transform;
            thisTransform.position = Vector3.zero + Vector3.up * GameTerrain.Current.OffsetAltitude(0);
            thisTransform.rotation = Quaternion.identity;
            thisTransform.localScale = Vector3.one;

            // If this object is draggable, attach to the drag event
            var thisDraggable = GetComponent<DraggableObject>();
            if (thisDraggable)
            {
                thisDraggable.OnStartDrag += HandleStartDrag;
                thisDraggable.OnDrag += HandleDrag;
                thisDraggable.OnStopDrag += HandleStopDrag;
            }

            InitialiseAnimations();
            InitialiseOptions();
            InitialiseLights();
            InitialiseLods();
            UpdateLights();

            // Apply the scale multiplier for this content item
            ScaleMultiplier = contentItem.Scale;
            base.ScaleTo(1);

            base.Select();
            PickUp();
        }

        public void Initialise(MapWorldObject mapObject)
        {
            FromMapObject(mapObject);
            
            // If this object is draggable, attach to the drag event
            var draggable = GetComponent<DraggableObject>();
            if (draggable)
            {
                draggable.OnStartDrag += HandleStartDrag;
                draggable.OnDrag += HandleDrag;
                draggable.OnStopDrag += HandleStopDrag;
            }

            _openCloseState = mapObject.openCloseState ? OpenCloseState.Open : OpenCloseState.Close;
            AutomaticElevation = mapObject.automaticElevation;

            InitialiseLights();
            InitialiseAnimations();
            InitialiseLods();
            UpdateLights();
            if (OptionValues.ContainsKey(WorldObjectOption.OpenClose)) PlayAnimation((bool)OptionValues[WorldObjectOption.OpenClose] ? OpenCloseState.Open : OpenCloseState.Close);
        }


        /// <summary>
        /// Creates a clone of another object
        /// </summary>
        public override void CloneFrom(WorldObjectBase fromObject, string prefabAddress = null, bool generateNewObjectId = true, ContentItem contentItem = null)
        {
            base.CloneFrom(fromObject, prefabAddress, generateNewObjectId, contentItem);

            // Set the lights based on the loaded options
            InitialiseLights();
            InitialiseAnimations();
            InitialiseLods();
            UpdateLights();

            // If this object is draggable, attach to the drag event
            var thisDraggable = GetComponent<DraggableObject>();
            if (thisDraggable)
            {
                thisDraggable.OnStartDrag += HandleStartDrag;
                thisDraggable.OnDrag += HandleDrag;
                thisDraggable.OnStopDrag += HandleStopDrag;
            }

        }

        /// <summary>
        /// Apply an option value and update the object.
        /// </summary>
        /// <param name="option">The option to set.</param>
        /// <param name="value">The value to set the option to.</param>
        public override void SetOption(WorldObjectOption option, object value)
        {
            base.SetOption(option, value);

            if (option == WorldObjectOption.OpenClose)
            {
                PlayAnimation((bool)value ? OpenCloseState.Open : OpenCloseState.Close);
            }

            UpdateLights();
        }

        /// <summary>
        /// Destroy the object.
        /// </summary>
        public override void Destroy()
        {
            // De-register event handler
            TimeController.TimeChanged -= HandleTimeChanged;

            base.Destroy();
        }

        /// <summary>
        /// Create a MapWorldObject representing this WorldObject.
        /// </summary>
        /// <returns>A MapWorldObject with all values set based on the current state of this WorldObject.</returns>
        public override MapObjectBase ToMapObject()
        {
            var mapObject = ToMapObject<MapWorldObject>();
            mapObject.openCloseState = _openCloseState == OpenCloseState.Open;
            mapObject.automaticElevation = AutomaticElevation;

            return mapObject;
        }

        /// <summary>
        /// Plays an open or close animation on this object.
        /// </summary>
        /// <param name="openClose">Whether to open or close the object.</param>
        public void PlayAnimation(OpenCloseState openClose = OpenCloseState.Toggle)
        {
            var thisAnimation = GetComponentInChildren<Animation>();
            if (thisAnimation != null)
            {
                var targetState = openClose == OpenCloseState.Open || (openClose == OpenCloseState.Toggle && _openCloseState == OpenCloseState.Close);

                if (targetState && _openCloseState != OpenCloseState.Open && !string.IsNullOrEmpty(_openAnimation))
                {
                    thisAnimation.PlayQueued(_openAnimation, QueueMode.CompleteOthers, PlayMode.StopSameLayer);
                    _openCloseState = OpenCloseState.Open;
                }
                else if (_openCloseState != OpenCloseState.Close && !string.IsNullOrEmpty(_closeAnimation))
                {
                    thisAnimation.PlayQueued(_closeAnimation, QueueMode.CompleteOthers, PlayMode.StopSameLayer);
                    _openCloseState = OpenCloseState.Close;
                }
            }
        }

        /// <summary>
        /// Called when the object is clicked.
        /// </summary>
        /// <param name="clickedObject">The object that was clicked.</param>
        /// <param name="position">The position of the mouse at the time of the click.</param>
        public override void Click(GameObject clickedObject, Vector3 position)
        {
            // Store the offset of the mouse to the object's position, ignoring the Y axis as the hit may happen higher than the base Position
            position.y = Position.y;
            _dragPositionOffset = position - Position;

            base.Click(clickedObject, position);
        }

        #endregion


        #region Private methods

        /// <summary>
        /// Starts or stops all the lights, particles and lens flares based on the light options and time.
        /// </summary>
        private void UpdateLights()
        {
            var lightsEnabled = true;

            if (OptionValues.ContainsKey(WorldObjectOption.LightsMode))
            {
                // Enable lights when:
                // Object light mode is auto and the moon is out
                // OR Light is always on
                // OR Map lighting mode isn't ambient (i.e. low light or indoor)
                lightsEnabled = ((LightsMode)OptionValues[WorldObjectOption.LightsMode] == LightsMode.Auto && TimeController.Current.MoonOut)
                                || (LightsMode)OptionValues[WorldObjectOption.LightsMode] == LightsMode.On
                                || TimeController.Current.LightingMode != LightingMode.Ambient;
            }

            var thisLights = GetComponentsInChildren<Light>();
            if (thisLights != null)
            {
                foreach (var currentLight in thisLights)
                {
                    // Set every light based on options
                    currentLight.enabled = lightsEnabled;
                    currentLight.intensity = OptionValues.ContainsKey(WorldObjectOption.LightsIntensity) ?
                                      (float)OptionValues[WorldObjectOption.LightsIntensity] : currentLight.intensity;
                    currentLight.range = OptionValues.ContainsKey(WorldObjectOption.LightsRange) ?
                                      (float)OptionValues[WorldObjectOption.LightsRange] : currentLight.range;
                    currentLight.color = OptionValues.ContainsKey(WorldObjectOption.LightsColor) ?
                                      (Color)OptionValues[WorldObjectOption.LightsColor] : currentLight.color;
                }
            }

            var thisParticleSystems = GetComponentsInChildren<ParticleSystem>();
            if (thisParticleSystems != null)
            {
                foreach (var currentParticleSystem in thisParticleSystems)
                {
                    // Enable / disable particle systems
                    if (lightsEnabled)
                    {
                        if (!currentParticleSystem.isPlaying) currentParticleSystem.Play(true);
                    }
                    else
                    {
                        if (currentParticleSystem.isPlaying) currentParticleSystem.Stop(true);
                    }
                }
            }

            var thisLensFlares = GetComponentsInChildren<LensFlare>();
            if (thisLensFlares != null)
            {
                foreach (var currentLensFlare in thisLensFlares)
                {
                    // Enable / disable lens flares
                    currentLensFlare.enabled = lightsEnabled;
                }
            }
        }

        /// <summary>
        /// Force the highest Level of Detail (LOD) for all objects
        /// </summary>
        private void InitialiseLods()
        {
            var lodGroup = GetComponent<LODGroup>();
            if (lodGroup != null)
            {
                var lods = lodGroup.GetLODs();
                lods[lods.Length - 1].screenRelativeTransitionHeight = 0;
                lodGroup.SetLODs(lods);
            }
        }

        /// <summary>
        /// Adds options to the OptionValues dictionary based on features of the current object, e.g. lights and animations.
        /// </summary>
        private void InitialiseOptions()
        {
            // Init light options if the object has lights
            var lights = GetComponentsInChildren<Light>();
            if (lights != null && lights.Length > 0)
            {
                // Add values from the lights
                OptionValues.Add(WorldObjectOption.LightsMode, LightsMode.Auto);
                OptionValues.Add(WorldObjectOption.LightsIntensity, 1f * ContentItem.Lights);
                OptionValues.Add(WorldObjectOption.LightsRange, 10f * ContentItem.Lights);
                OptionValues.Add(WorldObjectOption.LightsColor, lights[0].color);
            }

            // Init open close options if the object has open and close animations
            if (!string.IsNullOrEmpty(_openAnimation) && !string.IsNullOrEmpty(_closeAnimation))
            {
                OptionValues.Add(WorldObjectOption.OpenClose, _openCloseState == OpenCloseState.Open);
            }
        }

        /// <summary>
        /// Sets up all the Lights on this object.
        /// </summary>
        private void InitialiseLights()
        {
            // Get all light related objects
            var thisLights = GetComponentsInChildren<Light>();
            if (thisLights != null)
            {
                foreach (var currentLight in thisLights)
                {
                    currentLight.renderMode = LightRenderMode.ForcePixel;
                    currentLight.shadows = LightShadows.Hard;
                }

                // If this object has lights, hook into the time changed event to do auto lights
                TimeController.TimeChanged += HandleTimeChanged;
            }
        }

        /// <summary>
        /// Sets up the animations on this object.
        /// </summary>
        private void InitialiseAnimations()
        {
            var thisAnimation = GetComponentInChildren<Animation>();
            if (thisAnimation != null)
            {
                // Find and store the open and close animation names
                foreach (AnimationState state in thisAnimation)
                {
                    if (state.name.Contains("open"))
                    {
                        _openAnimation = state.name;
                    }
                    else if (state.name.Contains("close"))
                    {
                        _closeAnimation = state.name;
                    }
                }

                // Set current state based on default animation
                _openCloseState = (thisAnimation.clip.name == _closeAnimation) ? OpenCloseState.Open : OpenCloseState.Close;

                // Do not auto play animation
                thisAnimation.playAutomatically = false;
                thisAnimation.Stop();
            }
         }
        #endregion



    }

    public enum LightsMode
    {
        Auto,
        On,
        Off
    }
}