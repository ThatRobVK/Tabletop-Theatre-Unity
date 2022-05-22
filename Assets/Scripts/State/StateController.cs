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
using TT.Data;
using UnityEngine;

namespace TT.State
{
    public class StateController
    {
        #region Events

        // Events
        public static event Action<StateController> StateChanged;

        #endregion


        #region Singleton

        private static StateController _stateController;
        public static StateController Current
        {
            get
            {
                if (_stateController == null)
                {
                    Debug.LogWarning("StateController accessed before initialisation. Defaulting to Game.");
                    _stateController = new StateController();
                }
                return _stateController;
            }
        }

        #endregion

        #region Public properties

        /// <summary>
        /// The type of state currently active.
        /// </summary>
        public static StateType CurrentStateType { get; private set; }

        /// <summary>
        /// The actual state class currently active.
        /// </summary>
        public static StateBase CurrentState { get; private set; }

        #endregion


        #region Private fields

        // State controllers
        private Dictionary<StateType, StateBase> availableStates;

        #endregion


        #region Constructors

        public StateController()
        {
            // Set up available states
            availableStates = new Dictionary<StateType, StateBase>
            {
                { StateType.EditorIdleState, new EditorIdleState(this, null) },
                { StateType.TerrainPaint, new TerrainPaintState(this) },
                { StateType.ItemIdle, new EditorIdleState(this, new List<WorldObjectType> {WorldObjectType.Item}) },
                { StateType.ItemPlacement, new WorldObjectPlacementState(this) },
                { StateType.RamObjectIdle, new EditorIdleState(this, new List<WorldObjectType> {WorldObjectType.Road, WorldObjectType.River, WorldObjectType.Bridge}) },
                { StateType.RamObjectPlacement, new RamObjectPlacementState(this) },
                { StateType.NatureObjectIdle, new EditorIdleState(this, new List<WorldObjectType> {WorldObjectType.ScatterArea, WorldObjectType.NatureObject}) },
                { StateType.NatureObjectPlacement, new WorldObjectPlacementState(this) },
                { StateType.ScalableObjectIdle, new EditorIdleState(this, new List<WorldObjectType> {WorldObjectType.ScalableObject}) },
                { StateType.ScalableObjectPlacement, new WorldObjectPlacementState(this) },
                { StateType.BuildingObjectIdle, new EditorIdleState(this, new List<WorldObjectType> {WorldObjectType.Building, WorldObjectType.Construction, WorldObjectType.ConstructionProp}) },
                { StateType.BuildingObjectPlacement, new WorldObjectPlacementState(this) },
                { StateType.LightingObjectIdle, new EditorIdleState(this, new List<WorldObjectType> {WorldObjectType.Lightsource}) },
                { StateType.LightingObjectPlacement, new WorldObjectPlacementState(this) },
                { StateType.SoundObjectIdle, new EditorIdleState(this, new List<WorldObjectType> {WorldObjectType.Sound}) },
                { StateType.SoundObjectPlacement, new WorldObjectPlacementState(this) },
                { StateType.NpcIdle, new EditorIdleState(this, new List<WorldObjectType> {WorldObjectType.Npc}) },
                { StateType.NpcPlacement, new WorldObjectPlacementState(this) },
            };

            // Assign singleton
            _stateController = this;
        }

        #endregion


        #region Public methods

        // 
        /// <summary>
        /// Switch to a specified state if it is available.
        /// </summary>
        /// <param name="stateType">The state type to switch to.</param>
        public void ChangeState(StateType stateType)
        {
            if ((CurrentState == null || CurrentStateType != stateType) && availableStates.ContainsKey(stateType))
            {
                Debug.LogFormat("StateController :: ChangeState({0})", stateType.ToString());

                CurrentState?.Disable();
                CurrentState = availableStates[stateType];
                CurrentStateType = stateType;
                CurrentState.Enable();

                StateChanged?.Invoke(this);
            }
        }

        // Tell the current state to update
        public void Update()
        {
            CurrentState?.Update();
        }

        #endregion
    }
}