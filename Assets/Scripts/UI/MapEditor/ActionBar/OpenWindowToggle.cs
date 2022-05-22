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

using DuloGames.UI;
using UnityEngine;
using UnityEngine.UI;

namespace TT.UI.MapEditor.ActionBar
{
    [RequireComponent(typeof(Toggle))]
    public class OpenWindowToggle : MonoBehaviour
    {
        #region Editor fields
        #pragma warning disable IDE0044 // Make fields read-only

        [SerializeField] private UIWindow window;
        [SerializeField] private bool closeOnToggleOff;

        #pragma warning restore IDE0044
        #endregion


        #region Private fields

        private Toggle _toggle;

        #endregion


        #region Lifecycle events
        #pragma warning disable IDE0051 // Unused members
        
        /// <summary>
        /// Called when the scene loads. Initialise and start listening to events.
        /// </summary>
        void Start()
        {
            _toggle = GetComponent<Toggle>();

            if (window != null)
            {
                _toggle.isOn = window.IsVisible;
                _toggle.onValueChanged.AddListener(HandleToggleValueChanged);
                window.onTransitionComplete.AddListener(HandleWindowTransition);
            }
        }

        /// <summary>
        /// Called when the object is disabled. Stop listening for events.
        /// </summary>
        void OnDisable()
        {
            if (window != null)
            {
                window.onTransitionComplete.RemoveListener(HandleWindowTransition);
            }
        }

        #pragma warning restore IDE0051
        #endregion


        #region Event handlers

        /// <summary>
        /// Called when the toggle is changed. Show or hide the window.
        /// </summary>
        /// <param name="value"></param>
        private void HandleToggleValueChanged(bool value)
        {
            if (value && window != null && !window.IsVisible)
            {
                // Toggled on and window isn't visible - show it
                window.Show();
            }
            else if (closeOnToggleOff && !value && window != null && window.IsVisible)
            {
                // Toggled off and window is visible, hide the window if configured to
                window.Hide();
            }
        }


        /// <summary>
        /// Called when the window shows or hides. Update the toggle to match the new window state.
        /// </summary>
        /// <param name="changedWindow">The window that has changed state.</param>
        /// <param name="state">The new state.</param>
        private void HandleWindowTransition(UIWindow changedWindow, UIWindow.VisualState state)
        {
            _toggle.isOn = (state == UIWindow.VisualState.Shown);
        }

        #endregion
    }
}