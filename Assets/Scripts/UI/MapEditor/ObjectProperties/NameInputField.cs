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

using TT.MapEditor;
using TT.World;
using UnityEngine;
using UnityEngine.UI;

namespace TT.UI.MapEditor.ObjectProperties
{
    [RequireComponent(typeof(InputField))]
    public class NameInputField : MonoBehaviour
    {
        #region Private fields

        private InputField _inputField;

        #endregion


        #region Lifecycle events
#pragma warning disable IDE0051 // Unused members

        void Start()
        {
            _inputField = GetComponent<InputField>();
            _inputField.onEndEdit.AddListener(HandleEndEdit);
        }

        void Update()
        {
            if (WorldObjectBase.Current != null && !_inputField.isFocused)
            {
                _inputField.text = WorldObjectBase.Current.name;
            }
        }

#pragma warning restore IDE0051 // Unused members
        #endregion


        #region Event handlers

        /// <summary>
        /// Called when the InputField has been edited.
        /// </summary>
        /// <param name="newValue">The new value of the InputField.</param>
        private void HandleEndEdit(string newValue)
        {
            if (WorldObjectBase.Current != null)
            {
                UndoController.RegisterAction(ActionType.Name, WorldObjectBase.Current.ObjectId, WorldObjectBase.Current.name);
                WorldObjectBase.Current.name = newValue;
            }
        }

        #endregion
    }
}