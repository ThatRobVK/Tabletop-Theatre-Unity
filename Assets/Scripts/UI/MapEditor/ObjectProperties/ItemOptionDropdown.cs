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
using DuloGames.UI;
using TT.World;

namespace TT.UI.MapEditor.ObjectProperties
{
    public class ItemOptionDropdown : ItemOption
    {

        #region Editor fields

        [SerializeField][Tooltip("The dropdown to control")] private UISelectField selectField;

        #endregion


        #region Private fields

        private bool _updatedThisFrame;

        #endregion


        #region Lifecycle events

        void Awake()
        {
            selectField.onChange.AddListener(HandleChange);

            _updatedThisFrame = true;
        }

        void Update()
        {
            if (WorldObjectBase.Current != null)
            {
                _updatedThisFrame = true;
                selectField.SelectOptionByIndex(GetDropdownIndexFromWorldObject());
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

            if (WorldObjectBase.Current)
                WorldObjectBase.Current.SetOption(option, index);
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
                if (WorldObjectBase.Current.OptionValues.ContainsKey(option))
                {
                    return (int)WorldObjectBase.Current.OptionValues[option];
                }
            }

            return 0;
        }

        #endregion

    }
}