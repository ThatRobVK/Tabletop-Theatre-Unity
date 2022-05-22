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

using TT.State;
using TT.World;
using UnityEngine;
using UnityEngine.UI;

namespace TT.UI.MapEditor
{
    [RequireComponent(typeof(Toggle))]
    public class IndoorZoneTabButton : MonoBehaviour
    {

        #region Lifecycle events
#pragma warning disable IDE0051 // Unused members

        void Start()
        {
            // Handle toggle events and initialise
            var toggle = GetComponent<Toggle>();
            toggle.onValueChanged.AddListener(HandleValueChanged);
        }

#pragma warning restore IDE0051 // Unused members
        #endregion


        #region Event handlers

        /// <summary>
        /// Called when the toggle value changes. Toggle the connected canvas group.
        /// </summary>
        /// <param name="state">The new state of the toggle.</param>
        private void HandleValueChanged(bool state)
        {
            // If a supported object is selected, deselect it - if it is placing then cancel that
            if (!state && WorldObjectBase.Current is ScalableObject)
            {
                if (StateController.CurrentStateType == StateType.ItemPlacement)
                {
                    WorldObjectBase.Current.CancelPlacement();
                    StateController.Current.ChangeState(StateType.EditorIdleState);
                }

                WorldObjectBase.Current.Deselect();
            }
        }

        #endregion

    }
}