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

namespace TT.Shared.GameContent
{
    /// <summary>
    /// A container for content items or sub content categories.
    /// </summary>
    public class ContentItemCategory
    {
        /// <summary>
        /// A display name for the category.
        /// </summary>
        public string Name = string.Empty;

        /// <summary>
        /// If this can be scattered, the maximum density for the scatter positions.
        /// </summary>
        public float MaxDensity = 20f;
        
        /// <summary>
        /// If this can be scattered, the minimum density for the scatter positions.
        /// </summary>
        public float MinDensity = 0.01f;

        /// <summary>
        /// The items in this category.
        /// </summary>
        public ContentItem[] Items = { };

        /// <summary>
        /// Sub-categories under this category.
        /// </summary>
        public ContentItemCategory[] Categories = { };

        
        #region Public methods
        
        /// <summary>
        /// Creates another instance of content item category with the same values as this instance. All subcategories
        /// and items will be cloned as well.
        /// </summary>
        /// <returns>A clone of this content item category.</returns>
        public ContentItemCategory Clone()
        {
            // Init a clone object
            var clone = new ContentItemCategory
            {
                Name = Name,
                MaxDensity = MaxDensity,
                MinDensity = MinDensity
            };

            // Add clones of all the sub-categories
            List<ContentItemCategory> cloneCategories = new List<ContentItemCategory>();
            foreach (var category in Categories)
            {
                cloneCategories.Add(category.Clone());
            }
            clone.Categories = cloneCategories.ToArray();

            // Add clones of all the items
            List<ContentItem> cloneItems = new List<ContentItem>();
            foreach (var item in Items)
            {
                cloneItems.Add(item.Clone(clone));
            }
            clone.Items = cloneItems.ToArray();

            return clone;
        }
        
        #endregion
        
    }
}