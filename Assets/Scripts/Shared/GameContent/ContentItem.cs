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

using System.Diagnostics.CodeAnalysis;
using TT.Shared.World;
using UnityEngine.Experimental.GlobalIllumination;

namespace TT.Shared.GameContent
{
    
    public class ContentItem
    {
        /// <summary>
        /// List of IDs that this item covers.
        /// </summary>
        [SuppressMessage("ReSharper", "InconsistentNaming")] public string[] IDs = { };

        /// <summary>
        /// The default name for objects created from this content item.
        /// </summary>
        public string Name = string.Empty;

        /// <summary>
        /// True if player characters can traverse over this object, and item stacking ignores this item.
        /// </summary>
        public bool Traversable = false;

        /// <summary>
        /// A multiplier for the scale of this object. Applied to all axes. The user will see "1" for the result of this multiplication.
        /// </summary>
        public float Scale = 1f;

        /// <summary>
        /// A multiplier for the range and brightness of this item's light sources.
        /// </summary>
        public float Lights = 1f;

        /// <summary>
        /// The type of object this represents.
        /// </summary>
        public WorldObjectType Type = WorldObjectType.Item;
        
        /// <summary>
        /// The category this item belongs to.
        /// </summary>
        public ContentItemCategory Category;

        /// <summary>
        /// Creates another instance of content item with the same values as this instance, except for the specified
        /// parent category.
        /// </summary>
        /// <param name="parentCategory">The category object to parent this under.</param>
        /// <returns>A clone of this content item</returns>
        public ContentItem Clone(ContentItemCategory parentCategory)
        {
            return new ContentItem
            {
                IDs = IDs,
                Name = Name,
                Traversable = Traversable,
                Scale = Scale,
                Lights = Lights,
                Type = Type,
                Category = parentCategory
            };
        }
    }
}