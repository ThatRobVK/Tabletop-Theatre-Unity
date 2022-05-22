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

using TT.InputMapping;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TT.UI
{
    public class Textbox : InputField
    {
        #region Event handlers

        /// <summary>
        /// Called when the InputField is selected.
        /// </summary>
        /// <param name="eventData">Data about the event.</param>
        public override void OnSelect(BaseEventData eventData)
        {
            IsSelected = true;

            // Disable game input
            InputMapper.Current.SetActive(false);
            base.OnSelect(eventData);
        }

        /// <summary>
        /// Called when the InputField is deselected.
        /// </summary>
        /// <param name="eventData">Data about the event.</param>
        public override void OnDeselect(BaseEventData eventData)
        {
            IsSelected = false;
            
            // Enable game input
            InputMapper.Current.SetActive(true);
            base.OnDeselect(eventData);
        }

        #endregion

        #region Public properties

        /// <summary>
        /// True if selected, i.e. the user is editing this. False otherwise.
        /// </summary>
        public bool IsSelected { get; private set; }

        #endregion

    }
}