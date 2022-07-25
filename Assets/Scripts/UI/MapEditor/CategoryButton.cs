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
using TT.Data;
using TT.Shared.GameContent;
using UnityEngine;
using UnityEngine.UI;

namespace TT.UI.MapEditor
{
    public class CategoryButton : MonoBehaviour
    {
        #region Events

        /// <summary>
        /// Invoked when the button is clicked.
        /// </summary>
        public event Action<CategoryButton> OnClick;

        #endregion


        #region Public properties

        /// <summary>
        /// The content category this button relates to, when used as a top or sub category button.
        /// </summary>
        public ContentItemCategory Category { get; private set; }
        
        #endregion



        #region Event handlers

        /// <summary>
        /// Called when the underlying toggle is enabled. Call the click event.
        /// </summary>
        /// <param name="value">The new state of the toggle.</param>
        public void HandleToggle(bool value)
        {
            OnClick?.Invoke(this);
        }

        #endregion


        #region Public methods

        /// <summary>
        /// Initialises the button for a specified content category.
        /// </summary>
        /// <param name="category">The category to use this button for.</param>
        /// <param name="toggleGroup">The toggle group this forms part of.</param>
        /// <param name="parent">The transform to parent this button under.</param>
        public void Initialise(ContentItemCategory category, ToggleGroup toggleGroup, Transform parent)
        {
            Category = category;
            InitButton(category.Name, toggleGroup, parent);
        }


        #endregion


        #region Private methods

        /// <summary>
        /// Initialises the button.
        /// </summary>
        /// <param name="text">The text to display on the button.</param>
        /// <param name="toggleGroup">The toggle group this forms part of.</param>
        /// <param name="parent">The transform to parent this button under.</param>
        private void InitButton(string text, ToggleGroup toggleGroup, Transform parent)
        {
            GetComponentInChildren<Text>().text = text;
            GetComponent<Toggle>().group = toggleGroup;
            GetComponent<Toggle>().isOn = false;
            transform.SetParent(parent);
        }

        #endregion
    }
}