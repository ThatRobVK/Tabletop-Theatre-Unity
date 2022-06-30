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
using TT.Shared.UserContent;
using UnityEngine;
using UnityEngine.UI;

namespace TT.UI.Load
{
    [RequireComponent(typeof(ScrollRect))]
    public class MapList : MonoBehaviour
    {
        
        #region Editor fields
        
        [SerializeField][Tooltip("The prefab for each item in the map list.")] private MapListItem mapItemPrefab;
        [SerializeField][Tooltip("The details panel to show selected map.")] private MapListDetails mapListDetails;
        [SerializeField][Tooltip("The button the user clicks to load a map.")] private ToggledButton loadMapMapButton;
        [SerializeField][Tooltip("Toggle used to indicate the user is loading from this list. This can be null.")] private Toggle loadMapFromListToggle;
        [SerializeField][Tooltip("A transform to parent all inactive list items.")] private Transform inactiveUIElements;
        
        #endregion
        
        
        #region Private fields
        
        private Transform _content;
        private List<MapListItem> _items = new List<MapListItem>();
        
        #endregion
        
        
        #region Public properties
        
        public MapMetadata SelectedMap { get; private set; }
        
        #endregion
        
        
        #region Lifecycle events
        
        // Start is called before the first frame update
        private void Start()
        {
            _content = GetComponent<ScrollRect>().content.transform;

            // Listen for auth events
            Helpers.Comms.User.OnLoginSuccess += HandleLoginSuccess;
            Helpers.Comms.User.OnLogout += HandleLogout;
            
            // If already logged in, populate the list
            if (Helpers.Comms.User.IsLoggedIn) HandleLoginSuccess();
        }

        private void OnDestroy()
        {
            if (Helpers.Comms != null && Helpers.Comms.User != null)
            {
                // Stop listening for auth events
                Helpers.Comms.User.OnLoginSuccess += HandleLoginSuccess;
                Helpers.Comms.User.OnLogout += HandleLogout;
            }
        }
        
        #endregion
        
        
        #region Event handlers

        /// <summary>
        /// Called when the user successfully logs in. Re-populate the list for this user.
        /// </summary>
        private void HandleLoginSuccess()
        {
            ClearList();
            PopulateListAsync();
        }

        /// <summary>
        /// Called when the user is logged out. Clear the list.
        /// </summary>
        private void HandleLogout()
        {
            ClearList();
        }
        
        /// <summary>
        /// Called when one of the MapListItems is clicked. Pass the metadata to the details panel.
        /// </summary>
        /// <param name="metadata">The map metadata of the item that was clicked.</param>
        private void HandleButtonClicked(MapMetadata metadata)
        {
            SelectedMap = metadata;
            mapListDetails.ShowMapDetails(metadata);
            loadMapMapButton.Enabled = true;
            if (loadMapFromListToggle != null)
                loadMapFromListToggle.isOn = true;
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
        private async void PopulateListAsync()
        {
            // Get and sort the maps - note the order of the CompareTo to sort descending
            var maps = await Helpers.Comms.UserContent.GetMapList();
            maps.Sort((x, y) => y.dateSaved.CompareTo(x.dateSaved));

            foreach (var map in maps)
            {
                var mapListItem = Helpers.GetAvailableButton(mapItemPrefab, _items, inactiveUIElements);
                mapListItem.Initialise(map);
                mapListItem.OnClick += HandleButtonClicked;
                mapListItem.transform.SetParent(_content);
            }
        }

        #endregion
    }
}
