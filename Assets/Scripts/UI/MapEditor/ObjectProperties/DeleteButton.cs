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
using TT.InputMapping;
using TT.MapEditor;
using TT.State;
using TT.World;
using UnityEngine;

namespace TT.UI.MapEditor.ObjectProperties
{
    [RequireComponent(typeof(ToggledButton))]
    public class DeleteButton : MonoBehaviour
    {

        #region Lifecycle events
#pragma warning disable IDE0051 // Unused members

        void Start()
        {
            // Delete on button click
            GetComponent<ToggledButton>().OnClick += HandleButtonClicked;
        }

        void Update()
        {
            // Delete on key press
            if (InputMapper.Current.WorldObjectInput.Delete) HandleButtonClicked();
        }

#pragma warning restore IDE0051 // Unused members
        #endregion


        #region Event handlers

        /// <summary>
        /// Called when the delete button is clicked. Destroy the selected World Object.
        /// </summary>
        private void HandleButtonClicked()
        {
            if (WorldObjectBase.Current)
            {
                var objectType = WorldObjectBase.Current.Type;

                UndoController.RegisterAction(ActionType.Delete, Guid.Empty, WorldObjectBase.Current.ToDataObject());
                WorldObjectBase.Current.Destroy();

                StateController.CurrentState.ToIdle(objectType);
            }
        }

        #endregion
    }
}