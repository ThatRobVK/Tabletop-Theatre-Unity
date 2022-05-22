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
using TT.Data;
using TT.State;
using TT.World;
using UnityEngine;
using UnityEngine.UI;

namespace TT.UI.MapEditor
{
    [RequireComponent(typeof(Toggle))]
    public class SelectedPanelToggle : MonoBehaviour
    {

        #region Editor fields
#pragma warning disable IDE0044 // Make fields read-only

        [SerializeField][Tooltip("A panel to hide when a WorldObject of the right type is selected.")] private GameObject[] hideOnSelect = new GameObject[] { };
        [SerializeField][Tooltip("A panel to show when a WorldObject of the right type is selected.")] private GameObject[] showOnSelect = new GameObject[] { };
        [SerializeField][Tooltip("The WorldObjectType to check the current WorldObject for.")] private WorldObjectType objectType;

#pragma warning restore IDE0044
        #endregion


        #region Private fields

        private Toggle _toggle;
        private UIWindow _window;
        private WorldObjectBase _selectedObject;

        #endregion


        #region Lifecycle events
#pragma warning disable IDE0051 // Unused members

        void Start()
        {
            // Handle toggle events and initialise
            _toggle = GetComponent<Toggle>();
            _toggle.onValueChanged.AddListener(HandleValueChanged);
            _window = GetComponentInParent<UIWindow>();
        }

        void Update()
        {
            // Don't update if the window isn't visible
            if (_window != null && !_window.IsVisible) return;

            // Only update when toggled on, and the selected object has changed
            if (_toggle.isOn && (WorldObjectBase.Current == null || WorldObjectBase.Current != _selectedObject))
            {
                var shouldShow = WorldObjectBase.Current && WorldObjectBase.Current.Type == objectType;

                foreach (var showOnSelectPanel in showOnSelect)
                {
                    showOnSelectPanel.SetActive(shouldShow);
                }

                foreach (var hideOnSelectPanel in hideOnSelect)
                {
                    hideOnSelectPanel.SetActive(!shouldShow);
                }

                _selectedObject = WorldObjectBase.Current;
            }
            else
            {
                // Toggle on when the right object is selected
                if (WorldObjectBase.Current != null && WorldObjectBase.Current.Type == objectType) _toggle.isOn = true;
            }
        }

#pragma warning restore IDE0051 // Unused members
        #endregion


        #region Event handlers

        /// <summary>
        /// Called when the toggle value changes. Toggle the connected canvas group.
        /// </summary>
        /// <param name="state">The new state of the toggle.</param>
        private void HandleValueChanged(bool state)
        {
            // If toggled on and wrong type is selected, deselect it
            if (state && WorldObjectBase.Current && WorldObjectBase.Current.Type != objectType) WorldObjectBase.Current.Deselect();

            // If a supported object is selected, deselect it - if it is placing then cancel that
            if (!state && WorldObjectBase.Current && WorldObjectBase.Current.Type == objectType)
            {
                if (StateController.CurrentState.IsPlacementState)
                {
                    StateController.CurrentState.ToIdle();
                }

                if (WorldObjectBase.Current)
                    WorldObjectBase.Current.Deselect();
            }
        }

        #endregion
    }
}