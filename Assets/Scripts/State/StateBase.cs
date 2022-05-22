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

using System.Collections.Generic;
using TT.Data;

namespace TT.State
{
    public abstract class StateBase
    {
        protected StateController StateController;

        public StateBase(StateController stateController)
        {
            this.StateController = stateController;
        }

        public abstract void Enable();
        public abstract void Disable();
        public abstract void Update();

        public abstract void ToIdle();
        public abstract void ToPlacement();

        /// <summary>
        /// Changes to an Idle state based on the specified WorldObjectType.
        /// </summary>
        /// <param name="type">The type of object to go to an idle state for.</param>
        public virtual void ToIdle(WorldObjectType type)
        {
            StateController.Current.ChangeState(TypeToIdleStateMap[type]);
        }

        public abstract bool IsPlacementState { get; }

        protected readonly Dictionary<WorldObjectType, StateType> TypeToIdleStateMap = new Dictionary<WorldObjectType, StateType>
        {
            {WorldObjectType.Bridge, StateType.RamObjectIdle},
            {WorldObjectType.Building, StateType.BuildingObjectIdle},
            {WorldObjectType.Construction, StateType.BuildingObjectIdle},
            {WorldObjectType.ConstructionProp, StateType.BuildingObjectIdle},
            {WorldObjectType.Lightsource, StateType.LightingObjectIdle},
            {WorldObjectType.Item, StateType.ItemIdle},
            {WorldObjectType.NatureObject, StateType.NatureObjectIdle},
            {WorldObjectType.River, StateType.RamObjectIdle},
            {WorldObjectType.Road, StateType.RamObjectIdle},
            {WorldObjectType.ScalableObject, StateType.ScalableObjectIdle},
            {WorldObjectType.ScatterArea, StateType.NatureObjectIdle}
        };

        protected readonly Dictionary<StateType, StateType> IdleToPlacementStateMap = new Dictionary<StateType, StateType>
        {
            {StateType.RamObjectIdle, StateType.RamObjectPlacement},
            {StateType.BuildingObjectIdle, StateType.BuildingObjectPlacement},
            {StateType.LightingObjectIdle, StateType.LightingObjectPlacement},
            {StateType.ItemIdle, StateType.ItemPlacement},
            {StateType.NatureObjectIdle, StateType.NatureObjectPlacement},
            {StateType.ScalableObjectIdle, StateType.ScalableObjectPlacement}
        };

    }
}