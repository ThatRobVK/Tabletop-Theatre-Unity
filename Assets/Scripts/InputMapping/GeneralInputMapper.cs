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

using UnityEngine;

namespace TT.InputMapping
{
    public class GeneralInputMapper : InputMapperBase
    {
        public GeneralInputMapper(InputMapperBase parent) : base(parent)
        { }

        // Either Control held down and S pressed, but Shift is not held down (that is Save As)
        public bool Save { get => IsActive() && IsControlDown() && Input.GetKeyDown(KeyCode.S) && !IsShiftDown(); }

        // Either Control held down, either Shift held down, and S pressed
        public bool SaveAs { get => IsActive() && IsControlDown() && Input.GetKeyDown(KeyCode.S) && IsShiftDown(); }

        // Either Control held down and O pressed
        public bool Load { get => IsActive() && IsControlDown() && Input.GetKeyDown(KeyCode.O); }

    }
}