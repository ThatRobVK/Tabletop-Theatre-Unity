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

using TT.Data;
using TT.World;
using UnityEngine;

namespace TT.UI.MapEditor.ObjectProperties
{
    public class ItemOptionLights : ItemOption
    {

        #region Editor fields
#pragma warning disable IDE0044 // Make fields read-only

        [SerializeField][Tooltip("The image displaying the colour preview.")] private ItemOptionColor colorOption;

#pragma warning restore IDE0044
        #endregion

        #region Public methods

        public void Initialise(WorldObjectOption option, string header, object value)
        {
            if (WorldObjectBase.Current != null)
            {
                var color = WorldObjectBase.Current.OptionValues.ContainsKey(WorldObjectOption.LightsColor) ? 
                                    WorldObjectBase.Current.OptionValues[WorldObjectOption.LightsColor] : 
                                    Color.white;
                if (colorOption != null) colorOption.Initialise(WorldObjectOption.LightsColor, color);

                base.Initialise(option, header);
            }
        }

        #endregion
    }
}