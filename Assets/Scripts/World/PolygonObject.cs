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
using HighlightPlus;
using TT.Data;
using TT.MapEditor;
using TT.Shared;
using TT.Shared.UserContent;
using TT.UI;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

/*
 * This class is used to draw polygons. It is based on WorldObjectBase but has a few quirks.
 * Each time a point is added or moved, the mesh is updated to cover the new area. This mesh
 * has a mesh collider to capture clicks. Unfortunately the mesh collider covers a bounding
 * box around the mesh, not just the mesh itself. Therefore, a method IsPointInPolygon 
 * checks whether a point is actually within the polygon itself, using math instead of
 * meshes or colliders. The mesh is still used as an initial pass to avoid crunching math
 * on every frame to determine hover over. Instead, the math should only be run when the
 * mesh collider has triggered, indicating the mouse is over or very near the polygon.
 *
 * Important: Whenever a click or hover event happens on any PolygonObject, check
 * IsPointInPolygon first and only trigger the mouse event if it returns true.
 *
 */

namespace TT.World
{
    public class PolygonObject : WorldObjectBase
    {

        #region Editor fields
#pragma warning disable IDE0044 // Make fields read-only

        [SerializeField][Tooltip("A prefab to spawn for the handles shown over points.")] private GameObject handlePrefab;
        [FormerlySerializedAs("AddHandleButton")] [SerializeField] private GameObject addHandleButton;

#pragma warning restore IDE0044
        #endregion


        #region Private fields

        private HighlightEffect _highlighter;
        private readonly List<GameObject> _generatedGameObjects = new List<GameObject>();
        private readonly List<string> _generatedObjectAddresses = new List<string>();
        private readonly List<RamObjectHandle> _handles = new List<RamObjectHandle>();
        private RamObjectHandle _selectedHandle;
        private readonly List<Vector3> _vertices = new List<Vector3>();
        private MeshCollider _meshCollider;
        private MeshRenderer _meshRenderer;
        private MeshFilter _meshFilter;
        private readonly Material[] _highlightMaterials = new Material[1];
        private readonly Material[] _noMaterials = Array.Empty<Material>();
        private float _polygonYPos;
        private Mesh _mesh;
        // When adding handles at the start or end, the click on the button is also picked up by the 
        // state, causing it to tell the WorldObjectBase to place the handle. This var is set to true
        // for one frame when adding a handle to skip over that click.
        private bool _fixRaceConditionWhenAddingHandles;
        private int _targetItems;
        private int _placedItems;
        private bool _initialised;
        private KeyValuePair<int, Vector3> _undoHandlePosition;

        #endregion


        #region Public properties

        /// <summary>
        /// This object is always in continuous placement mode to allow for more points to be added.
        /// </summary>
        public override bool ContinuousPlacementMode => true;

        /// <summary>
        /// Gets the area of the polygon in square game units.
        /// </summary>
        public float Area => PolygonHelpers.PolygonArea(_vertices);

        /// <summary>
        /// Gets the bounding box surrounding the polygon.
        /// </summary>
        public Bounds BoundingBox => _meshCollider.bounds;

        public override Vector3 Position => _handles.Count >= 1 ? _handles[0].transform.position : addHandleButton.transform.position;

        #endregion


        #region Lifecycle events
#pragma warning disable IDE0051 // Unused members

        protected override void Start()
        {
            AutomaticElevation = true;
            
            _meshCollider = GetComponent<MeshCollider>();
            _meshRenderer = GetComponent<MeshRenderer>();
            _meshFilter = GetComponent<MeshFilter>();
            _highlighter = GetComponent<HighlightEffect>();
            if (_highlighter) _highlighter.effectGroup = TargetOptions.OnlyThisObject;

            _highlightMaterials[0] = Helpers.Settings.editorSettings.polygonMaterial;

            _mesh = new Mesh();
            _mesh.MarkDynamic();

            addHandleButton.GetComponentInChildren<MouseTrigger>().OnChildClick += HandleAddHandleButtonClicked;

            // Add this object to the list
            if (!All.Contains(this)) All.Add(this);
        }

        void Update()
        {
            // Show mesh if controls are visible and either Show Polygons has been selected by the user, or this is in placement mode
            if (_meshRenderer) _meshRenderer.materials = ControlsVisible && (Helpers.Settings.editorSettings.showPolygons || InPlacementMode) ? _highlightMaterials : _noMaterials;

            // Show Add Handle button only when selected but not in placement mode
            addHandleButton.SetActive(ControlsVisible && !InPlacementMode);

            // While the number of placed items is less than the number of requested items, keep the wait cursor
            if (_placedItems < _targetItems)
            {
                CursorController.Current.Wait = true;
            }
            else if (!_initialised)
            {
                _initialised = true;
            }
        }

        void LateUpdate()
        {
            _fixRaceConditionWhenAddingHandles = false;
        }

#pragma warning restore IDE0051 // Unused members
        #endregion


        #region Event handlers

        /// <summary>
        /// Called when a placed handle is dragged - move the handle and associated spline point.
        /// </summary>
        /// <param name="handle">The handle that was dragged.</param>
        private void HandleDragged(DraggableObject handle)
        {
            // Do not allow dragging handles while placing a handle
            if (!InPlacementMode)
            {
                _selectedHandle = handle as RamObjectHandle;

                // Move position to mouse
                var newPosition = Helpers.GetWorldPointFromMouse();
                MoveTo(newPosition);

                var handleIndex = _handles.IndexOf(_selectedHandle);
                _vertices[handleIndex] = newPosition;

                UpdateMesh();
                PositionAddHandleButton();
            }
        }

        private void HandleStopDrag(DraggableObject handle)
        {
            if (handle.transform.parent.position != _undoHandlePosition.Value)
            {
                UndoController.RegisterAction(ActionType.MoveHandle, ObjectId, _undoHandlePosition);
            }
        }

        private void HandleStartDrag(DraggableObject handle)
        {
            if (!InPlacementMode)
            {
                _undoHandlePosition = new KeyValuePair<int, Vector3>(_handles.IndexOf(handle as RamObjectHandle), handle.transform.parent.position);
            }
        }

        /// <summary>
        /// Called when the Add Handle button is clicked - create a new handle and go into placement mode.
        /// </summary>
        private void HandleAddHandleButtonClicked()
        {
            _fixRaceConditionWhenAddingHandles = true;

            // Create a new handle, add the point and re-position the add handle button
            _selectedHandle = CreateHandle(addHandleButton.transform.position);
            _handles.Add(_selectedHandle);

            UndoController.RegisterAction(ActionType.PlaceHandle, ObjectId, _handles.IndexOf(_selectedHandle));

            AddPointToMesh(_selectedHandle.transform.position);
            PositionAddHandleButton();

            _selectedHandle = null;

            if (!Selected) Select();
        }

        #endregion

        #region Public methods

        public PolygonObject()
        {
            OptionValues = new Dictionary<WorldObjectOption, object>();
        }

        /// <summary>
        /// Checks whether a point is within this polygon and returns the result.
        /// </summary>
        /// <param name="point">The world point to check.</param>
        /// <returns>True if the point is within this polygon, false if it isn't.</returns>
        /// <remarks>This only checks the X and Z axes of the point as the polygon is treated as a flat area. Differences in the Y axis are ignored.</remarks>
        public bool IsPointInPolygon(Vector3 point)
        {
            List<Vector2> vertices2 = new List<Vector2>();
            foreach (Vector3 vertex in _vertices)
            {
                vertices2.Add(new Vector2(vertex.x, vertex.z));
            }

            Vector2 mouseVector2 = new Vector2(point.x, point.z);

            return PolygonHelpers.IsPointInside(vertices2.ToArray(), vertices2.Count, mouseVector2);
        }

        /// <summary>
        /// Creates a new polygon.
        /// </summary>
        /// <param name="contentItem">Ignored.</param>
        /// <param name="itemIndex">Ignored.</param>
        public override void Initialise(ContentItem contentItem, int itemIndex)
        {
            base.Initialise(contentItem, itemIndex);

            // Initialise at 0,0,0 as some calculations are local and some global - with this root coord they're the same
            transform.position = Vector3.zero;

            // Create a handle and pick it up
            _selectedHandle = CreateHandle(Helpers.GetWorldPointFromMouse());
            _handles.Add(_selectedHandle);
            PickUp();

            // Initialised - set this as the selected WorldObjectBase
            Select();

            ShowControls();
        }

        public void Initialise(ScatterAreaData scatterAreaData)
        {
            Start();
            FromMapObject(scatterAreaData);

            ContentItem = new ContentItem()
            {
                Type = WorldObjectType.ScatterArea
            };

            foreach (var point in scatterAreaData.points)
            {
                Vector3 vector = point.ToVector4();
                _handles.Add(CreateHandle(vector));
                AddPointToMesh(vector);
            }

            _targetItems = scatterAreaData.scatterInstances.Count;

            foreach (var scatterInstance in scatterAreaData.scatterInstances)
            {
                var contentItem = Content.GetContentItemById(WorldObjectType.NatureObject, scatterInstance.prefabAddress);
                if (contentItem != null)
                {
                    var itemIndex = 0;
                    for (int i = 0; i < contentItem.IDs.Length; i++)
                    {
                        if (contentItem.IDs[i].Equals(scatterInstance.prefabAddress))
                        {
                            itemIndex = i;
                            break;
                        }
                    }
                    PlacePrefab(contentItem, itemIndex, scatterInstance.position.ToVector4(), scatterInstance.rotation, transform);
                }
            }

            UpdateMesh();
            PositionAddHandleButton();
            HideControls();
        }

        /// <summary>
        /// Shows the handles and in some circumstances the polygon itself.
        /// </summary>
        public override void ShowControls()
        {
            base.ShowControls();

            // Show handles whenever selected
            _handles.ForEach(x => x.gameObject.SetActive(true));

            var controlsToggles = GetComponentsInChildren<WorldObjectControlsToggle>();
            foreach (var controlsToggle in controlsToggles)
            {
                controlsToggle.Show();
            }
        }

        /// <summary>
        /// Hides the handles and the polygon itself.
        /// </summary>
        public override void HideControls()
        {
            base.HideControls();

            // Show handles whenever selected
            _handles.ForEach(x => x.gameObject.SetActive(false));

            var controlsToggles = GetComponentsInChildren<WorldObjectControlsToggle>();
            foreach (var controlsToggle in controlsToggles)
            {
                controlsToggle.Hide();
            }
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

            UndoController.RegisterAction(ActionType.PlaceHandle, ObjectId, _handles.IndexOf(_selectedHandle));

            Release();
            _selectedHandle = CreateHandle(Helpers.GetWorldPointFromMouse());
            _handles.Add(_selectedHandle);
            PositionAddHandleButton();
            PickUp();
        }

        /// <summary>
        /// Releases the current handle and creates another.
        /// </summary>
        public override void Release()
        {
            InPlacementMode = false;

            if (_selectedHandle != null)
            {
                // Make the handle not ignore raycasts
                SetLayerRecursive(_selectedHandle.gameObject, Helpers.TraversableLayer);

                AddPointToMesh(_selectedHandle.transform.position);

                _selectedHandle = null;
            }
        }

        /// <summary>
        /// Move the current handle to the specified position.
        /// </summary>
        /// <param name="targetPosition">The position to move the handle to.</param>
        public override void MoveTo(Vector3 targetPosition)
        {
            if (_selectedHandle != null)
            {
                targetPosition.y = GameTerrain.Current.OffsetAltitude(0);
                _selectedHandle.transform.parent.position = targetPosition;
            }
        }

        /// <summary>
        /// Moves the handle with the given index to the position.
        /// </summary>
        /// <param name="index">The index of the handle to move.</param>
        /// <param name="position">The position to move the handle to.</param>
        public void MoveTo(int index, Vector3 position)
        {
            _handles[index].transform.parent.position = position;
            _vertices[index] = position;

            UpdateMesh();
            PositionAddHandleButton();
        }

        /// <summary>
        /// Removes and destroys the handle currently in placement mode.
        /// </summary>
        public override void CancelPlacement()
        {
            Debug.LogFormat("PolygonObject[{0}] :: CancelPlacement", name);

            InPlacementMode = false;

            if (_selectedHandle != null)
            {
                _handles.Remove(_selectedHandle);
                DestroyImmediate(_selectedHandle.transform.parent.gameObject);
                _selectedHandle = null;
            }

            if (_handles.Count < 3)
            {
                // Can't exist with fewer than 3 points
                Destroy();
            }
        }

        /// <summary>
        /// Returns the world position of the handle at the specified index.
        /// </summary>
        /// <param name="index">The index of the handle.</param>
        /// <returns>The world position as a Vector3.</returns>
        public Vector3 GetHandlePosition(int index)
        {
            if (index >= 0 && index < _handles.Count) return _handles[index].transform.position;

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
            _vertices.Insert(index, position);

            UpdateMesh();
        }

        /// <summary>
        /// Removes a handle at the given point
        /// </summary>
        /// <param name="value"></param>
        public void RemoveHandle(int value)
        {
            _vertices.RemoveAt(value);
            DestroyImmediate(_handles[value].transform.parent.gameObject);
            _handles.RemoveAt(value);

            if (_handles.Count >= 3)
            {
                UpdateMesh();
            }
            else
            {
                // Can't exist with fewer than 3 points
                Destroy();
            }
        }

        /// <summary>
        /// Destroys and releases all generated items.
        /// </summary>
        public void DestroyChildren()
        {
            _generatedGameObjects.ForEach(x => { Addressables.Release(x); Destroy(x); });
            _generatedGameObjects.Clear();
            _generatedObjectAddresses.Clear();
        }

        public void PlaceRandom(List<ContentItemCategory> categories, float density)
        {
            List<Vector3> positions = GeneratePositions(density);
            
            // Store the number of items being created so we know when to clear the wait cursor
            _targetItems += positions.Count;

            PlacePrefabs(positions, categories, transform);
        }

        public override void MouseOver(GameObject hoveredObject, Vector3 position)
        {
            if (hoveredObject.GetComponent<DraggableObject>())
            {
                // Hovering over a handle
                CursorController.Current.MoveObject = true;
            }
            else if (IsPointInPolygon(position))
            {
                // Hovering over one of the colliders
                base.MouseOver(hoveredObject, position);
            }
        }

        public override void Click(GameObject clickedObject, Vector3 position)
        {
            if (clickedObject.GetComponent<DraggableObject>() is { } clickedDraggable)
            {
                // Hovering over a handle
                CursorController.Current.MoveObject = true;
                clickedDraggable.StartDrag();

                if (!Selected) Select();
            }
            else if (IsPointInPolygon(position))
            {
                base.Click(clickedObject, position);
            }
        }

        public override BaseObjectData ToDataObject()
        {
            var mapObject = ToMapObject<ScatterAreaData>();
            mapObject.points.AddRange(Vector4Data.FromVector3List(_vertices));

            if (_generatedGameObjects.Count != _generatedObjectAddresses.Count)
            {
                Debug.LogErrorFormat("PolygonObject[{0}] :: ToDataObject :: Item count mismatch. GameObjects ({1}) vs addresses ({2}).", name, _generatedGameObjects.Count, _generatedObjectAddresses.Count);
            }

            for (int i = 0; i < _generatedGameObjects.Count; i++)
            {
                mapObject.scatterInstances.Add(
                    new ScatterObjectData()
                    {
                        prefabAddress = _generatedObjectAddresses[i],
                        position = new Vector4Data(_generatedGameObjects[i].transform.position),
                        rotation = _generatedGameObjects[i].transform.rotation
                    }
                );
            }

            return mapObject;
        }

        #endregion


        #region Private methods

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
            return handle;
        }

        /// <summary>
        /// Adds a point to the mesh and updates it.
        /// </summary>
        /// <param name="point">The point to add to the mesh.</param>
        /// <remarks>Mesh generation code inspired by https://medium.com/@hyperparticle/draw-2d-physics-shapes-in-unity3d-2e0ec634381c</remarks>
        private void AddPointToMesh(Vector3 point)
        {
            if (_polygonYPos == 0) _polygonYPos = GameTerrain.Current.OffsetAltitude(0.1f);
            _vertices.Add(new Vector3(point.x, _polygonYPos, point.z));
            UpdateMesh();
        }

        /// <summary>
        /// Updates the mesh with the current points and refreshes the components reliant on the mesh.
        /// </summary>
        private void UpdateMesh()
        {
            List<Vector2> vertices2 = new List<Vector2>();
            foreach (Vector3 vertex in _vertices)
            {
                vertices2.Add(new Vector2(vertex.x, vertex.z));
            }

            var indices = PolygonHelpers.Triangulate(vertices2);

            // Update the mesh, reset triangles to null before setting vertices or it errors on removing vertices
            _mesh.triangles = null;
            _mesh.vertices = _vertices.ToArray();
            _mesh.triangles = indices;
            
            _mesh.RecalculateNormals();
            _mesh.RecalculateBounds();

            // Prevent errors on loading when the following objects may not have loaded completely yet
            if (!_meshCollider) _meshCollider = GetComponent<MeshCollider>();
            if (_meshCollider) _meshCollider.sharedMesh = _mesh;
            
            if (!_meshFilter) _meshFilter = GetComponent<MeshFilter>();
            if (_meshFilter) _meshFilter.mesh = _mesh;

            if (!_highlighter) _highlighter = GetComponent<HighlightEffect>();
            if (_highlighter) _highlighter.Refresh();
        }

        /// <summary>
        /// Positions the Add Handle button so that it sits near the first button, and doesn't overlap with the polygon.
        /// </summary>
        private void PositionAddHandleButton()
        {
            if (_vertices.Count >= 2)
            {
                var midPoint = Vector3.Lerp(_vertices[0], _vertices[_vertices.Count - 1], 0.5f);
                addHandleButton.transform.position = midPoint;
            }
            else if (_handles.Count >= 1)
            {
                addHandleButton.transform.position = _handles[0].transform.position + Vector3.right;
            }
        }

        private List<Vector3> GeneratePositions(float density)
        {
            var positions = new List<Vector3>();

            int maxTries = (int)(Area * density * 10);
            int targetItemCount = (int)Mathf.Clamp(Area * density, 1, Mathf.Infinity);
            var objectYPos = GameTerrain.Current.OffsetAltitude(0.01f);

            for (int i = 0; i < maxTries; i++)
            {
                // Get a random position within the bounding box of the polygon
                float posX = Random.Range(BoundingBox.min.x, BoundingBox.max.x);
                float posZ = Random.Range(BoundingBox.min.z, BoundingBox.max.z);
                Vector3 position = new Vector3(posX, objectYPos, posZ);

                if (IsPointInPolygon(position)) positions.Add(position);

                // On reaching the target number, break out
                if (positions.Count >= targetItemCount) break;
            }

            // Log if unable to generate the required number
            if (positions.Count < targetItemCount) Debug.LogWarningFormat("PolygonObject :: GeneratePositions :: Unable to generate enough positions. Area {0}, target {1}, actual {2} after {3} tries.", Area, targetItemCount, positions.Count, maxTries);

            return positions;
        }

        private void PlacePrefabs(List<Vector3> positions, List<ContentItemCategory> categories, Transform parent)
        {
            // If no positions found, return
            if (positions.Count == 0) return;

            // Instantiate for all the positions
            for (int i = 0; i < positions.Count; i++)
            {
                ContentItemCategory categoryToPlace = categories[Random.Range(0, categories.Count)];
                ContentItem contentItemToPlace = categoryToPlace.Items[Random.Range(0, categoryToPlace.Items.Length)];
                var itemNumber = contentItemToPlace.IDs.Length > 1 ? Random.Range(0, contentItemToPlace.IDs.Length) : 0;

                var rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
                PlacePrefab(contentItemToPlace, itemNumber, positions[i], rotation, parent);
            }
        }


        private async void PlacePrefab(ContentItem contentItem, int itemNumber, Vector3 position, Quaternion rotation, Transform parent)
        {
            var address = contentItem.IDs[itemNumber];
            var placedObject = await Addressables.InstantiateAsync(address, position, rotation, parent).Task;

            // Store the object so we can destroy it later
            _generatedGameObjects.Add(placedObject);
            _generatedObjectAddresses.Add(address);

            // Set the object's scale based on its content item
            Helpers.SetLayerRecursive(placedObject, contentItem.Traversable ? Helpers.TraversableLayer : Helpers.ImpassableLayer);
            var placedObjectScale = contentItem.Scale;
            if (Math.Abs(placedObjectScale - 1f) > 0)
            {
                placedObject.transform.localScale = new Vector3(placedObjectScale, placedObjectScale, placedObjectScale);
            }

            // Increment the number of items done so we know when to clear the wait cursor
            _placedItems++;
        }

        #endregion



    }
}