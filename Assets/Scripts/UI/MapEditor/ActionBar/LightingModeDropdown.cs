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

using DuloGames.UI;
using TT.World;
using UnityEngine;

namespace TT.UI.MapEditor.ActionBar
{
    [RequireComponent(typeof(UISelectField))]
    public class LightingModeDropdown : MonoBehaviour
    {
        #region Private fields

        private UISelectField _selectField;
        private bool _updatedThisFrame;

        #endregion


        #region Lifecycle events
#pragma warning disable IDE0051 // Unused members

        void Awake()
        {
            _selectField = GetComponent<UISelectField>();
            _selectField.onChange.AddListener(HandleChange);
        }

        void Update()
        {
            if (_selectField.selectedOptionIndex != (int)TimeController.Current.LightingMode)
            {
                _updatedThisFrame = true;
                _selectField.SelectOptionByIndex((int)TimeController.Current.LightingMode);
            }
        }

        void LateUpdate()
        {
            _updatedThisFrame = false;
        }

#pragma warning restore IDE0051 // Unused members
        #endregion


        #region Event handlers

        /// <summary>
        /// Called when the dropdown selection is changed. Updates the TimeController with the selected lighting mode.
        /// </summary>
        /// <param name="index">The index that was selected.</param>
        /// <param name="value">The text that was selected.</param>
        private void HandleChange(int index, string value)
        {
            if (_updatedThisFrame) return;

            TimeController.Current.LightingMode = (LightingMode)index;
        }

        #endregion


    }
}