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

using TT.CameraControllers;
using TT.InputMapping;
using UnityEngine;
using UnityEngine.UI;

namespace TT.UI.MapEditor.ActionBar
{
    [RequireComponent(typeof(Toggle))]
    public class CameraModeButton : MonoBehaviour
    {

        #region Editor fields
#pragma warning disable IDE0044 // Make fields read-only

        [SerializeField] private CameraController cameraController;

#pragma warning restore IDE0044
        #endregion


        #region Private fields

        private Toggle _toggle;

        #endregion


        #region Lifecycle events
#pragma warning disable IDE0051 // Unused members

        void Start()
        {
            _toggle = GetComponent<Toggle>();
            _toggle.onValueChanged.AddListener(HandleValueChanged);
        }

        void Update()
        {
            if (_toggle.isOn != cameraController.TopDown)
            {
                _toggle.onValueChanged.RemoveListener(HandleValueChanged);
                _toggle.isOn = cameraController.TopDown;
                _toggle.onValueChanged.AddListener(HandleValueChanged);
            }

            // If the user has pressed the hotkey, toggle topdown mode
            if (InputMapper.Current.CameraInput.TopDownToggle) cameraController.TopDown = !cameraController.TopDown;
        }

#pragma warning restore IDE0051 // Unused members
        #endregion


        #region Event handlers

        private void HandleValueChanged(bool value)
        {
            cameraController.TopDown = value;
        }

        #endregion
    }
}