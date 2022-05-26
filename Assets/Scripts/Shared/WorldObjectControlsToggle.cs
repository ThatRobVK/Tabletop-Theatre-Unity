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

namespace TT.Shared
{
    public class WorldObjectControlsToggle : MonoBehaviour
    {

        #region Editor fields
#pragma warning disable IDE0044 // Make fields read-only

        [SerializeField][Tooltip("The objects to show and hide.")] private GameObject[] controls = new GameObject[] { };

#pragma warning restore IDE0044
        #endregion


        #region Public methods

        /// <summary>
        /// Show the controls attached to this toggle.
        /// </summary>
        public void Show()
        {
            Toggle(true);
        }

        /// <summary>
        /// Hide the controls attached to this toggle.
        /// </summary>
        public void Hide()
        {
            Toggle(false);
        }

        /// <summary>
        /// Switch the visibility of the controls attached to this toggle.
        /// </summary>
        /// <remarks>All controls will be switched to the opposite state of the first control. This does not support a variety of visibility statuses across the controls.</remarks>
        public void Toggle()
        {
            if (controls.Length > 0)
            {
                Toggle(!controls[0].activeSelf);
            }
        }

        #endregion


        #region Private methods

        /// <summary>
        /// Show or hide the controls.
        /// </summary>
        /// <param name="show">A boolean indicating whether to show (true) or hide (false) the controls.</param>
        private void Toggle(bool show)
        {
            foreach (var control in controls)
            {
                control.gameObject.SetActive(show);
            }

        }

        #endregion
    }
}