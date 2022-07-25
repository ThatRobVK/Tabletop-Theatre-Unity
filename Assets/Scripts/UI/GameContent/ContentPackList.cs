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
using System.Linq;
using DuloGames.UI;
using TMPro;
using TT.Data;
using TT.Shared.GameContent;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace TT.UI.GameContent
{
    /// <summary>
    /// Attached to a list of maps. Populates the list when the parent window opens.
    /// </summary>
    [RequireComponent(typeof(ScrollRect))]
    public class ContentPackList : MonoBehaviour
    {
        #region Editor fields

        [SerializeField][Tooltip("The prefab for each item in the map list.")]
        private ContentPackListItem contentPackItemPrefab;

        [SerializeField][Tooltip("The label for the total download size.")][FormerlySerializedAs("totalDownloadSizeText")]
        private TMP_Text totalDownloadSizeLabel;

        [SerializeField][Tooltip("The text to show when there is nothing to download.")]
        private string nothingToDownloadText = "nothing";

        [SerializeField][Tooltip("The panel to show whilst loading content.")]
        private GameObject waitPanel;

        [SerializeField][Tooltip("A transform to parent all inactive list items.")]
        private Transform inactiveUIElements;

        #endregion


        #region Private fields

        private Transform _content;
        private readonly List<ContentPackListItem> _items = new List<ContentPackListItem>();
        private UIWindow _window;
        private long _totalDownloadSize = 0;

        #endregion


        #region Lifecycle events

        private void Start()
        {
            _content = GetComponent<ScrollRect>().content.transform;
            _window = GetComponentInParent<UIWindow>();

            // Listen for window events to reload the list, and call it now to initialise
            _window.onTransitionBegin.AddListener(HandleWindowTransitionBegin);
        }

        private void OnDestroy()
        {
            // Stop listening for window events
            if (_window != null)
                _window.onTransitionBegin.RemoveListener(HandleWindowTransitionBegin);
        }

        #endregion


        #region Event handlers

        /// <summary>
        /// Called when the window starts showing or hiding. Repopulate the map list.
        /// </summary>
        /// <param name="window">The window that is transitioning.</param>
        /// <param name="targetState">The state it is transitioning to.</param>
        /// <param name="instant">Whether the transition is instant.</param>
        private void HandleWindowTransitionBegin(UIWindow window, UIWindow.VisualState targetState, bool instant)
        {
            if (targetState == UIWindow.VisualState.Shown)
            {
                // On show, clear, repopulate, and force all toggles off
                ClearList();
                PopulateList();
            }
            else
            {
                // Clear the list on hide, some nasty side effects of items staying selected otherwise
                ClearList();
            }
        }

        /// <summary>
        /// Called when an item in the list is unticked. Reduce the total download size.
        /// </summary>
        /// <param name="contentPack">The content pack that was unticked.</param>
        /// <param name="downloadSize">The size of the content pack.</param>
        private void HandlePackUnticked(ContentPack contentPack, long downloadSize)
        {
            _totalDownloadSize -= downloadSize;
            totalDownloadSizeLabel.text = Helpers.FormatFileSizeString(_totalDownloadSize) ?? nothingToDownloadText;
        }

        /// <summary>
        /// Called when an item in the list is ticked. Increase the total download size.
        /// </summary>
        /// <param name="contentPack">The content pack that was ticked.</param>
        /// <param name="downloadSize">The size of the content pack.</param>
        private void HandlePackTicked(ContentPack contentPack, long downloadSize)
        {
            _totalDownloadSize += downloadSize;
            totalDownloadSizeLabel.text = Helpers.FormatFileSizeString(_totalDownloadSize) ?? nothingToDownloadText;
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
                child.GetComponent<ContentPackListItem>().OnTick -= HandlePackTicked;
                child.GetComponent<ContentPackListItem>().OnUntick -= HandlePackUnticked;
                child.gameObject.SetActive(false);
                child.SetParent(inactiveUIElements);
            }
        }

        /// <summary>
        /// Loads the map index and populates the list based on it.
        /// </summary>
        private async void PopulateList()
        {
            if (!Content.ContentLoaded)
            {
                if (waitPanel != null) waitPanel.SetActive(true);
                await Content.Load();
                if (waitPanel != null) waitPanel.SetActive(false);
            }

            // Get and sort the maps - note the order of the CompareTo to sort descending
            var packs = Content.Current.Packs.ToList();
            packs.Sort((x, y) => x.SortOrder.CompareTo(y.SortOrder));

            foreach (var pack in packs)
            {
                var contentPackListItem = Helpers.GetAvailableButton(contentPackItemPrefab, _items, inactiveUIElements);
                contentPackListItem.OnTick += HandlePackTicked;
                contentPackListItem.OnUntick += HandlePackUnticked;
                contentPackListItem.Initialise(pack, !pack.Name.Equals("Default"));

                contentPackListItem.transform.SetParent(_content);
                contentPackListItem.gameObject.SetActive(true);
            }
        }
        
        #endregion
    }
}