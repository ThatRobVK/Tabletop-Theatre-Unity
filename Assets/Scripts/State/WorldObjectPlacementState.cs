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

using UnityEngine;
using TT.InputMapping;
using TT.Shared.GameContent;
using TT.World;

namespace TT.State
{
    public class WorldObjectPlacementState : StateBase
    {
        #region Private fields
        
        private ContentItemCategory _category;
        private ContentItem _contentItem;
        
        #endregion

        
        #region Public properties
        
        public override bool IsPlacementState => true;
        
        #endregion
        

        #region Constructor

        public WorldObjectPlacementState(StateController stateController) : base(stateController)
        { }
        
        #endregion
        
        
        #region Public methods

        public override void Enable()
        {
            Debug.Log("WorldObjectPlacementState :: Enable");
        }

        public override void Disable()
        {
            Debug.Log("WorldObjectPlacementState :: Disable");

            if (WorldObjectBase.Current && WorldObjectBase.Current.InPlacementMode)
            {
                WorldObjectBase.Current.CancelPlacement();
            }
        }

        public override void Update()
        {
            // If the object hasn't yet initialised, don't do anything
            if (WorldObjectBase.Current == null) return;

            // Move object to mouse
            var mousePosition = WorldObjectBase.Current.AutomaticElevation ? Helpers.GetWorldPointFromMouse(Helpers.StackableMask) : Helpers.GetWorldPointFromMouse(WorldObjectBase.Current.transform.position.y);
            WorldObjectBase.Current.MoveTo(mousePosition);

            // Place item on map
            if (InputMapper.Current.WorldObjectInput.PlaceObject && !Helpers.IsPointerOverUIElement()) PlaceObjectOnMap();

            // Cancel placement mode
            if (InputMapper.Current.WorldObjectInput.Cancel) ToIdle();
        }

        public override void ToIdle()
        {
            StateController.Current.ChangeState(TypeToIdleStateMap[WorldObjectBase.Current.Type]);
        }

        public override void ToPlacement()
        { }
        
        /// <summary>
        /// Starts placement of the specified content item.
        /// </summary>
        /// <param name="contentItem">The data definition of the object to instantiate.</param>
        /// <returns>A boolean to indicate whether a new instance was spawned and placement started.</returns>
        public bool Initialise(ContentItem contentItem)
        {
            Debug.LogFormat("WorldObjectPlacementState :: Initialise({0})", contentItem.Name);

            _category = null;
            _contentItem = contentItem;
            SpawnObject();

            return true;
        }

        public bool Initialise(ContentItemCategory category)
        {
            Debug.LogFormat("WorldObjectPlacementState :: Initialise({0})", category.Name);

            _category = category;
            _contentItem = null;
            SpawnObject();

            return true;
        }

        #endregion

        
        #region Private methods

        /// <summary>
        /// Place the current item on the map. Remain in placement mode if in Continuous Placement, or revert to Idle otherwise.
        /// </summary>
        private void PlaceObjectOnMap()
        {
            Debug.Log("WorldObjectPlacementState :: PlaceObjectOnMap");

            var previousObject = WorldObjectBase.Current;

            // Tell the world object to place itself
            WorldObjectBase.Current.Place();

            if (Helpers.Settings.editorSettings.continuousPlacement && !previousObject.ContinuousPlacementMode)
            {
                // If in continuous placement mode, and the object doesn't spawn itself (e.g. RAM or Polygon) then spawn a new object
                SpawnObject(previousObject);
            }
            else if (!previousObject.ContinuousPlacementMode)
            {
                // Change to an idle state
                StateController.Current.ChangeState(TypeToIdleStateMap[previousObject.Type]);
            }
        }

        /// <summary>
        /// Spawns a new object based on the current ContentItem.
        /// </summary>
        /// <param name="cloneFrom">(optional) An object to clone the transform from.</param>
        private async void SpawnObject(WorldObjectBase cloneFrom = null)
        {
            ContentItem contentItemToPlace = _contentItem ?? _category.Items[Random.Range(0, _category.Items.Length)];
            var itemNumber = contentItemToPlace.IDs.Length > 1 ? Random.Range(0, contentItemToPlace.IDs.Length) : 0;

            // Randomise item index if it is 0, otherwise use the one given
            var newWorldObject = await WorldObjectFactory.CreateFromContent(contentItemToPlace, itemNumber);

            // Clone the transform from the last object, if supplied
            if (cloneFrom) newWorldObject.CloneTransformFrom(cloneFrom);
        }

        #endregion
    }
}