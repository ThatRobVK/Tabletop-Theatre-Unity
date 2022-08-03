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
using UnityEngine;
using DuloGames.UI;
using TT.MapEditor;
using TT.Shared.World;
using TT.World;

namespace TT.UI.MapEditor.ObjectProperties
{
    [RequireComponent(typeof(UISelectField))]
    public class LightsDropdown : ItemOption
    {

        #region Private fields

        private UISelectField _selectField;
        private bool _updatedThisFrame;
        private int _undoValue;

        #endregion


        #region Lifecycle events

        void Awake()
        {
            _selectField = GetComponent<UISelectField>();
            _selectField.onChange.AddListener(HandleChange);

            _updatedThisFrame = true;
            _selectField.SelectOptionByIndex(GetDropdownIndexFromWorldObject());
            _undoValue = _selectField.selectedOptionIndex;
        }

        void Update()
        {
            if (WorldObjectBase.Current != null)
            {
                _updatedThisFrame = true;
                _selectField.SelectOptionByIndex(GetDropdownIndexFromWorldObject());
            }
        }

        void LateUpdate()
        {
            _updatedThisFrame = false;
        }

        #endregion


        #region Event handlers

        /// <summary>
        /// Called when the dropdown selection is changed. Updates the lights on the selected WorldObject.
        /// </summary>
        /// <param name="index">The index that was selected.</param>
        /// <param name="value">The text that was selected.</param>
        private void HandleChange(int index, string value)
        {
            if (_updatedThisFrame) return;

            if (index != _undoValue)
            {
                UndoController.RegisterAction(ActionType.Option, WorldObjectBase.Current.ObjectId, new KeyValuePair<WorldObjectOption, object>(WorldObjectOption.LightsMode, WorldObjectBase.Current.OptionValues[WorldObjectOption.LightsMode]));
            }
            
            if (WorldObjectBase.Current)
                WorldObjectBase.Current.SetOption(WorldObjectOption.LightsMode, (LightsMode)index);
        }

        #endregion


        #region Private methods

        /// <summary>
        /// Returns a dropdown index for the current WorldObject's lighting options
        /// </summary>
        /// <returns></returns>
        private int GetDropdownIndexFromWorldObject()
        {
            if (WorldObjectBase.Current != null)
            {
                if (WorldObjectBase.Current.OptionValues.ContainsKey(WorldObjectOption.LightsMode))
                {
                    return (int)WorldObjectBase.Current.OptionValues[WorldObjectOption.LightsMode];
                }
            }

            return (int)LightsMode.Auto;
        }

        #endregion

    }
}
