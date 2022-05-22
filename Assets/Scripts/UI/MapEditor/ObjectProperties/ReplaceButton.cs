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

namespace TT.UI.MapEditor.ObjectProperties
{
    [RequireComponent(typeof(ToggledButton))]
    public class ReplaceButton : MonoBehaviour
    {

        #region Editor fields
#pragma warning disable IDE0044 // Make fields read-only

        [SerializeField][Tooltip("The panel containing the prefab replacement grid.")] private GameObject replacePanel;

#pragma warning restore IDE0044
        #endregion


        #region Private fields

        private ToggledButton _toggledButton;

        #endregion


        #region Lifecycle events
#pragma warning disable IDE0051 // Unused members

        // Start is called before the first frame update
        void Start()
        {
            _toggledButton = GetComponent<ToggledButton>();
            _toggledButton.OnClick += HandleButtonClick;
        }

#pragma warning restore IDE0051 // Unused members
        #endregion


        #region Event handlers

        /// <summary>
        /// Called when the button is clicked. Toggle the panel display.
        /// </summary>
        private void HandleButtonClick()
        {
            replacePanel.SetActive(!replacePanel.activeSelf);
            _toggledButton.Highlight = replacePanel.activeSelf;
        }

        #endregion
    }
}