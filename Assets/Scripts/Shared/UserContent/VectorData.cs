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
using System.Linq;
using UnityEngine;

namespace TT.Shared.UserContent
{
    /// <summary>
    /// A class used to serialize Vector3 and Vector4 objects, working around some serialization issues with Unity
    /// built-in objects (self reference via the normalized field).
    /// </summary>
    [Serializable]
    public class VectorData
    {
        
        /// <summary>
        /// The X axis for this vector.
        /// </summary>
        public float x;
        
        /// <summary>
        /// The Y axis for this vector.
        /// </summary>
        public float y;
        
        /// <summary>
        /// The Z axis for this vector.
        /// </summary>
        public float z;

        /// <summary>
        /// The W axis for this vector.
        /// </summary>
        public float w;

        #region Constructors
        
        /// <summary>
        /// Default constructor for deserialization.
        /// </summary>
        public VectorData()
        {}
        
        /// <summary>
        /// Creates a new instance with the passed in values.
        /// </summary>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The Y coordinate.</param>
        /// <param name="z">The Z coordinate.</param>
        /// <param name="w">The W coordinate.</param>
        public VectorData(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        /// <summary>
        /// Creates a new instance with values based on the passed in Vector4.
        /// </summary>
        /// <param name="vector">The vector to base the Vector4Data instance on.</param>
        public VectorData(Vector4 vector)
        {
            x = vector.x;
            y = vector.y;
            z = vector.z;
            w = vector.w;
        }

        /// <summary>
        /// Creates a new instance with values based on the passed in Vector4.
        /// </summary>
        /// <param name="vector">The vector to base the Vector4Data instance on.</param>
        public VectorData(Vector3 vector)
        {
            x = vector.x;
            y = vector.y;
            z = vector.z;
            w = 0;
        }
        
        #endregion

        
        #region Public methods

        /// <summary>
        /// Converts this object to a Unity Vector3.
        /// </summary>
        /// <returns>A Vector3 with the same values as this data object.</returns>
        public Vector3 ToVector3()
        {
            return new Vector3(x, y, z);
        }
        
        /// <summary>
        /// Converts this object to a Unity Vector4.
        /// </summary>
        /// <returns>A Vector4 with the same values as this data object.</returns>
        public Vector4 ToVector4()
        {
            return new Vector4(x, y, z, w);
        }

        /// <summary>
        /// Converts a list of Vector4 objects to a list of data objects.
        /// </summary>
        /// <param name="list">The list of Vectors to convert.</param>
        /// <returns>A list of data objects with the same values as the passed in vectors.</returns>
        public static List<VectorData> FromVector4List(List<Vector4> list)
        {
            return list.ConvertAll(x => new VectorData(x)).ToList();
        }

        /// <summary>
        /// Converts a list of data objects to a list of Unity Vector4 objects.
        /// </summary>
        /// <param name="list">The list of data objects to convert.</param>
        /// <returns>A list of Vector4 objects with the same values as the passed in data objects.</returns>
        public static List<Vector4> ToVector4List(List<VectorData> list)
        {
            return list.ConvertAll(x => x.ToVector4()).ToList();
        }

        /// <summary>
        /// Converts a list of Vector4 objects to a list of data objects.
        /// </summary>
        /// <param name="list">The list of Vectors to convert.</param>
        /// <returns>A list of data objects with the same values as the passed in vectors.</returns>
        public static List<VectorData> FromVector3List(List<Vector3> list)
        {
            return list.ConvertAll(x => new VectorData(x)).ToList();
        }

        /// <summary>
        /// Converts a list of data objects to a list of Unity Vector4 objects.
        /// </summary>
        /// <param name="list">The list of data objects to convert.</param>
        /// <returns>A list of Vector4 objects with the same values as the passed in data objects.</returns>
        public static List<Vector3> ToVector3List(List<VectorData> list)
        {
            return list.ConvertAll(x => x.ToVector3()).ToList();
        }
        
        #endregion
        
    }
}