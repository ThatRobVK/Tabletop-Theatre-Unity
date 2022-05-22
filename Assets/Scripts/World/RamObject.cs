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
using System.Threading.Tasks;
using HighlightPlus;
using TT.Data;
using TT.MapEditor;
using TT.State;
using TT.UI;
using UnityEngine;
using UnityEngine.Serialization;

namespace TT.World
{
    public class RamObject : WorldObjectBase
    {

        #region Editor fields
#pragma warning disable IDE0044 // Make fields read-only

        [FormerlySerializedAs("AddHandleAtEndButton")] [SerializeField] private GameObject addHandleAtEndButton;
        [FormerlySerializedAs("AddHandleAtStartButton")] [SerializeField] private GameObject addHandleAtStartButton;
        [SerializeField] private GameObject handlePrefab;
        [SerializeField] private GameObject colliderPrefab;

#pragma warning restore IDE0044
        #endregion


        #region Private fields

        private readonly List<RamObjectHandle> _handles = new List<RamObjectHandle>();
        private readonly List<MouseTrigger> _colliders = new List<MouseTrigger>();
        private RamObjectHandle _selectedHandle;
        private RamSpline _spline;
        private HighlightEffect _highlighter;
        private string _primaryTerrainAddress;
        private string _secondaryTerrainAddress;
        private float _splinePointElevation;
        // When adding handles at the start or end, the click on the button is also picked up by the 
        // state, causing it to tell the WorldObjectBase to place the handle. This var is set to true
        // for one frame when adding a handle to skip over that click.
        private bool _fixRaceConditionWhenAddingHandles;
        private bool _isDirty;
        private bool _addAtEnd = true;
        private bool _finishedLoading;
        private KeyValuePair<int, Vector3> _undoHandlePosition;
        private static readonly int SlowWaterSpeed = Shader.PropertyToID("_SlowWaterSpeed");

        #endregion


        #region Public properties

        public override bool ContinuousPlacementMode => true;

        public override Vector3 Position
        {
            get
            {
                if (_spline != null)
                {
                    if (_spline.controlPoints.Count == 1)
                    {
                        return _spline.controlPoints[0];
                    }

                    if (_spline.controlPoints.Count > 1)
                    {
                        if (Math.Abs((float)_spline.controlPoints.Count % 2 - 1) < 0.01)
                        {
                            // Get middle point, on uneven it's .5 which rounds down and becomes the index
                            int middlePoint = _spline.controlPoints.Count / 2;
                            return _spline.controlPoints[middlePoint];
                        }
                        else
                        {
                            // Even number, return the centre between the two middle points
                            int middlePoint = _spline.controlPoints.Count / 2 - 1;
                            return Vector3.Lerp(_spline.controlPoints[middlePoint], _spline.controlPoints[middlePoint + 1], 0.5f);
                        }
                    }
                }

                if (_handles.Count >= 1)
                {
                    // If there is a handle, e.g. during placement of the first handle, return its current position
                    return _handles[0].transform.position;
                }

                if (!_finishedLoading)
                {
                    // If not loaded yet, return zero
                    return Vector3.zero;
                }

                Debug.LogWarningFormat("RamObject[{0}] :: Position :: Unable to determine position based on spline or handles.", name);
                return Vector3.zero;
            }
        }

        #endregion


        #region Lifecycle events
#pragma warning disable IDE0051 // Unused members

        protected override void Start()
        {
            // Get the settings object
            AutomaticElevation = true;
            
            _splinePointElevation = GameTerrain.Current.OffsetAltitude(0.1f);

            var trigger = addHandleAtEndButton.GetComponentInChildren<MouseTrigger>();
            trigger.OnChildClick += HandleAddHandleAtEndButtonClicked;

            trigger = addHandleAtStartButton.GetComponentInChildren<MouseTrigger>();
            trigger.OnChildClick += HandleAddHandleAtStartButtonClicked;

            // Set the highlighter to only highlight the spline, not the handles etc.
            // Do a null check as this sometimes blows up on loading a map
            _highlighter = GetComponent<HighlightEffect>();
            if (_highlighter) _highlighter.effectGroup = TargetOptions.OnlyThisObject;

            // Add this object to the list
            if (!All.Contains(this)) All.Add(this);
        }

        void Update()
        {
            // Update the controls visible in the game world
            ToggleAddHandleButtons(ControlsVisible && !InPlacementMode);
        }

        void LateUpdate()
        {
            if (_isDirty && _finishedLoading)
            {
                // If changed this frame, regenerate the spline and update handles and colliders
                UpdateSpline();
                _isDirty = false;
            }

            _fixRaceConditionWhenAddingHandles = false;
        }

#pragma warning restore IDE0051 // Unused members
        #endregion


        #region Event handlers

        /// <summary>
        /// Called when the AddHandleButton after the last handle is clicked.
        /// </summary>
        private void HandleAddHandleAtEndButtonClicked()
        {
            _fixRaceConditionWhenAddingHandles = true;

            if (!Selected) Select();

            // Create a new handle
            _addAtEnd = true;
            _selectedHandle = CreateHandle(Helpers.GetWorldPointFromMouse());
            _handles.Add(_selectedHandle);
            PickUp();

            // Enter placement state
            StateController.CurrentState.ToPlacement();
        }

        /// <summary>
        /// Called when the AddHandleButton before the first handle is clicked.
        /// </summary>
        private void HandleAddHandleAtStartButtonClicked()
        {
            _fixRaceConditionWhenAddingHandles = true;

            if (!Selected) Select();

            _addAtEnd = false;
            _selectedHandle = CreateHandle(Helpers.GetWorldPointFromMouse());
            _handles.Add(_selectedHandle);
            PickUp();

            // Enter placement state
            StateController.CurrentState.ToPlacement();
        }

        /// <summary>
        /// Called when a placed handle is dragged - move the handle.
        /// </summary>
        /// <param name="handle">The handle that was dragged.</param>
        private void HandleDragged(DraggableObject handle)
        {
            // Do not allow dragging handles while placing a handle
            if (!InPlacementMode)
            {
                _selectedHandle = handle as RamObjectHandle;

                // Move position to mouse, maintaining current elevation
                var newPosition = Helpers.GetWorldPointFromMouse(Helpers.TerrainAndWaterMask);
                newPosition.y = _splinePointElevation;
                MoveTo(newPosition);
            }
        }

        /// <summary>
        /// Called when a handle stops being dragged. Register an undo and update the spline.
        /// </summary>
        /// <param name="handle">The handle that was dragged.</param>
        private void HandleStopDrag(DraggableObject handle)
        {
            if (handle.transform.parent.position != _undoHandlePosition.Value)
            {
                UndoController.RegisterAction(ActionType.MoveHandle, ObjectId, _undoHandlePosition);

                // Update the spline
                _selectedHandle = handle as RamObjectHandle;
                var handleIndex = _handles.IndexOf(_selectedHandle);
                _spline.ChangePointPosition(handleIndex, handle.transform.parent.position);

                _isDirty = true;
            }
        }

        /// <summary>
        /// Called when a handle starts being dragged. Store its starting point for undo.
        /// </summary>
        /// <param name="handle">The handle that's being dragged.</param>
        private void HandleStartDrag(DraggableObject handle)
        {
            if (!InPlacementMode)
            {
                _undoHandlePosition = new KeyValuePair<int, Vector3>(_handles.IndexOf(handle as RamObjectHandle), handle.transform.parent.position);
            }
        }

        #endregion


        #region Public methods

        /// <summary>
        /// Constructor
        /// </summary>
        public RamObject()
        {
            OptionValues = new Dictionary<WorldObjectOption, object>();
        }

        /// <summary>
        /// Initialises the Ram Object on a Content Item.
        /// </summary>
        /// <param name="contentItem">The ContentItem this RamObject is based on.</param>
        /// <param name="itemIndex">The index of the Ram Profile to load within the ContentItem.</param>
        public override void Initialise(ContentItem contentItem, int itemIndex)
        {
            base.Initialise(contentItem, itemIndex);

            bool isRoad = contentItem.Type == WorldObjectType.Road;

            // Basic game object properties
            GameLayer = isRoad ? Helpers.TraversableLayer : Helpers.WaterLayer;
            gameObject.layer = GameLayer;

            OptionValues.Add(WorldObjectOption.Terrain, -1);
            OptionValues.Add(WorldObjectOption.SplineWidth, isRoad ? 2f : 10f);
            if (!isRoad)
            {
                OptionValues.Add(WorldObjectOption.Detailing, -1);
                OptionValues.Add(WorldObjectOption.RiverFlowSpeed, 0f);
            }

            // Initialise at 0,0,0 as some calculations are local and some global - with this root coord they're the same
            transform.position = Vector3.zero;

            // Load profile
            var profileAddress = isRoad ? Content.Current.Combined.RiversRoads.Roads[itemIndex] : Content.Current.Combined.RiversRoads.Rivers[itemIndex];
            SetProfile(profileAddress).ConfigureAwait(false);

            // Get reference to the RamSpline
            _spline = GetComponent<RamSpline>();
            _spline.maskCarve = Helpers.TerrainMask;

            // Create a handle and pick it up
            _selectedHandle = CreateHandle(Helpers.GetWorldPointFromMouse());
            _handles.Add(_selectedHandle);
            PickUp();

            _finishedLoading = true;

            // Initialised - set this as the selected WorldObjectBase
            Select();
        }

        // Initialises the Ram Object on a Map Object
        public async Task Initialise(MapRamObject mapObject)
        {
            FromMapObject(mapObject);
            // Initialise at 0,0,0 as some calculations are local and some global - with this root coord they're the same
            transform.position = Vector3.zero;

            bool isRoad = Type == WorldObjectType.Road;
            GameLayer = isRoad ? Helpers.TraversableLayer : Helpers.WaterLayer;
            gameObject.layer = GameLayer;

            _primaryTerrainAddress = mapObject.primaryTerrainAddress;
            _secondaryTerrainAddress = mapObject.secondaryTerrainAddress;

            _spline = gameObject.GetComponent<RamSpline>();
            _spline.maskCarve = Helpers.TerrainMask;

            ContentItem = new ContentItem()
            {
                Name = "River",
                Type = WorldObjectType.River,
                IDs = Content.Current.Combined.RiversRoads.Rivers
            };

            await SetProfile(PrefabAddress);

            // Add all the points back in
            foreach (var point in mapObject.points)
            {
                _spline.AddPoint(point);
                _handles.Add(CreateHandle(point));
            }

            // Load terrain textures if any were saved
            if ((int)OptionValues[WorldObjectOption.Terrain] > -1) SetPrimaryTexture(Content.Current.Combined.TerrainLayers[(int)OptionValues[WorldObjectOption.Terrain]].ID);
            if (!isRoad && (int)OptionValues[WorldObjectOption.Detailing] > -1) SetSecondaryTexture(Content.Current.Combined.TerrainLayers[(int)OptionValues[WorldObjectOption.Detailing]].ID);

            HideControls();

            _finishedLoading = true;
            UpdateSpline();

            _isDirty = false;
        }

        /// <summary>
        /// Creates a MapRamObject and sets its properties based on the current state of the RamObject.
        /// </summary>
        /// <returns>A MapRamObject representing this RamObject.</returns>
        public override MapObjectBase ToMapObject()
        {
            var mapObject = ToMapObject<MapRamObject>();
            mapObject.primaryTerrainAddress = _primaryTerrainAddress;
            mapObject.secondaryTerrainAddress = _secondaryTerrainAddress;
            mapObject.points.AddRange(_spline.controlPoints);

            return mapObject;
        }

        /// <summary>
        /// Sets an option to the specified value. Updates the RamObject accordingly.
        /// </summary>
        /// <param name="option">The option to set.</param>
        /// <param name="value">The value to set the option to.</param>
        public override void SetOption(WorldObjectOption option, object value)
        {
            base.SetOption(option, value);

            switch (option)
            {
                case WorldObjectOption.RiverFlowSpeed:
                    SetFlowSpeed((float)OptionValues[WorldObjectOption.RiverFlowSpeed]);
                    break;
                case WorldObjectOption.SplineWidth:
                    SetSplineWidth((float)OptionValues[WorldObjectOption.SplineWidth]);
                    break;
                case WorldObjectOption.Terrain:
                    var primaryTexture = (int)value > -1 ? Content.Current.Combined.TerrainLayers[(int)value].ID : null;
                    SetPrimaryTexture(primaryTexture);
                    break;
                case WorldObjectOption.Detailing:
                    var secondaryTexture = (int)value > -1 ? Content.Current.Combined.TerrainLayers[(int)value].ID : null;
                    SetSecondaryTexture(secondaryTexture);
                    break;
            }
        }

        public override void ShowControls()
        {
            base.ShowControls();

            _handles.ForEach(x => x.gameObject.SetActive(true));
        }

        public override void HideControls()
        {
            base.HideControls();

            _handles.ForEach(x => x.gameObject.SetActive(false));
        }

        /// <summary>
        /// Move the current handle to the specified position.
        /// </summary>
        /// <param name="targetPosition">The position to move the handle to.</param>
        public override void MoveTo(Vector3 targetPosition)
        {
            if (_selectedHandle != null)
            {
                _selectedHandle.transform.parent.position = targetPosition;
            }
        }

        /// <summary>
        /// Moves a handle with the specified index to the specified position.
        /// </summary>
        /// <param name="index">The index of the handle to move.</param>
        /// <param name="position">The position to move the handle to.</param>
        public void MoveTo(int index, Vector3 position)
        {
            _handles[index].transform.parent.position = position;
            _spline.ChangePointPosition(index, position);
            _isDirty = true;
        }

        /// <summary>
        /// Picks up the selected handle, making it transparent to raycasts.
        /// </summary>
        public override void PickUp()
        {
            InPlacementMode = true;

            if (_selectedHandle != null)
            {
                SetLayerRecursive(_selectedHandle.gameObject, Helpers.IgnoreRaycastLayer);
            }
        }

        /// <summary>
        /// Releases the current handle.
        /// </summary>
        public override void Place()
        {
            // Do not place if skipping the frame due to a race condition on adding handles
            if (_fixRaceConditionWhenAddingHandles) return;

            // Prevent spline looping back on itself by not placing on top of an existing collider
            var hoveredObject = Helpers.GetObjectAtMouse(new List<WorldObjectType> {WorldObjectType.River, WorldObjectType.Road}, Helpers.SelectableMask);
            if (hoveredObject == gameObject) return;

            UndoController.RegisterAction(ActionType.PlaceHandle, ObjectId, _addAtEnd ? _handles.Count - 1 : 0);

            Release();
            _selectedHandle = CreateHandle(Helpers.GetWorldPointFromMouse());
            _handles.Add(_selectedHandle);
            PickUp();
        }

        /// <summary>
        /// Releases the current handle and creates another.
        /// </summary>
        public override void Release()
        {
            if (_selectedHandle != null)
            {
                // Make the handle not ignore raycasts
                SetLayerRecursive(_selectedHandle.gameObject, Helpers.TraversableLayer);

                // Add the point to the spline
                Vector4 newPoint = _selectedHandle.transform.parent.localPosition;
                newPoint.y = _splinePointElevation;
                newPoint.w = (float)OptionValues[WorldObjectOption.SplineWidth];
                if (_addAtEnd)
                {
                    _spline.AddPoint(newPoint);
                }
                else
                {
                    // Insert at the start (add a new 1, move 0 to 1, change 0 to new point)
                    _spline.AddPointAfter(0);
                    _spline.ChangePointPosition(1, _spline.controlPoints[0]);
                    _spline.ChangePointPosition(0, newPoint);
                }

                _selectedHandle = null;
                _isDirty = true;
            }
        }

        /// <summary>
        /// Removes and destroys the handle currently in placement mode.
        /// </summary>
        public override void CancelPlacement()
        {
            Debug.LogFormat("RamObject[{0}] :: CancelPlacement", name);

            InPlacementMode = false;

            if (_selectedHandle != null)
            {
                _handles.Remove(_selectedHandle);
                DestroyImmediate(_selectedHandle.transform.parent.gameObject);
                _selectedHandle = null;
            }

            Deselect();

            // If cancelling placement before 2 handles were placed, destroy this
            if (_handles.Count < 2)
            {
                Destroy();
            }
        }

        /// <summary>
        /// Destroys the object.
        /// </summary>
        public override void Destroy()
        {
            if (Current == this) Deselect();

            if (Spawned)
            {
                // Remove the spline
                PaintTerrain(true);
                PaintTerrain(true); // paint twice to remove stubborn stains
            }

            // Remove this from the list
            if (All.Contains(this)) All.Remove(this);

            DestroyImmediate(gameObject);

            // Reset the terrain to flat and let all other RAM objects carve again
            GameTerrain.Current.ResetMapWithRamRecarve();
        }

        /// <summary>
        /// Returns the world position of the handle at the specified index.
        /// </summary>
        /// <param name="index">The index of the handle.</param>
        /// <returns>The world position as a Vector3.</returns>
        public Vector3 GetHandlePosition(int index)
        {
            if (index >= 0 && index < _handles.Count) return _handles[index].transform.parent.position;

            return Vector3.zero;
        }

        /// <summary>
        /// Adds a handle into the spline at the given index.
        /// </summary>
        /// <param name="index">The index at which to insert the handle.</param>
        /// <param name="position">The position of the handle.</param>
        /// <remarks>This doesn't work for index 0, which should never happen as that can't be removed without removing the whole spline.</remarks>
        public void AddHandle(int index, Vector3 position)
        {
            var handle = CreateHandle(position);
            _handles.Insert(index, handle);
            _spline.AddPointAfter(index - 1);
            _spline.ChangePointPosition(index, position);

            _isDirty = true;
        }

        /// <summary>
        /// Removes a handle at the given index.
        /// </summary>
        /// <param name="index">The index at which to remove a handle.</param>
        public void RemoveHandle(int index)
        {
            if (_spline.controlPoints.Count <= 2)
            {
                // If removing second to last point, remove the river instead
                Destroy();
            }
            else
            {
                // Remove point
                DestroyImmediate(_handles[index].transform.parent.gameObject);
                _handles.RemoveAt(index);
                _spline.RemovePoint(index);
                _isDirty = true;
            }
        }

        /// <summary>
        /// Not supported.
        /// </summary>
        /// <param name="amount">Not supported.</param>
        public override void Rotate(float amount)
        { }

        /// <summary>
        /// Not supported.
        /// </summary>
        /// <param name="amount">Not supported.</param>
        public override void Scale(float amount)
        { }

        /// <summary>
        /// Not supported.
        /// </summary>
        /// <param name="amount">Not supported.</param>
        public override void Elevate(float amount)
        { }

        /// <summary>
        /// Loads the spline profile at the specified address asynchronously and applies it to the current spline.
        /// </summary>
        /// <param name="profileAddress">The addressables address of the profile to load.</param>
        public async Task SetProfile(string profileAddress)
        {
            PrefabAddress = profileAddress;

            var profiles = await Helpers.LoadAddressables<SplineProfile>(new[] { profileAddress });
            if (profiles.Count > 0)
            {
                SetProfile(profiles[0]);
            }
        }

        /// <summary>
        /// Handle the user hovering their mouse over this object.
        /// </summary>
        /// <param name="hoveredObject">The actual object behing hovered over (can be a sub-object).</param>
        /// <param name="position">The position the user is hovering in world space.</param>
        public override void MouseOver(GameObject hoveredObject, Vector3 position)
        {
            if (hoveredObject.GetComponent<DraggableObject>())
            {
                // Hovering over a handle
                CursorController.Current.MoveObject = true;
            }
            else
            {
                base.MouseOver(hoveredObject, position);
            }
        }

        /// <summary>
        /// Handle the user clicking on any part of this object.
        /// </summary>
        /// <param name="clickedObject">The actual object that was clicked (can be a sub-object).</param>
        /// <param name="position">The position the user clicked in world space.</param>
        public override void Click(GameObject clickedObject, Vector3 position)
        {
            if (clickedObject.GetComponent<DraggableObject>() is RamObjectHandle handle)
            {
                _selectedHandle = handle;
                _selectedHandle.StartDrag();
            }
            else
            {
                base.Click(clickedObject, position);
            }
        }

        /// <summary>
        /// Carves the terrain based on the current RAM profile.
        /// </summary>
        public void CarveTerrain()
        {
            // Do not render until finished loading
            if (!_finishedLoading) return;

            if (_spline.meshfilter == null)
            {
                _spline.GenerateSpline();
            }

            _spline.ShowTerrainCarve();
            _spline.TerrainCarve();
        }

        /// <summary>
        /// Paints the terrain for the riverbed - when remove is passed as true the default terrain texture is painted.
        /// </summary>
        /// <param name="remove">True to paint with the base texture, false to paint with the selected textures.</param>
        public void PaintTerrain(bool remove = false)
        {
            // Do not render until finished loading
            if (!_finishedLoading) return;

            // Get the terrain texture indices for the set textures
            var currentSplatIndex = (!string.IsNullOrEmpty(_primaryTerrainAddress)) ? GameTerrain.Current.GetTextureIndex(_primaryTerrainAddress) : 0;
            var secondSplatIndex = (!string.IsNullOrEmpty(_secondaryTerrainAddress)) ? GameTerrain.Current.GetTextureIndex(_secondaryTerrainAddress) : 0;

            // Don't paint if either texture hasn't been loaded - this can happen in race conditions during map loading
            if (currentSplatIndex == -1 || secondSplatIndex == -1 || _spline == null) return;

            // Set painting textures and noise settings
            _spline.mixTwoSplatMaps = true;
            _spline.currentSplatMap = remove ? 0 : currentSplatIndex;
            _spline.secondSplatMap = remove ? 0 : secondSplatIndex;

            _spline.ShowTerrainCarve();
            _spline.TerrainPaintMeshBased();
        }

        #endregion


        #region Private methods

        /// <summary>
        /// Resets the spline and updates all objects related to it, such as handles, colliders, highlighter, etc.
        /// </summary>
        private void UpdateSpline()
        {
            Debug.Log("RamObject :: UpdateSpline");            

            ResetSpline();

            PositionHandles();
            PositionColliders();
            PositionAddHandleButtons();

            if (OptionValues.ContainsKey(WorldObjectOption.RiverFlowSpeed)) SetFlowSpeed((float)OptionValues[WorldObjectOption.RiverFlowSpeed]);

            // Update the highlighter so it highlights the new spline shape
            if (_highlighter)
            {
                _highlighter.effectGroup = TargetOptions.OnlyThisObject;
                _highlighter.Refresh();
            }
        }

        /// <summary>
        /// Instantiates a new Handle and returns its RamObjectHandle.
        /// </summary>
        /// <param name="handlePosition">The position to instantiate the handle at.</param>
        /// <returns>The new handle as a RamObjectHandle.</returns>
        private RamObjectHandle CreateHandle(Vector3 handlePosition)
        {
            var handleObject = Instantiate(handlePrefab, handlePosition, Quaternion.identity, transform);
            var handle = handleObject.GetComponentInChildren<RamObjectHandle>();
            handle.OnStartDrag += HandleStartDrag;
            handle.OnDrag += HandleDragged;
            handle.OnStopDrag += HandleStopDrag;
            return handleObject.GetComponentInChildren<RamObjectHandle>();
        }

        /// <summary>
        /// Instantiates a new collider and returns its MouseTrigger.
        /// </summary>
        /// <returns>The new collider as a MouseTrigger.</returns>
        private MouseTrigger CreateCollider()
        {
            var colliderObject = Instantiate(colliderPrefab, transform);
            return colliderObject.GetComponentInChildren<MouseTrigger>();
        }

        /// <summary>
        /// Show or hide the Add Handle buttons.
        /// </summary>
        /// <param name="show">True to show the handles, false to hide them.</param>
        private void ToggleAddHandleButtons(bool show)
        {
            addHandleAtStartButton.gameObject.SetActive(show);
            addHandleAtEndButton.gameObject.SetActive(show);
        }

        /// <summary>
        /// Position the Add Handle buttons relative to the spline points.
        /// </summary>
        private void PositionAddHandleButtons()
        {
            if (_spline.controlPoints.Count == 1)
            {
                // If one point, show a handle to the right of it
                addHandleAtEndButton.transform.localPosition = (Vector3)_spline.controlPoints[0] + Vector3.right * 2;
            }
            else if (_spline.controlPoints.Count >= 2)
            {
                // If there are two or more, show a handle besides each, in the opposite direction of the next handle
                var direction = (_spline.controlPoints[0] - _spline.controlPoints[1]).normalized;
                addHandleAtStartButton.transform.localPosition = _spline.controlPoints[0] + direction * 2;
                
                direction = (_spline.controlPoints[_spline.controlPoints.Count - 1] - _spline.controlPoints[_spline.controlPoints.Count - 2]).normalized;
                addHandleAtEndButton.transform.localPosition = _spline.controlPoints[_spline.controlPoints.Count - 1] + direction * 2;
            }
        }

        /// <summary>
        /// Positions the handles over the spline control points.
        /// </summary>
        private void PositionHandles()
        {
            for (int i = 0; i < _spline.controlPoints.Count; i++)
            {
                _handles[i].transform.parent.localPosition = _spline.controlPoints[i];
            }
        }

        /// <summary>
        /// Positions the colliders between each set of two control points.
        /// </summary>
        private void PositionColliders()
        {
            if (_spline.controlPoints.Count >= 2)
            {
                for (int i = 1; i < _spline.controlPoints.Count; i++)
                {
                    // Add a new collider if we've run out
                    if (i > _colliders.Count)
                    {
                        _colliders.Add(CreateCollider());
                    }

                    // Set collider position, rotation and size to span between the last control point and the current
                    var currentCollider = _colliders[i - 1];
                    var fromPosition = _spline.controlPoints[i - 1];
                    var toPosition = _spline.controlPoints[i];
                    // Drop the positions slightly to avoid getting in the way of mouse interaction with the handles
                    fromPosition.y -= .5f;
                    toPosition.y -= 0.5f;

                    currentCollider.transform.localPosition = Vector3.Lerp(fromPosition, toPosition, 0.5f);
                    currentCollider.transform.LookAt(fromPosition);
                    currentCollider.transform.localScale = new Vector3((float)OptionValues[WorldObjectOption.SplineWidth], 0.01f, Vector3.Distance(fromPosition, toPosition));
                }
            }

            if (_colliders.Count > _spline.controlPoints.Count - 1)
            {
                for (int i = _spline.controlPoints.Count; i < _colliders.Count; i++)
                {
                    DestroyImmediate(_colliders[i].transform.parent.gameObject);
                    _colliders.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Re-carve and paint the terrain.
        /// </summary>
        private void ResetSpline()
        {
            // Do not render until finished loading
            if (!_finishedLoading) return;

            if (_spline.controlPoints.Count >= 2)
            {
                // If not previously generated, the meshfilter is null which blows up, so check it
                if (_spline.meshfilter != null)
                {
                    // Remove the existing spline
                    PaintTerrain(true);
                }

                // Generate the spline with new points
                _spline.GenerateSpline();

                // Reset the width and add noise
                SetSplineWidth((float)OptionValues[WorldObjectOption.SplineWidth]);
                _spline.AddNoiseToWidths();

                // Carve and paint the new spline
                PaintTerrain();
                GameTerrain.Current.ResetMapWithRamRecarve();
                
                Spawned = true;
            }
        }

        /// <summary>
        /// Loads the profile at the specified address and loads its values into the current spline.
        /// </summary>
        /// <param name="profile">The profile to load.</param>
        /// <remarks>This code is copied from RamSwitch built into RAM2019 with minor alterations.</remarks>
        private void SetProfile(SplineProfile profile)
        {
            _spline.currentProfile = profile;

            _spline.meshCurve = new AnimationCurve(_spline.currentProfile.meshCurve.keys);
            _spline.flowFlat = new AnimationCurve(_spline.currentProfile.flowFlat.keys);
            _spline.flowWaterfall = new AnimationCurve(_spline.currentProfile.flowWaterfall.keys);
            _spline.terrainCarve = new AnimationCurve(_spline.currentProfile.terrainCarve.keys);
            _spline.terrainPaintCarve = new AnimationCurve(_spline.currentProfile.terrainPaintCarve.keys);

            for (int i = 0; i < _spline.controlPointsMeshCurves.Count; i++)
            {
                _spline.controlPointsMeshCurves[i] = new AnimationCurve(_spline.meshCurve.keys);
            }
            MeshRenderer ren = _spline.GetComponent<MeshRenderer>();
            ren.sharedMaterial = _spline.currentProfile.splineMaterial;

            _spline.minVal = _spline.currentProfile.minVal;
            _spline.maxVal = _spline.currentProfile.maxVal;

            _spline.traingleDensity = _spline.currentProfile.traingleDensity;
            _spline.vertsInShape = _spline.currentProfile.vertsInShape;

            _spline.uvScale = _spline.currentProfile.uvScale;

            _spline.uvRotation = _spline.currentProfile.uvRotation;

            _spline.noiseflowMap = _spline.currentProfile.noiseflowMap;
            _spline.noiseMultiplierflowMap = _spline.currentProfile.noiseMultiplierflowMap;
            _spline.noiseSizeXflowMap = _spline.currentProfile.noiseSizeXflowMap;
            _spline.noiseSizeZflowMap = _spline.currentProfile.noiseSizeZflowMap;

            _spline.floatSpeed = _spline.currentProfile.floatSpeed;

            _spline.distSmooth = _spline.currentProfile.distSmooth;
            _spline.distSmoothStart = _spline.currentProfile.distSmoothStart;

            _spline.noiseCarve = _spline.currentProfile.noiseCarve;
            _spline.noiseMultiplierInside = _spline.currentProfile.noiseMultiplierInside;
            _spline.noiseMultiplierOutside = _spline.currentProfile.noiseMultiplierOutside;
            _spline.noiseSizeX = _spline.currentProfile.noiseSizeX;
            _spline.noiseSizeZ = _spline.currentProfile.noiseSizeZ;
            _spline.terrainSmoothMultiplier = _spline.currentProfile.terrainSmoothMultiplier;
            _spline.currentSplatMap = _spline.currentProfile.currentSplatMap;
            _spline.mixTwoSplatMaps = _spline.currentProfile.mixTwoSplatMaps;
            _spline.secondSplatMap = _spline.currentProfile.secondSplatMap;

            _spline.distanceClearFoliage = _spline.currentProfile.distanceClearFoliage;
            _spline.distanceClearFoliageTrees = _spline.currentProfile.distanceClearFoliageTrees;
            _spline.noisePaint = _spline.currentProfile.noisePaint;
            _spline.noiseMultiplierInsidePaint = _spline.currentProfile.noiseMultiplierInsidePaint;
            _spline.noiseMultiplierOutsidePaint = _spline.currentProfile.noiseMultiplierOutsidePaint;
            _spline.noiseSizeXPaint = _spline.currentProfile.noiseSizeXPaint;
            _spline.noiseSizeZPaint = _spline.currentProfile.noiseSizeZPaint;

            _spline.simulatedRiverLength = _spline.currentProfile.simulatedRiverLength;
            _spline.simulatedRiverPoints = _spline.currentProfile.simulatedRiverPoints;
            _spline.simulatedMinStepSize = _spline.currentProfile.simulatedMinStepSize;
            _spline.simulatedNoUp = _spline.currentProfile.simulatedNoUp;
            _spline.simulatedBreakOnUp = _spline.currentProfile.simulatedBreakOnUp;
            _spline.noiseWidth = _spline.currentProfile.noiseWidth;
            _spline.noiseMultiplierWidth = _spline.currentProfile.noiseMultiplierWidth;
            _spline.noiseSizeWidth = _spline.currentProfile.noiseSizeWidth;

            _spline.receiveShadows = _spline.currentProfile.receiveShadows;
            _spline.shadowCastingMode = _spline.currentProfile.shadowCastingMode;

            _spline.oldProfile = _spline.currentProfile;

            // Set as dirty so the spline is re-generated at the end of this frame
            _isDirty = true;
        }

        /// <summary>
        /// Updates the texture and repaints the spline.
        /// </summary>
        /// <param name="address">Address of the new texture.</param>
        private void SetPrimaryTexture(string address)
        {
            if (string.IsNullOrEmpty(address))
            {
                // If it's empty, just paint with blank
                _primaryTerrainAddress = null;
                PaintTerrain();
                return;
            }

            var index = GameTerrain.Current.GetTextureIndex(address);
            if (index > -1)
            {
                _primaryTerrainAddress = address;
                PaintTerrain();
            }
            else
            {
                // Texture not loaded - load it and call back into this method
                GameTerrain.Current.LoadTextureWithCallback(address, SetPrimaryTexture);
            }
        }

        /// <summary>
        /// Updates the texture and repaints the spline.
        /// </summary>
        /// <param name="address">Address of the texture.</param>
        private void SetSecondaryTexture(string address)
        {
            if (string.IsNullOrEmpty(address))
            {
                // If it's empty, just paint with blank
                _secondaryTerrainAddress = null;
                PaintTerrain();
                return;
            }

            var index = GameTerrain.Current.GetTextureIndex(address);
            if (index > -1)
            {
                _secondaryTerrainAddress = address;
                PaintTerrain();
            }
            else
            {
                // Texture not loaded - load it and call back into this method
                GameTerrain.Current.LoadTextureWithCallback(address, SetSecondaryTexture);
            }
        }

        /// <summary>
        /// Sets the given width on all points.
        /// </summary>
        /// <param name="width">The width to set.</param>
        private void SetSplineWidth(float width)
        {
            if (_handles.Count >= 2)
            {
                // Remove the existing spline
                PaintTerrain(true);
            }

            for (int i = 0; i < _spline.controlPoints.Count; i++)
            {
                var point = _spline.controlPoints[i];
                point.y = _splinePointElevation;
                point.w = width;
                _spline.ChangePointPosition(i, point);
            }

            _isDirty = true;
        }

        /// <summary>
        /// Sets the flow speed of the river.
        /// </summary>
        /// <param name="flowSpeed">A speed. This is incremented and then divided by 2 to get the final value used for the shader.</param>
        private void SetFlowSpeed(float flowSpeed)
        {
            // Set flow speed on shader
            var splineRenderer = _spline.GetComponent<Renderer>();
            var slowWaterSpeed = (flowSpeed + 1) / 2;
            splineRenderer.material.SetVector(SlowWaterSpeed, new Vector4(slowWaterSpeed, slowWaterSpeed, 0, 0));
        }

        #endregion
    }
}