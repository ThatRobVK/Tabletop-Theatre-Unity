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

namespace TT.InputMapping
{
    public class InputMapper : InputMapperBase
    {
        private static InputMapper _current;
        public static InputMapper Current
        {
            get
            {
                if (_current == null) _current = new InputMapper();
                return _current;
            }
        }


        private InputMapper() : base(null)
        {
        }

        private WorldObjectInputMapper _worldObjectInput;
        public WorldObjectInputMapper WorldObjectInput
        {
            get
            {
                if (_worldObjectInput == null) _worldObjectInput = new WorldObjectInputMapper(this);
                return _worldObjectInput;
            }
        }

        private CameraInputMapper _cameraInput;
        public CameraInputMapper CameraInput
        {
            get
            {
                if (_cameraInput == null) _cameraInput = new CameraInputMapper(this);
                return _cameraInput;
            }
        }

        private GeneralInputMapper _generalInput;
        public GeneralInputMapper GeneralInput
        {
            get
            {
                if (_generalInput == null) _generalInput = new GeneralInputMapper(this);
                return _generalInput;
            }
        }
    }
}