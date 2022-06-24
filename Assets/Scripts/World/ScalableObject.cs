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
using System.Threading.Tasks;
using TT.Data;
using TT.MapEditor;
using TT.Shared.UserContent;
using TT.UI;
using UnityEngine;

namespace TT.World
{
    public class ScalableObject : WorldObjectBase
    {
        #region Editor fields

        public GameObject floorPlane;
        public GameObject roofPlane;

        #endregion


        #region Public properties

        public override Vector3 Position { get => transform.position; }

        #endregion


        #region Private fields

        private const float MAGNITUDE_MULTIPLIER = 0.1f;
        private List<ScalableObjectHandle> _resizeHandles;
        private Vector3 _undoPosition;

        #endregion


        #region Event handlers

        private void HandleDrag(DraggableObject draggableObject)
        {
            if (draggableObject is ScalableObjectHandle handle)
            {
                // Dragging a handle
                var handleMovementVector = handle.GetMovementVector();
                var newPosition = handle.transform.position;

                // Get mouse position and snap it to grid if required
                var mousePosition = Helpers.GetWorldPointFromMouse();
                if (Helpers.Settings.editorSettings.snapToGrid)
                {
                    // Snap to the centre of the nearest grid square
                    var grid = GameTerrain.Current.GetComponent<Grid>();
                    mousePosition = grid.GetCellCenterWorld(grid.WorldToCell(mousePosition));
                }

                // Move the relevant axis of the handle to the mouse position
                if (handleMovementVector.x != 0)
                {
                    newPosition.x = mousePosition.x;
                }
                else
                {
                    newPosition.z = mousePosition.z;
                }
                handle.ResizeTo(newPosition);

                MoveAndScale(handle.transform.position, handle.GetOpposingHandlePosition(), handleMovementVector);
            }
            else
            {
                // Dragging the zone itself
                MoveTo(Helpers.GetWorldPointFromMouse(Helpers.StackableMask));
            }
        }

        private void HandleStartDrag(DraggableObject draggableObject)
        {
            if (draggableObject is ScalableObjectHandle)
            {
                // Not implementing this as this class is due for replacement and it would be too much work, not worth it rn
            }
            else
            {
                // Store object position
                _undoPosition = gameObject.transform.position;
            }
        }

        private void HandleStopDrag(DraggableObject draggableObject)
        {
            if (draggableObject is ScalableObjectHandle)
            {
                // Not implementing this as this class is due for replacement and it would be too much work, not worth it rn
            }
            else
            {
                // Register move undo
                UndoController.RegisterAction(ActionType.Move, ObjectId, _undoPosition);
            }
        }

        #endregion


        #region Public methods

        public ScalableObject()
        {
            OptionValues = new Dictionary<WorldObjectOption, object>() { { WorldObjectOption.Roof, true } };
        }

        // Initialises the Scalable Object based on a Content Item
        public override void Initialise(ContentItem contentItem, int itemIndex)
        {
            ContentItem = contentItem;
            ObjectId = System.Guid.NewGuid();

            _resizeHandles = new List<ScalableObjectHandle>(GetComponentsInChildren<ScalableObjectHandle>());
            GameLayer = Helpers.TraversableLayer;

            gameObject.name = string.Empty;
            SetMaterial(Content.Current.Combined.Construction.Floors[itemIndex]).ConfigureAwait(false);

            // If this object is draggable, attach to the drag event
            var thisDraggable = GetComponent<DraggableObject>();
            if (thisDraggable)
            {
                thisDraggable.OnStartDrag += HandleStartDrag;
                thisDraggable.OnDrag += HandleDrag;
                thisDraggable.OnStopDrag += HandleStopDrag;
            }
            
            Select();
            PickUp();
        }

        // Initialises the Scalable Object based on a Map Object
        public void Initialise(MapScalableObject mapObject)
        {
            //FromMapObject(mapObject);
            transform.localScale = Vector3.one;

            floorPlane.transform.position = mapObject.position;
            floorPlane.transform.rotation = Quaternion.Euler(mapObject.rotation);
            floorPlane.transform.localScale = mapObject.scale;

            roofPlane.transform.position = mapObject.position + Vector3.up * 3;
            roofPlane.transform.rotation = Quaternion.Euler(new Vector3(180, mapObject.rotation.y, mapObject.rotation.z));
            roofPlane.transform.localScale = mapObject.scale;

            _resizeHandles = new List<ScalableObjectHandle>(GetComponentsInChildren<ScalableObjectHandle>());
            foreach (var handle in _resizeHandles)
            {
                var movementVector = handle.GetMovementVector().normalized;
                var gameObjectPosition = gameObject.transform.position;
                var roofPlaneScale = roofPlane.transform.localScale;

                if (movementVector.x > 0)
                {
                    handle.transform.position = new Vector3(gameObjectPosition.x + roofPlaneScale.x * 5 + .5f, gameObjectPosition.y + .01f, gameObjectPosition.z);
                }
                else if (movementVector.x < 0)
                {
                    handle.transform.position = new Vector3(gameObjectPosition.x - roofPlaneScale.x * 5 - .5f, gameObjectPosition.y + .01f, gameObjectPosition.z);
                }
                else if (movementVector.z > 0)
                {
                    handle.transform.position = new Vector3(gameObjectPosition.x, gameObjectPosition.y + .01f, gameObjectPosition.z + roofPlaneScale.z * 5 + .5f);
                }
                else
                {
                    handle.transform.position = new Vector3(gameObjectPosition.x, gameObjectPosition.y + .01f, gameObjectPosition.z - roofPlaneScale.z * 5 - .5f);
                }
            }

            ContentItem = new ContentItem()
            {
                Type = WorldObjectType.ScalableObject,
            };

            SetMaterial(PrefabAddress).ConfigureAwait(false);
            roofPlane.SetActive((bool)OptionValues[WorldObjectOption.Roof]);
            ToggleHandles(false);
        }

        // Returns a MapScalableObject representing this Scalable Object
        public MapObjectBase ToMapObject()
        {
            return null;
        }

        // Apply an option to the Scalable Object
        public override void SetOption(WorldObjectOption option, object value)
        {
            base.SetOption(option, value);

            roofPlane.SetActive((bool)OptionValues[WorldObjectOption.Roof]);
        }

        public override void ShowControls()
        {
            ToggleHandles(true);
        }

        public override void HideControls()
        {
            ToggleHandles(false);
        }

        private void ToggleHandles(bool show)
        {
            foreach (var handle in _resizeHandles)
            {
                handle.gameObject.SetActive(show);
                if (show)
                {
                    handle.OnDrag += HandleDrag;
                }
                else
                {
                    handle.OnDrag -= HandleDrag;
                }
            }
        }

        public override void MoveTo(Vector3 targetPosition)
        {
            // Offset the movement if the floor has scaled as its visible position won't be the same as
            // the game object's invisible position (but not the altitude axis)
            var gameObjectPosition = gameObject.transform.position;
            var planePosition = floorPlane.transform.position;
            targetPosition.x += gameObjectPosition.x - planePosition.x;
            targetPosition.z += gameObjectPosition.z - planePosition.z;


            // Offset the movement by the distance between the centre of the plane and the invisible game object
            base.MoveTo(targetPosition);
        }

        // Moves and scales the plane to fill the space between two handles
        public void MoveAndScale(Vector3 handlePosition, Vector3 oppositeHandlePosition, Vector3 movementVector)
        {
            // Position the plane half-way between the two handles
            var floorPosition = floorPlane.transform.position;
            var roofPosition = Vector3.Lerp(handlePosition, oppositeHandlePosition, 0.5f);
            floorPlane.transform.position = new Vector3(roofPosition.x, floorPosition.y, roofPosition.z);
            roofPlane.transform.position = floorPosition + Vector3.up * 3;

            // Scale the plane so it fills the space between the handles
            var scaleVector = new Vector3((movementVector.x != 0) ? movementVector.magnitude * MAGNITUDE_MULTIPLIER - 0.1f : floorPlane.transform.localScale.x,
                                            floorPlane.transform.localScale.y,
                                            (movementVector.z != 0) ? movementVector.magnitude * MAGNITUDE_MULTIPLIER - 0.1f : floorPlane.transform.localScale.z);
            floorPlane.transform.localScale = scaleVector;
            roofPlane.transform.localScale = scaleVector;

            // Update the material scale to tile correctly
            ScaleMaterial();

            // Tell each handle to update its position along the new shape of the plane
            _resizeHandles.ForEach(x => x.MoveToCentreOfParent(parentPosition: floorPlane.transform.position, triggeringMovementVector: movementVector));
        }

        /// <summary>
        /// Loads the material and sets it on the plane
        /// </summary>
        /// <param name="newMaterialAddress">The addressable location of the material to load.</param>
        public async Task SetMaterial(string newMaterialAddress)
        {
            PrefabAddress = newMaterialAddress;

            var materials = await Helpers.LoadAddressables<Material>(new[] { newMaterialAddress });

            if (materials != null && materials.Count > 0)
            {
                floorPlane.GetComponent<Renderer>().material = materials[0];
                roofPlane.GetComponent<Renderer>().material = materials[0];
                ScaleMaterial();
            }
        }

        // Do not rotate
        public override void Rotate(float amount)
        {
        }

        // Do not scale with keyboard
        public override void Scale(float amount)
        {
        }

        // Do not elevate
        public override void Elevate(float amount)
        {
        }

        public override void MouseOver(GameObject hoveredObject, Vector3 position)
        {
            if (hoveredObject.GetComponent<DraggableObject>() is ScalableObjectHandle handle)
            {
                if (handle.GetMovementVector().x != 0)
                {
                    CursorController.Current.ScaleHorizontal = true;
                }
                else
                {
                    CursorController.Current.ScaleVertical = true;
                }
            }
            else
            {
                base.MouseOver(hoveredObject, position);
            }
        }

        public override void Click(GameObject clickedObject, Vector3 position)
        {
            if (clickedObject.GetComponent<DraggableObject>() is ScalableObjectHandle handle)
            {
                handle.StartDrag();
            }
            else
            {
                base.Click(clickedObject, position);
            }
        }

        public override BaseObjectData ToDataObject()
        {
            throw new System.NotImplementedException();
        }

        #endregion


        #region Private methods

        // Scales the material to the current size
        private void ScaleMaterial()
        {
            var floorPlaneScale = floorPlane.transform.lossyScale;
            var roofPlaneScale = roofPlane.transform.lossyScale;
            floorPlane.GetComponent<Renderer>().material.mainTextureScale = new Vector2(floorPlaneScale.x / MAGNITUDE_MULTIPLIER / 2, floorPlaneScale.z / MAGNITUDE_MULTIPLIER / 2);
            roofPlane.GetComponent<Renderer>().material.mainTextureScale = new Vector2(roofPlaneScale.x / MAGNITUDE_MULTIPLIER / 2, roofPlaneScale.z / MAGNITUDE_MULTIPLIER / 2);
        }

        #endregion
    }
}