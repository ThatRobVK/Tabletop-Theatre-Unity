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
using TT.State;
using UnityEngine;
using UnityEngine.UI;

namespace TT.UI.MapEditor.ActionBar
{
    [RequireComponent(typeof(Toggle))]
    public class StateToggle : MonoBehaviour
    {

        #region Editor fields
#pragma warning disable IDE0044 // Make fields read-only

        [SerializeField] private List<StateType> stateType = new List<StateType>();

#pragma warning restore IDE0044
        #endregion


        #region Private fields

        private Toggle _toggle;

        #endregion

 
        #region Lifecycle events
#pragma warning disable IDE0051 // Unused members

       void Start()
        {
            _toggle = GetComponent<Toggle>();
            _toggle.onValueChanged.AddListener(HandleValueChanged);
        }

        void Update()
        {
            if (!_toggle.isOn && stateType.Contains(StateController.CurrentStateType))
            {
                // Turn on when the required state is activated
                _toggle.isOn = true;
            }
            else if (_toggle.isOn && !stateType.Contains(StateController.CurrentStateType) && !StateController.CurrentState.IsPlacementState)
            {
                // Turn off when the required state is deactivated
                // Don't toggle when in a placement state to stop the window from opening and closing while placing items
                _toggle.isOn = false;
            }
        }

#pragma warning restore IDE0051 // Unused members
        #endregion


        #region Event handlers

        private void HandleValueChanged(bool toggleValue)
        {
            // When toggled on, change the state
            if (toggleValue && !stateType.Contains(StateController.CurrentStateType) && stateType.Count > 0)
            {
                StateController.Current.ChangeState(stateType[0]);
            }
        }

        #endregion

    }
}