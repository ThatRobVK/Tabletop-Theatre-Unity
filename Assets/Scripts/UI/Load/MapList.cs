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
using System.Threading.Tasks;
using DuloGames.UI;
using TT.Shared.UserContent;
using UnityEngine;
using UnityEngine.UI;

namespace TT.UI.Load
{
    /// <summary>
    /// Attached to a list of maps for loading.
    /// </summary>
    [RequireComponent(typeof(ScrollRect))]
    [RequireComponent(typeof(ToggleGroup))]
    public class MapList : MonoBehaviour
    {
        #region Editor fields

        [SerializeField] [Tooltip("The prefab for each item in the map list.")]
        private MapListItem mapItemPrefab;

        [SerializeField] [Tooltip("The details panel to show selected map.")]
        private MapListDetails mapListDetails;

        [SerializeField] [Tooltip("The button the user clicks to load a map.")]
        private ToggledButton loadMapMapButton;

        [SerializeField] [Tooltip("Toggle used to indicate the user is loading from this list. This can be null.")]
        private Toggle loadMapFromListToggle;

        [SerializeField] [Tooltip("A transform to parent all inactive list items.")]
        private Transform inactiveUIElements;

        #endregion


        #region Private fields

        private Transform _content;
        private List<MapListItem> _items = new List<MapListItem>();
        private ToggleGroup _toggleGroup;
        private UIWindow _window;

        #endregion


        #region Public properties

        /// <summary>
        /// The map that is currently selected in the list.
        /// </summary>
        public MapMetadata SelectedMap { get; private set; }

        #endregion


        #region Lifecycle events

        private void Start()
        {
            _content = GetComponent<ScrollRect>().content.transform;
            _toggleGroup = GetComponent<ToggleGroup>();
            _window = GetComponentInParent<UIWindow>();

            // Listen for window events to reload the list, and call it now to initialise
            _window.onTransitionBegin.AddListener(HandleWindowTransitionBegin);
            HandleWindowTransitionBegin(_window, UIWindow.VisualState.Shown, false);

            // Listen for map delete events
            mapListDetails.OnDelete += HandleMapDeleted;
        }

        private void OnDestroy()
        {
            // Stop listening for window events
            if (_window != null)
                _window.onTransitionBegin.RemoveListener(HandleWindowTransitionBegin);
            
            // Stop listening for map delete events
            if (mapListDetails != null)
                mapListDetails.OnDelete -= HandleMapDeleted;
        }

        #endregion


        #region Event handlers

        /// <summary>
        /// Called when the window starts showing or hiding. Repopulate the map list.
        /// </summary>
        /// <param name="window">The window that is transitioning.</param>
        /// <param name="targetState">The state it is transitioning to.</param>
        /// <param name="instant">Whether the transition is instant.</param>
        private async void HandleWindowTransitionBegin(UIWindow window, UIWindow.VisualState targetState, bool instant)
        {
            if (targetState == UIWindow.VisualState.Shown)
            {
                // On show, clear, repopulate, and force all toggles off
                ClearList();
                await PopulateListAsync();
                _toggleGroup.SetAllTogglesOff();
            }
            else
            {
                // Clear the list on hide, some nasty side effects of items staying selected otherwise
                ClearList();
            }
        }

        /// <summary>
        /// Called when one of the MapListItems is clicked. Pass the metadata to the details panel.
        /// </summary>
        /// <param name="metadata">The map metadata of the item that was clicked.</param>
        private void HandleButtonClicked(MapMetadata metadata)
        {
            if (_toggleGroup.AnyTogglesOn())
            {
                // Update to the specified metadata if any map is selected (toggled on)
                SelectedMap = metadata;
                mapListDetails.ShowMapDetails(metadata);
                loadMapMapButton.Enabled = true;
                if (loadMapFromListToggle != null)
                    loadMapFromListToggle.isOn = true;
            }
            else
            {
                // If nothing selected, clear the view
                SelectedMap = null;
                mapListDetails.ShowMapDetails(null);
                loadMapMapButton.Enabled = false;
            }
        }

        /// <summary>
        /// Called when a map is deleted. Reload the map list.
        /// </summary>
        private async void HandleMapDeleted()
        {
            loadMapMapButton.Enabled = false;
            ClearList();
            await PopulateListAsync();
        }

        #endregion


        #region Private methods

        /// <summary>
        /// Removes all items from the list by moving them to the inactive UI elements parent and deactivating them.
        /// </summary>
        private void ClearList()
        {
            if (_content == null)
                return;

            while (_content.childCount > 0)
            {
                var child = _content.GetChild(0);
                child.GetComponent<MapListItem>().OnClick -= HandleButtonClicked;
                child.gameObject.SetActive(false);
                child.SetParent(inactiveUIElements);
            }
        }

        /// <summary>
        /// Loads the map index and populates the list based on it.
        /// </summary>
        private async Task PopulateListAsync()
        {
            // Get and sort the maps - note the order of the CompareTo to sort descending
            var maps = await Helpers.Comms.UserContent.GetMapList();
            maps.Sort((x, y) => y.dateSaved.CompareTo(x.dateSaved));

            foreach (var map in maps)
            {
                var mapListItem = Helpers.GetAvailableButton(mapItemPrefab, _items, inactiveUIElements);
                mapListItem.Initialise(map, _toggleGroup);
                mapListItem.OnClick += HandleButtonClicked;

                mapListItem.transform.SetParent(_content);
                mapListItem.gameObject.SetActive(true);
            }
        }

        #endregion
    }
}