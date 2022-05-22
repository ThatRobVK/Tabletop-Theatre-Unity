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

namespace TT.Data
{
    
    public class ContentItemCategory
    {
        /// <summary>
        /// A display name for the category.
        /// </summary>
        public string Name = string.Empty;

        public float MaxDensity = 20f;
        public float MinDensity = 0.01f;

        /// <summary>
        /// The items in this category.
        /// </summary>
        public ContentItem[] Items = new ContentItem[] { };

        /// <summary>
        /// Sub-categories under this category.
        /// </summary>
        public ContentItemCategory[] Categories = new ContentItemCategory[] { };
    }
}