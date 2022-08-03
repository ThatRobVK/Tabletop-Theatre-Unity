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
using TT.Data;
using TT.MapEditor;
using TT.Shared;
using TT.Shared.GameContent;
using TT.Shared.UserContent;
using TT.Shared.World;
using TT.State;
using TT.UI;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace TT.World
{
    /// <summary>
    /// Base class for all objects in the world.
    /// </summary>
    public abstract class WorldObjectBase : MonoBehaviour
    {
        #region Static members

        /// <summary>
        /// The currently selected object.
        /// </summary>
        public static WorldObjectBase Current { get; private set; }


        /// <summary>
        /// A list of all instances of WorldObjectBase on the current map.
        /// </summary>
        public static readonly List<WorldObjectBase> All = new List<WorldObjectBase>();

        #endregion


        #region Public properties

        /// <summary>
        /// A unique identifier for this object on this map. This is consistent between saves and loads.
        /// </summary>
        public Guid ObjectId { get; protected set; }
        /// <summary>
        /// The prefab from which this object was created.
        /// </summary>
        public string PrefabAddress { get; protected set; }
        /// <summary>
        /// The type of object this is.
        /// </summary>
        public WorldObjectType Type { get; private set; }
        /// <summary>
        /// The position of this object in world space.
        /// </summary>
        public virtual Vector3 Position => gameObject.transform.position;
        /// <summary>
        /// The local euler angles this object has been rotated to.
        /// </summary>
        [NonSerialized] public Vector3 LocalRotation = Vector3.zero;
        /// <summary>
        /// If true the game should spawn another item when this object is placed in placement mode.
        /// </summary>
        public virtual bool ContinuousPlacementMode => false;
        /// <summary>
        /// Object-specific options as defined by the ContentItem, and their values.
        /// </summary>
        public Dictionary<WorldObjectOption, object> OptionValues { get; protected set; }
        /// <summary>
        /// Whether the user has starred this object.
        /// </summary>
        public bool Starred { get; set; }
        /// <summary>
        /// Whether elevation should be determined on movement by what is underneath (true) or maintained at the same level (false).
        /// </summary>
        public bool AutomaticElevation 
        { 
            get
            {
                return _automaticElevation;
            } 
            set
            {
                _automaticElevation = value;

                if (_automaticElevation)
                {
                    // Check if in the game layer, otherwise don't touch the layer
                    bool setLayer = gameObject.layer == GameLayer;

                    // Make this object see-through for the raycast
                    if (setLayer) Helpers.SetLayerRecursive(gameObject, Helpers.IgnoreRaycastLayer);
                    
                    // Find the highest stackable object at this position (including terrain)
                    var rayOrigin = transform.position;
                    rayOrigin.y = GameTerrain.Current.OffsetAltitude(20f);
                    Ray newRay = new Ray(rayOrigin, Vector3.down);
                    if (Physics.Raycast(newRay, out RaycastHit hit, Mathf.Infinity, Helpers.StackableMask))
                    {
                        // Place this on top of the object
                        ElevateTo(hit.point.y);
                    }

                    // Return the object to its normal solid state
                    if (setLayer) Helpers.SetLayerRecursive(gameObject, GameLayer);
                }
            } 
        }


        /// <summary>
        /// Gets the ContentItem representing this world object.
        /// </summary>
        public ContentItem ContentItem
        {
            get
            {
                if (_contentItem == null)
                {
                    _contentItem = Content.GetContentItemById(Type, PrefabAddress);
                }
                return _contentItem;
            }
            protected set
            {
                _contentItem = value;
            }
        }

        /// <summary>
        /// Returns whether this object is draggable.
        /// </summary>
        public bool IsDraggable => Draggable != null;

        /// <summary>
        /// True when the object is being placed. False once it has been placed.
        /// </summary>
        public bool InPlacementMode { get; protected set; }

        #endregion


        #region Private fields

        private ContentItem _contentItem;
        private bool _automaticElevation = true;
        protected bool Selected => Current == this;
        protected bool ControlsVisible { get; set; }
        protected DraggableObject Draggable;
        protected bool Spawned;
        protected float ScaleMultiplier = 1f;
        protected int GameLayer;
        protected readonly bool Interactable = true;

        #endregion


        #region Event Handlers

        protected virtual void Start()
        {
            if (string.IsNullOrEmpty(PrefabAddress))
            {
                // Warn if PrefabAddress isn't set, this causes errors elsewhere
                Debug.LogWarningFormat("No PrefabAddress set on {0}", name);
            }

            // Get all colliders in the item's tree
            AddColliderRecursive(gameObject);

            Draggable = GetComponent<DraggableObject>();

            // Add this object to the list
            if (!All.Contains(this)) All.Add(this);

            // Spawned and ready to take calls
            Spawned = true;
        }

        #endregion


        #region Public methods

        /// <summary>
        /// Initialises the World Object from a Content Item.
        /// </summary>
        /// <param name="contentItem">The content item to initialise from.</param>
        /// <param name="itemIndex">The specific prefab index within the content item.</param>
        public virtual void Initialise(ContentItem contentItem, int itemIndex)
        {
            ContentItem = contentItem;
            Type = (WorldObjectType)contentItem.Type;
            ObjectId = Guid.NewGuid();
            GameLayer = contentItem.Traversable ? Helpers.TraversableLayer : Helpers.ImpassableLayer;
            PrefabAddress = contentItem.IDs[itemIndex];
            name = contentItem.Name;
            AutomaticElevation = true;
        }

        /// <summary>
        /// Applies an option to the World Object.
        /// </summary>
        /// <param name="option">The option to apply.</param>
        /// <param name="value">The value to apply to the option.</param>
        public virtual void SetOption(WorldObjectOption option, object value)
        {
            if (OptionValues != null && OptionValues.ContainsKey(option))
            {
                OptionValues[option] = value;
            }
            else
            {
                Debug.LogWarningFormat("WorldObjectBase[{0}] :: SetOption({1}, {2}) :: {3}", name, option.ToString(), value, OptionValues == null ? "OptionValues is null." : "Key not found in OptionValues");
            }
        }

        /// <summary>
        /// Selects this map object.
        /// </summary>
        public virtual void Select()
        {
            // Do nothing if this isn't interactable (e.g. in a networked game but not the host)
            if (!Interactable) return;

            Debug.LogFormat("WorldObjectBase[{0}] :: Select", name);

            if (Current != this)
            {
                Current = this;
                WorldObjectHighlighter.Select(this);
            }
            else
            {
                Debug.LogWarningFormat("WorldObjectBase[{0}] :: Select :: Already selected", name);
            }
        }

        /// <summary>
        /// Deselects this map object.
        /// </summary>
        public virtual void Deselect()
        {
            Debug.LogFormat("WorldObjectBase[{0}] :: Deselect", name);

            if (Current == this)
            {
                // Remove highlighting
                Current = null;
                WorldObjectHighlighter.Select(null);
            }
            else
            {
                Debug.LogWarningFormat("WorldObjectBase[{0}] :: Deselect :: Not selected. Selected is {1}", name, Current == null ? "null" : Current.name);
            }
        }

        /// <summary>
        /// Switches the selection from whichever object is currently selected, to the specified object.
        /// </summary>
        /// <param name="to"></param>
        public virtual void SwitchSelection(WorldObjectBase to = null)
        {
            if (!Interactable) return;

            Debug.LogFormat("WorldObjectBase[{0}] :: SwitchSelection([{1}])", name, to != null ? to.name : "null");

            if (to == null) to = this;
            
            // Deselect the current and select the specified object, or this if that is null
            if (to != Current)
            {
                if (Current)
                    Current.Deselect();
                
                to.Select();
            }
        }

        /// <summary>
        /// Shows any controls that are part of the world object (e.g. drag handles)
        /// </summary>
        public virtual void ShowControls()
        {
            ControlsVisible = true;

            var controlsToggles = GetComponent<WorldObjectControlsToggle>();
            if (controlsToggles) controlsToggles.Show();
        }

        /// <summary>
        /// Hides any controls that are part of the world object (e.g. drag handles)
        /// </summary>
        public virtual void HideControls()
        {
            ControlsVisible = false;

            var controlsToggles = GetComponent<WorldObjectControlsToggle>();
            if (controlsToggles) controlsToggles.Hide();
        }

        /// <summary>
        /// Places the object on the map and deselects it.
        /// </summary>
        public virtual void Place()
        {
            Debug.LogFormat("WorldObjectBase[{0}] :: Place", name);

            UndoController.RegisterAction(ActionType.Create, ObjectId, null);

            Release();
        }

        /// <summary>
        /// Cancels the placement of a new object.
        /// </summary>
        public virtual void CancelPlacement()
        {
            Debug.LogFormat("WorldObjectBase[{0}] :: CancelPlacement", name);

            Deselect();
            Destroy();
        }

        /// <summary>
        /// Rotates the object around its Y axis.
        /// </summary>
        /// <param name="amount">The amount of rotation to apply in degrees.</param>
        public virtual void Rotate(float amount)
        {
            // Rotate from the current position by the specified amount
            RotateTo(transform.localEulerAngles.y + amount);
        }

        /// <summary>
        /// Rotates to the desired angle. Snaps to grid if active.
        /// </summary>
        /// <param name="targetAngle">The desired angle. This may be changed to a 90 degree angle if snap to grid is set.</param>
        /// <param name="axis">The axis to rotate on.</param>
        public virtual void RotateTo(float targetAngle, SnapAxis axis = SnapAxis.Y)
        {
            if (!Spawned) return;

            // Snap to a grid position if required
            if (Helpers.Settings.editorSettings.snapToGrid) targetAngle = Mathf.Round(targetAngle / 90) * 90;

            LocalRotation = new Vector3(axis == SnapAxis.X ? targetAngle : LocalRotation.x,
                                        axis == SnapAxis.Y ? targetAngle : LocalRotation.y,
                                        axis == SnapAxis.Z ? targetAngle : LocalRotation.z);
            transform.localRotation = Quaternion.Euler(LocalRotation);
        }

        /// <summary>
        /// Raises or lowers the object.
        /// </summary>
        /// <param name="amount">The amount to elevate by in game units.</param>
        public virtual void Elevate(float amount)
        {
            if (!Spawned) return;

            // Clamp to min/max elevation
            var targetElevation = Mathf.Clamp(transform.position.y + amount, GameTerrain.Current.OffsetAltitude(-5f), GameTerrain.Current.OffsetAltitude(20f));
            ElevateTo(targetElevation);
        }

        /// <summary>
        /// Elevate to the specified height. Will snap to grid when enabled.
        /// </summary>
        /// <param name="targetElevation">The desired elevation in game units, ignoring the elevation of the ground underneath (absolute units).</param>
        public virtual void ElevateTo(float targetElevation)
        {
            if (!Spawned) return;

            if (Helpers.Settings.editorSettings.snapToGrid) targetElevation = Mathf.Round(targetElevation);

            // If automatic, don't let the object go below the min elevation to avoid z-fighting on floors
            if (AutomaticElevation) targetElevation = Mathf.Clamp(targetElevation, GameTerrain.Current.MinPlacementElevation, GameTerrain.Current.OffsetAltitude(20f));

            var currentPosition = transform.position;
            currentPosition.y = targetElevation;
            transform.position = currentPosition;
        }

        /// <summary>
        /// Moves the object by the specified amount.
        /// </summary>
        /// <param name="moveAmount">The amount to move from the current position. This is incremental on the current position.</param>
        public virtual void Move(Vector3 moveAmount)
        {
            if (moveAmount != Vector3.zero)
            {
                var targetPosition = gameObject.transform.position + moveAmount;
                MoveTo(targetPosition);
            }
        }

        /// <summary>
        /// Move the object to the specified position.
        /// </summary>
        /// <param name="targetPosition">The position to move the object to.</param>
        public virtual void MoveTo(Vector3 targetPosition)
        {
            if (!Spawned) return;

            if (Helpers.Settings.editorSettings.snapToGrid)
            {
                // Snap to the centre of the nearest grid square, but maintain altitude as the grid is 3D
                var targetY = targetPosition.y;
                var grid = GameTerrain.Current.GetComponent<Grid>();
                targetPosition = grid.GetCellCenterWorld(grid.WorldToCell(targetPosition));
                targetPosition.y = targetY;
            }

            // If on the floor, nudge up slightly to avoid z-fighting
            if (AutomaticElevation) targetPosition.y = Mathf.Max(targetPosition.y, GameTerrain.Current.MinPlacementElevation);

            // Apply the position
            gameObject.transform.position = targetPosition;
        }

        /// <summary>
        /// Scale the object by the specified amount.
        /// </summary>
        /// <param name="amount">The amount to scale by. 1 is original size.</param>
        public virtual void Scale(float amount)
        {
            if (!Spawned) return;

            ScaleTo(gameObject.transform.localScale.x + amount);
        }

        /// <summary>
        /// Scales the object to the specified multiplier.
        /// </summary>
        /// <param name="targetScale">The target scale multiplier.</param>
        public virtual void ScaleTo(float targetScale)
        {
            targetScale = Mathf.Clamp(targetScale, .5f, 3f);
            gameObject.transform.localScale = targetScale * ScaleMultiplier * Vector3.one;
        }

        /// <summary>
        /// Pick the item up, becoming transparent to rays.
        /// </summary>
        public virtual void PickUp()
        {
            Debug.LogFormat("WorldObjectBase[{0}] :: PickUp", name);

            SetLayerRecursive(gameObject, Helpers.IgnoreRaycastLayer);
            InPlacementMode = true;
        }

        /// <summary>
        /// Set item down, optionally snapping back to its previous position.
        /// </summary>
        public virtual void Release()
        {
            Debug.LogFormat("WorldObjectBase[{0}] :: Release", name);

            SetLayerRecursive(gameObject, GameLayer);
            InPlacementMode = false;

            if (StateController.CurrentStateType == StateType.ItemPlacement)
            {
                // If placing a new object, stop tracking it
                Deselect();
            }
        }

        /// <summary>
        /// Releases the gameObject this is attached to.
        /// </summary>
        public virtual void Destroy()
        {
            Debug.LogFormat("WorldObjectBase[{0}] :: Destroy", name);

            if (this == Current) Deselect();

            // Remove this from the list
            if (All.Contains(this)) All.Remove(this);

            Addressables.ReleaseInstance(gameObject);
        }

        /// <summary>
        /// Create a new object from another.
        /// </summary>
        /// <param name="fromObject">The object to base this object on.</param>
        /// <param name="prefabAddress">The Addressables address of the prefab to set as this object's address. If empty
        ///     then the PrefabAddress of the fromObject will be used.</param>
        /// <param name="generateNewObjectId">If true a new object ID will be used, otherwise it will be taken from the
        ///     fromObject. Note: no two objects should have the same ID so only pass false to this parameter if the
        ///     fromObject doesn't exist on this map.</param>
        /// <param name="contentItem">The ContentItem to set for this object. If empty the ContentItem of the fromObject
        ///     will be used.</param>
        public virtual void CloneFrom(WorldObjectBase fromObject, string prefabAddress = null, bool generateNewObjectId = true, ContentItem contentItem = null)
        {
            Debug.LogFormat("WorldObjectBase[{0}] :: CloneFrom({1})", name, fromObject.name);

            ContentItem = contentItem ?? fromObject.ContentItem;
            Type = (WorldObjectType)ContentItem.Type;
            ObjectId = generateNewObjectId ? Guid.NewGuid() : fromObject.ObjectId;
            GameLayer = fromObject.GameLayer;
            PrefabAddress = prefabAddress ?? fromObject.PrefabAddress;
            name = fromObject.name;

            CloneTransformFrom(fromObject);

            SetLayerRecursive(gameObject, GameLayer);

            foreach (var option in fromObject.OptionValues)
            {
                OptionValues.Add(option.Key, option.Value);
            }
        }

        /// <summary>
        /// Clones the transform properties from another object, i.e. the position, rotation and scale.
        /// </summary>
        /// <param name="fromObject">The object to clone the transform from.</param>
        public virtual void CloneTransformFrom(WorldObjectBase fromObject)
        {
            Debug.LogFormat("WorldObjectBase[{0}] :: CloneTransformFrom({1})", name, fromObject == null ? "null" : fromObject.name);

            gameObject.transform.position = fromObject.gameObject.transform.position;
            LocalRotation = fromObject.LocalRotation;
            transform.localRotation = Quaternion.Euler(LocalRotation);
            gameObject.transform.localScale = fromObject.gameObject.transform.localScale;
        }

        /// <summary>
        /// Highlight the object as selectable and change the cursor.
        /// </summary>
        /// <param name="hoveredObject">The object that was hovered over.</param>
        /// <param name="position">The mouse position.</param>
        public virtual void MouseOver(GameObject hoveredObject, Vector3 position)
        {
            WorldObjectHighlighter.Highlight(this);

            if (Selected && Draggable)
            {
                // Move cursor
                CursorController.Current.MoveObject = true;
            }
            else if (!Selected)
            {
                // Select cursor
                CursorController.Current.SelectObject = true;
            }
        }

        /// <summary>
        /// Select or drag the object.
        /// </summary>
        /// <param name="clickedObject">The object that was clicked on.</param>
        /// <param name="position">The mouse position.</param>
        public virtual void Click(GameObject clickedObject, Vector3 position)
        {
            if (Selected && Draggable)
            {
                // Selected and ready to drag
                Draggable.StartDrag();
            }
            else if (!Selected)
            {
                SwitchSelection();
            }
        }

        /// <summary>
        /// Converts the object to a MapObject.
        /// </summary>
        /// <returns>The base class of a MapObject.</returns>
        public abstract BaseObjectData ToDataObject();

        #endregion


        #region Private methods

        /// <summary>
        /// Sets the layer on the given object and all its child objects.
        /// </summary>
        /// <param name="obj">The object to set the layer on.</param>
        /// <param name="targetLayer">The layer to set on the object.</param>
        protected void SetLayerRecursive(GameObject obj, int targetLayer)
        {
            if (!obj.CompareTag("DoNotChangeLayer")) obj.layer = targetLayer;

            foreach (Transform child in obj.transform)
            {
                if (child != null && !child.CompareTag("DoNotChangeLayer")) SetLayerRecursive(child.gameObject, targetLayer);
            }
        }

        /// <summary>
        /// Ensures all objects in the object tree have a collider. Iterates through all children of the specified object.
        /// </summary>
        /// <param name="obj">A GameObject to add colliders to.</param>
        protected void AddColliderRecursive(GameObject obj)
        {
            if (obj.GetComponent<Collider>() == null)
            {
                // Add a collider if the object has none
                obj.AddComponent<BoxCollider>();

                // Add to sub-components in case of a complex multi-component object
                // Only if the parent has no collider, assuming an existing parent collider covers the whole object
                foreach (Transform child in obj.transform)
                {
                    AddColliderRecursive(child.gameObject);
                }
            }
        }

        /// <summary>
        /// Stores the current state of this object in a data class and returns it.
        /// </summary>
        /// <typeparam name="T">A type that derives from BaseObjectData and implements a parameterless constructor.</typeparam>
        /// <returns>An instance of T with the general object properties populated.</returns>
        protected T ToMapObject<T>() where T : BaseObjectData, new()
        {
            //TODO: Rename to ToDataObject
            var thisTransform = transform;
            
            return new T()
            {
                objectId = ObjectId.ToString(),
                name = gameObject.name,
                position = new VectorData(thisTransform.position),
                rotation = new VectorData(LocalRotation),
                scale = new VectorData(thisTransform.localScale),
                prefabAddress = PrefabAddress,
                gameLayer = LayerMask.LayerToName(GameLayer),
                options = ConvertOptionsDictionaryToList(OptionValues),
                starred = Starred,
                scaleMultiplier = ScaleMultiplier,
                objectType = (int)Type,
            };
        }

        /// <summary>
        /// Loads the basic object properties from the passed in data class.
        /// </summary>
        /// <param name="objectData">The object data to load.</param>
        protected void FromMapObject(BaseObjectData objectData)
        {
            //TODO: Rename to FromDataObject
            ObjectId = Guid.Parse(objectData.objectId);
            gameObject.name = objectData.name;
            transform.position = objectData.position.ToVector3();
            LocalRotation = objectData.rotation.ToVector3();
            gameObject.transform.localRotation = Quaternion.Euler(LocalRotation);
            transform.localScale = objectData.scale.ToVector3();
            PrefabAddress = objectData.prefabAddress;
            GameLayer = LayerMask.NameToLayer(objectData.gameLayer);
            OptionValues = ConvertOptionsListToDictionary(objectData.options);
            Starred = objectData.starred;
            ScaleMultiplier = objectData.scaleMultiplier;
            Type = (WorldObjectType)objectData.objectType;

            SetLayerRecursive(gameObject, GameLayer);
        }
        
        /// <summary>
        /// Converts an internal Dictionary to List for serialization.
        /// </summary>
        /// <param name="dictionary">A dictionary of WordObjectOptions with their values.</param>
        /// <returns>A serializable list of ObjectOptionData.</returns>
        private static List<ObjectOptionData> ConvertOptionsDictionaryToList(Dictionary<WorldObjectOption, object> dictionary)
        {
            //TODO: Refactor map object options to no longer need this
            if (dictionary == null) return null;

            return dictionary.Select(x => new ObjectOptionData() { option = (int)x.Key, value = x.Value.ToString(), valueType = x.Value.GetType().FullName }).ToList();
        }

        /// <summary>
        /// Converts a List from serialization to an internal Dictionary.
        /// </summary>
        /// <param name="options">A list of ObjectOptionData objects.</param>
        /// <returns>A dictionary of WorldObjectOptions and their values.</returns>
        private static Dictionary<WorldObjectOption, object> ConvertOptionsListToDictionary(List<ObjectOptionData> options)
        {
            if (options == null) return null;

            return options.ToDictionary(x => (WorldObjectOption)x.option, x => x.ParsedValue);
        }


        #endregion
    }
}