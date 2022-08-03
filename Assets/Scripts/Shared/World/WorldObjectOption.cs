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

namespace TT.Shared.World
{
    /// <summary>
    /// An enum detailing the various types of options that a world object can have.
    /// </summary>
    public enum WorldObjectOption
    {
        Terrain = 0,
        Detailing = 1,
        Roof = 2,
        LightsMode = 3,
        LightsColor = 4,
        LightsRange = 5,
        LightsIntensity = 6,
        SplineWidth = 7,
        RiverFlowSpeed = 8,
        OpenClose = 9
    }
}