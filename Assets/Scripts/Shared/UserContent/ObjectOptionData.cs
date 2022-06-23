using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TT.Shared.UserContent
{
    [Serializable]
    public class ObjectOptionData
    {
        public int option;
        public string value;
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
                    return Convert.ChangeType(value, Type.GetType(valueType));
                }

                if (valueType == "UnityEngine.Color")
                {
                    // Parse the value "RGBA(red, green, blue, alpha)" and return a color with its values
                    var colorElements = value.Replace("RGBA(", "").Replace(")", "").Replace(" ", "").Split(',');
                    return new Color(float.Parse(colorElements[0]), float.Parse(colorElements[1]), float.Parse(colorElements[2]), float.Parse(colorElements[3]));
                }

                if (valueType == "MOD.World.LightsMode")
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