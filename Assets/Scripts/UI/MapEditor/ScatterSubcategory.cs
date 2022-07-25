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

using TMPro;
using TT.Data;
using TT.Shared.GameContent;
using UnityEngine;
using UnityEngine.UI;

namespace TT.UI.MapEditor
{
    public class ScatterSubcategory : MonoBehaviour
    {

        #region Editor fields
#pragma warning disable IDE0044 // Make fields read-only

        [SerializeField][Tooltip("The text element in which to show the category name.")] private TMP_Text categoryName;
        [SerializeField][Tooltip("The toggle that determines whether to use this category.")] private Toggle toggle;

#pragma warning restore IDE0044
        #endregion


        #region Public properties

        private ContentItemCategory _category;
        /// <summary>
        /// The category of this toggle if toggled on, otherwise null.
        /// </summary>
        public ContentItemCategory Category => toggle.isOn ? _category : null;

        #endregion


        #region Public methods

        public void Initialise(ContentItemCategory category)
        {
            this._category = category;
            categoryName.text = category.Name;
        }

        #endregion
    }
}