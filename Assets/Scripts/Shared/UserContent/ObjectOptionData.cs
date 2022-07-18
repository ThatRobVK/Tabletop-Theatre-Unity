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
using UnityEngine;

namespace TT.Shared.UserContent
{
    /// <summary>
    /// Data about a WorldObjectOption.
    /// </summary>
    [Serializable]
    public class ObjectOptionData
    {
        /// <summary>
        /// An integer representation of the WorldObjectOption enum value.
        /// </summary>
        public int option;
        
        /// <summary>
        /// A string representation of the value of this object. This is used for serializing the value regardless of
        /// its data type. When reading this information back, use ParsedValue instead.
        /// </summary>
        public string value;
        
        /// <summary>
        /// The data type of the value.
        /// </summary>
        public string valueType;
        
        /// <summary>
        /// The option value parsed to its correct type.
        /// </summary>
        public object ParsedValue
        {
            get
            {
                if (valueType.StartsWith("System."))
                {
                    return Convert.ChangeType(value, Type.GetType(valueType) ?? typeof(object));
                }

                if (valueType == "UnityEngine.Color")
                {
                    // Parse the value "RGBA(red, green, blue, alpha)" and return a color with its values
                    var colorElements = value.Replace("RGBA(", "").Replace(")", "").Replace(" ", "").Split(',');
                    return new Color(float.Parse(colorElements[0]), float.Parse(colorElements[1]), float.Parse(colorElements[2]), float.Parse(colorElements[3]));
                }

                if (valueType == "TT.World.LightsMode")
                {
                    if (value.Equals("On")) return 1;
                    if (value.Equals("Off")) return 2;
                    return 0;
                }

                Debug.LogFormat("MapObjectOption.ParsedValue: Unexpected ValueType [{0}]", valueType);
                return value;
            }
        }
    }
}