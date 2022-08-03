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

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TT.MapEditor;
using TT.Shared.World;
using TT.World;

namespace TT.UI.MapEditor.ObjectProperties
{
    public class ItemOptionBool : MonoBehaviour
    {
        #region Editor fields

        [SerializeField][Tooltip("The Text field to display the heading text.")] private TMP_Text headerText;
        [SerializeField][Tooltip("The toggle to control via this option.")] private Toggle toggle;

        #endregion


        #region Private fields

        private WorldObjectOption _option;
        private bool _updatedThisFrame;

        #endregion


        #region Lifecycle events

        void Start()
        {
            toggle.onValueChanged.AddListener(UpdateFromToggle);
        }

        void Update()
        {
            if (!_updatedThisFrame && WorldObjectBase.Current != null && WorldObjectBase.Current.OptionValues.ContainsKey(_option))
            {
                UpdateFromWorldObject((bool)WorldObjectBase.Current.OptionValues[_option]);
            }
        }

        void LateUpdate()
        {
            _updatedThisFrame = false;
        }

        #endregion


        #region Public methods

        /// <summary>
        /// Sets up the toggle for the specified option.
        /// </summary>
        /// <param name="option">The option to use this toggle for.</param>
        /// <param name="header">The text to display in the label.</param>
        public void Initialise(WorldObjectOption option, string header)
        {
            _option = option;
            headerText.text = header;
        }

        #endregion


        #region Private methods

        /// <summary>
        /// Updates the display based on the selected WorldObject.
        /// </summary>
        /// <param name="worldObjectValue">The euler angle of the WorldObject.</param>
        private void UpdateFromWorldObject(bool worldObjectValue)
        {
            // Do not update if another upate has run this frame
            if (_updatedThisFrame) return;

            if (worldObjectValue != toggle.isOn)
            {
                _updatedThisFrame = true;
                toggle.isOn = worldObjectValue;
            }           
        }

        /// <summary>
        /// Updates the WorldObject and input based on the slider position.
        /// </summary>
        /// <param name="toggleValue">The slider's Position.</param>
        private void UpdateFromToggle(bool toggleValue)
        {
            // Do not update if another upate has run this frame
            if (_updatedThisFrame) return;

            if (WorldObjectBase.Current)
            {
                UndoController.RegisterAction(ActionType.Option, WorldObjectBase.Current.ObjectId, new KeyValuePair<WorldObjectOption, object>(_option, WorldObjectBase.Current.OptionValues[_option]));
                _updatedThisFrame = true;
                WorldObjectBase.Current.SetOption(_option, toggleValue);
            }
        }

        #endregion

    }
}