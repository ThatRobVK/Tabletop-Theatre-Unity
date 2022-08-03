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
using System.Collections.Generic;

namespace TT.Shared.UserContent
{
    /// <summary>
    /// Metadata about a map.
    /// </summary>
    [Serializable]
    public class MapMetadata
    {
        /// <summary>
        /// A unique identifier for the map.
        /// </summary>
        public string id;
        
        /// <summary>
        /// A user-defined name for the map.
        /// </summary>
        public string name;

        /// <summary>
        /// A user-defined description for the map.
        /// </summary>
        public string description;
        
        /// <summary>
        /// The unique identifier of the user who first created this map.
        /// </summary>
        public string authorId;
        
        /// <summary>
        /// The username of the user who first created this map.
        /// </summary>
        public string authorName;
        
        /// <summary>
        /// The unique identifier of the user who last modified this map.
        /// </summary>
        public string modifiedById;
        
        /// <summary>
        /// The username of the user who last modified this map.
        /// </summary>
        public string modifiedByName;
        
        /// <summary>
        /// The date and time on which this map was first created.
        /// </summary>
        public long dateCreated;
        
        /// <summary>
        /// The date and time on which this map was last saved.
        /// </summary>
        public long dateSaved;
        
        /// <summary>
        /// A list of content pack names that were loaded for this map.
        /// </summary>
        public List<string> contentPacks = new List<string>();
        
    }
}