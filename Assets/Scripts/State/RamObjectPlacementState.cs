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

using TT.Data;
using TT.InputMapping;
using TT.Shared.GameContent;
using TT.World;
using UnityEngine;

namespace TT.State
{
    public class RamObjectPlacementState : StateBase
    {
        public override bool IsPlacementState => true;

        #region Base class overrides

        public RamObjectPlacementState(StateController stateController) : base(stateController)
        { }

        public override void Enable()
        {
            Debug.Log("RamObjectPlacementState :: Enable");
        }

        public override void Disable()
        {
            Debug.Log("RamObjectPlacementState :: Disable");

            if (WorldObjectBase.Current)
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
            if (InputMapper.Current.WorldObjectInput.PlaceObject && !Helpers.IsPointerOverUIElement())
            {
                WorldObjectBase.Current.Place();
            }

            // Cancel placement mode
            if (InputMapper.Current.WorldObjectInput.Cancel)
            {
                ToIdle();
            }
        }

        public override void ToIdle()
        {
            StateController.Current.ChangeState(StateType.RamObjectIdle);
        }

        public override void ToPlacement()
        { }
        
        #endregion

        #region Public methods

        /// <summary>
        /// Starts placement of the specified content item.
        /// </summary>
        /// <param name="contentItem">The data definition of the object to instantiate.</param>
        /// <param name="itemIndex">The index to instantiate, or 0 to instantiate a random index.</param>
        /// <returns>A boolean to indicate whether a new instance was spawned and placement started.</returns>
        public bool Initialise(ContentItem contentItem, int itemIndex)
        {
            Debug.LogFormat("RamObjectPlacementState :: Initialise({0})", itemIndex);

            WorldObjectFactory.CreateFromContent(contentItem, itemIndex).ConfigureAwait(false);

            return true;
        }

        #endregion

    }
}