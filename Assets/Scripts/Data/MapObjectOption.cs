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
using TT.Shared.UserContent;
using TT.World;
using UnityEngine;
using UnityEngine.Serialization;

namespace TT.Data
{
    [Serializable]
    public class MapObjectOption
    {
        [FormerlySerializedAs("Option")] public WorldObjectOption option;
        [FormerlySerializedAs("Value")] public string value;
        [FormerlySerializedAs("ValueType")] public string valueType;

        public MapObjectOption()
        { }

        public MapObjectOption(WorldObjectOption option, object value)
        {
            this.option = option;

            valueType = value.GetType().ToString();
            this.value = value.ToString();
        }

        // Returns the value as an object from the typed fields
        public object ParsedValue
        {
            get
            {
                if (valueType.StartsWith("System."))
                {
                    return Convert.ChangeType(value, Type.GetType(valueType));
                }
                else if (valueType == "UnityEngine.Color")
                {
                    // Parse the value "RGBA(red, green, blue, alpha)" and return a color with its values
                    var colorElements = value.Replace("RGBA(", "").Replace(")", "").Replace(" ", "").Split(',');
                    return new Color(float.Parse(colorElements[0]), float.Parse(colorElements[1]), float.Parse(colorElements[2]), float.Parse(colorElements[3]));
                }
                else if (valueType == "MOD.World.LightsMode")
                {
                    if (value.Equals("On")) return LightsMode.On;
                    if (value.Equals("Off")) return LightsMode.Off;
                    return LightsMode.Auto;
                }
                else
                {
                    Debug.LogFormat("MapObjectOption.ParsedValue: Unexpected ValueType [{0}]", valueType);
                    return value;
                }
            }
        }

        // Converts an internal Dictionary to List for serialization
        public static List<ObjectOptionData> FromDictionary(Dictionary<WorldObjectOption, object> dictionary)
        {
            if (dictionary == null) return null;

            return dictionary.Select(x => new ObjectOptionData() { option = (int)x.Key, value = x.Value.ToString(), valueType = x.Value.GetType().FullName }).ToList();
        }

        // Converts a List from serialization to an internal Dictionary
        public static Dictionary<WorldObjectOption, object> ToDictionary(List<ObjectOptionData> options)
        {
            if (options == null) return null;

            return options.ToDictionary(x => x.option, x => x.ParsedValue);
        }
    }
}