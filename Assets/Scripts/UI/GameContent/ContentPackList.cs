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
using System.Threading.Tasks;
using DuloGames.UI;
using TMPro;
using TT.Data;
using TT.World;
using UnityEngine;
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

        [SerializeField][Tooltip("The label for the total download size.")]
        private TMP_Text totalDownloadSizeLabel;

        [SerializeField][Tooltip("The text to show when there is nothing to download.")]
        private string nothingToDownloadText = "nothing";

        [SerializeField][Tooltip("The panel to show whilst loading content.")]
        private GameObject waitPanel;

        [SerializeField][Tooltip("A transform to parent all inactive list items.")]
        private Transform inactiveUIElements;

        [SerializeField][Tooltip("When clicked, changes are applied.")]
        private ToggledButton applyButton;
        
        [SerializeField][Tooltip("When clicked, changes are reverted.")]
        private ToggledButton undoButton;

        #endregion


        #region Private fields

        private Transform _content;
        private readonly List<ContentPackListItem> _items = new List<ContentPackListItem>();
        private UIWindow _window;
        private Dictionary<string, int> _contentPackUsages = new Dictionary<string, int>();
        private UIModalBox _packInUseModal;

        #endregion


        #region Lifecycle events

        private void Start()
        {
            _content = GetComponent<ScrollRect>().content.transform;
            _window = GetComponentInParent<UIWindow>();

            // Listen for window events to reload the list, and call it now to initialise
            _window.onTransitionBegin.AddListener(HandleWindowTransitionBegin);
            
            if (applyButton) applyButton.OnClick += HandleApplyButtonClick;
            if (undoButton) undoButton.OnClick += HandleUndoButtonClick;

            Task.Run(CalculateContentPackUsages);
        }

        private void OnDestroy()
        {
            // Stop listening for window events
            if (_window != null) _window.onTransitionBegin.RemoveListener(HandleWindowTransitionBegin);

            if (applyButton) applyButton.OnClick -= HandleApplyButtonClick;
            if (undoButton) undoButton.OnClick -= HandleUndoButtonClick;
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
                // On show, clear, repopulate
                ClearList();
                PopulateList();
                ShowTotalDownloadSize();
                CalculateContentPackUsages();
            }
        }

        /// <summary>
        /// Called when an item is clicked. Check the item can be toggled and apply the change.
        /// </summary>
        /// <param name="listItem"></param>
        private void HandleItemClicked(ContentPackListItem listItem)
        {
            // Can't deselect the default pack
            if (listItem.ContentPack.Name.Equals("Default"))
                return;

            if (_contentPackUsages.ContainsKey(listItem.ContentPack.Name))
            {
                var modal = UIModalBoxManager.Instance.Create(gameObject);
                modal.SetText1("Pack in use");
                modal.SetText2($"This pack can't be deselected as it is used by {_contentPackUsages[listItem.ContentPack.Name]} items on the current map.");
                modal.SetConfirmButtonText("OK");
                modal.Show();
                return;
            }

            // Toggle the item and update download size - if there is no apply button, auto apply the change
            listItem.IsOn = !listItem.IsOn;
            if (applyButton == null) listItem.ContentPack.Selected = listItem.IsOn;
            ShowTotalDownloadSize();
            
            // Changes have been made, so buttons are enabled
            if (applyButton) applyButton.Enabled = true;
            if (undoButton) undoButton.Enabled = true;
        }
        
        /// <summary>
        /// Called when the Undo button is clicked. Revert the pack selection to their previous state.
        /// </summary>
        private void HandleUndoButtonClick()
        {
            // Set all items back to the content pack state
            foreach (var listItem in _items)
            {
                listItem.IsOn = listItem.ContentPack.Selected;
            }

            // Update download size
            ShowTotalDownloadSize();
            
            // Changes undone so buttons are disabled
            if (applyButton) applyButton.Enabled = false;
            if (undoButton) undoButton.Enabled = false;
        }

        /// <summary>
        /// Called when the Apply button is clicked. Apply the changes and load any missing content.
        /// </summary>
        private void HandleApplyButtonClick()
        {
            // If no content packs have been selected / deselected, return
            if (_items.Count(x => x.IsOn ^ x.ContentPack.Selected) == 0)
                return;
            
            // Apply the selection to the underlying content packs
            foreach (var listItem in _items)
            {
                listItem.ContentPack.Selected = listItem.IsOn;
            }

            // Show loading screen and tell it to load new content
            LoadingScreen.Current.LoadAndRender(false, null, null, false);

            // Re-populate the list to force a full refresh of all data
            ClearList();
            PopulateList();
            
            // Update the download size
            ShowTotalDownloadSize();
            
            // Changes applied so buttons are disabled
            if (applyButton) applyButton.Enabled = false;
            if (undoButton) undoButton.Enabled = false;
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
                child.GetComponent<ContentPackListItem>().OnClick -= HandleItemClicked;
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
                await contentPackListItem.Initialise(pack);
                contentPackListItem.OnClick += HandleItemClicked;

                contentPackListItem.transform.SetParent(_content);
                contentPackListItem.gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// Calculates the total download size based on the items that are ticked and shows it in the UI.
        /// </summary>
        private void ShowTotalDownloadSize()
        {
            long totalDownloadSize = _items.Where(x => x.IsOn).Sum(x => x.DownloadSize);
            totalDownloadSizeLabel.text = Helpers.FormatFileSizeString(totalDownloadSize) ?? nothingToDownloadText;
        }

        /// <summary>
        /// Counts how many items on the current map use each content pack.
        /// </summary>
        private void CalculateContentPackUsages()
        {
            if (WorldObjectBase.All == null || WorldObjectBase.All.Count == 0)
            {
                // If no world objects loaded, no packs are in use
                _contentPackUsages.Clear();
                return;
            }

            // From all world objects that have a content pack specified, that aren't in the Default pack, get a list
            // of distinct content pack names
            var uniquePacks = new List<string>(
                WorldObjectBase.All
                    .Where(x => x.ContentItem != null && 
                                               x.ContentItem.ContentPack != null && 
                                               !x.ContentItem.ContentPack.Name.Equals("Default"))
                    .Select(x => x.ContentItem.ContentPack.Name)
                    .Distinct());

            var usages = new Dictionary<string, int>();
            foreach (string pack in uniquePacks)
            {
                // Add each pack and a count of items in that pack that are in use
                usages.Add(pack,
                    WorldObjectBase.All.Count(x => x.ContentItem != null && 
                                                   x.ContentItem.ContentPack != null && 
                                                   x.ContentItem.ContentPack.Name.Equals(pack)));
            }

            // Scatter areas record the packs used, loop through and add them all
            var scatterAreas = FindObjectsOfType<PolygonObject>();
            foreach (var scatterArea in scatterAreas)
            {
                foreach (var scatterItemPack in scatterArea.ContentPacksUsed)
                {
                    if (usages.ContainsKey(scatterItemPack))
                    {
                        usages[scatterItemPack] += 1;
                    }
                    else
                    {
                        usages.Add(scatterItemPack, 1);
                    }
                }
            }
            _contentPackUsages = usages;
        }
        
        #endregion
    }
}