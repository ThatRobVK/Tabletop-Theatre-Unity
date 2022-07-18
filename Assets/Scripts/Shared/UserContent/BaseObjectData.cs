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
    /// Base class for metadata about objects. Used when saving and loading.
    /// </summary>
    [Serializable]
    public class BaseObjectData
    {
        /// <summary>
        /// A unique ID for this object.
        /// </summary>
        public string objectId;

        /// <summary>
        /// The name given to the object by the user.
        /// </summary>
        public string name;
        
        /// <summary>
        /// The position on the map.
        /// </summary>
        public VectorData position;
        
        /// <summary>
        /// The orientation of the object. 
        /// </summary>
        public VectorData rotation;
        
        /// <summary>
        /// The scaling of the object along each axis.
        /// </summary>
        public VectorData scale;
        
        /// <summary>
        /// The addressables address of the prefab used to create the object.
        /// </summary>
        public string prefabAddress;
        
        /// <summary>
        /// The Layer the object is spawned into.
        /// </summary>
        public string gameLayer;
        
        /// <summary>
        /// A list of options applied to the object. The contents depends on the type of object.
        /// </summary>
        public List<ObjectOptionData> options = new List<ObjectOptionData>();
        
        /// <summary>
        /// Whether the user has starred this object.
        /// </summary>
        public bool starred;
        
        /// <summary>
        /// A multiplier applied to the scale of the object. This is used to ensure the base scale is correct compared
        /// to other objects.
        /// </summary>
        public float scaleMultiplier;
        
        /// <summary>
        /// An integer representing the WorldObjectType enum of this object.
        /// </summary>
        public int objectType;
    }
}