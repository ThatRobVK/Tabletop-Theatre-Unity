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

using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using TMPro;
using DuloGames.UI;
using TT.Data;
using TT.Shared.UserContent;

namespace TT.UI.Load
{
    /// <summary>
    /// Attached to the map details panel in the load window. Shows the details when called.
    /// </summary>
    public class MapListDetails : MonoBehaviour
    {
        
        #region Events
        
        public event Action OnDelete;
        
        #endregion
        
        
        #region Editor fields
        
        [SerializeField] private TMP_Text nameLabel;
        [SerializeField] private TMP_Text authorLabel;
        [SerializeField] private TMP_Text descriptionLabel;
        [SerializeField] private TMP_Text createDateLabel;
        [SerializeField] private TMP_Text saveDateLabel;
        [SerializeField] private TMP_Text downloadStatusLabel;
        [SerializeField] private Button deleteButton;

        #endregion
        
        
        #region Private fields

        private string _id;
        private UIModalBox _deleteModal;
        private AsyncOperationHandle<long> _downloadSizeHandle;

        #endregion
        
        
        #region Lifecycle events

        private void Start()
        {
            deleteButton.onClick.AddListener(HandleButtonClick);
        }
        
        #endregion


        #region Event handlers

        /// <summary>
        /// Called when the delete button is clicked. Confirm whether the user wants to delete the map.
        /// </summary>
        private void HandleButtonClick()
        {
            if (Map.Current != null && Map.Current.Id.ToString().Equals(_id))
            {
                // Disallow deleting of a currently loaded map
                _deleteModal = UIModalBoxManager.Instance.Create(gameObject);
                _deleteModal.SetText1("Can't delete this map");
                _deleteModal.SetText2("You can't delete a map that is currently loaded. To delete this map, either load a different map and try again, or delete it via the main menu.");
                _deleteModal.SetConfirmButtonText("OK");
                _deleteModal.Show();
                return;
            }
            
            // Confirm the user wants to delete the map
            _deleteModal = UIModalBoxManager.Instance.Create(gameObject);
            _deleteModal.SetText1("Delete map?");
            _deleteModal.SetText2("Are you sure you want to delete this map? This cannot be undone.");
            _deleteModal.SetConfirmButtonText("Delete");
            _deleteModal.SetCancelButtonText("Cancel");
            _deleteModal.onConfirm.AddListener(HandleModalConfirm);
            _deleteModal.Show();
        }

        /// <summary>
        /// Called when the user confirms the delete prompt. Delete the map.
        /// </summary>
        private async void HandleModalConfirm()
        {
            // Delete, if successful invoke the event and clear the details
            if (await Helpers.Comms.UserContent.DeleteMap(_id))
            {
                OnDelete?.Invoke();
                ShowMapDetails(null);
            }
        }

        #endregion

        
        #region Public methods
        
        /// <summary>
        /// Shows the passed in map metadata, or clears the display if null is passed in.
        /// </summary>
        /// <param name="mapMetadata">Metadata about a map to show, or null to clear.</param>
        public void ShowMapDetails(MapMetadata mapMetadata)
        {
            if (mapMetadata != null)
            {
                _id = mapMetadata.id;
                nameLabel.text = mapMetadata.name;
                authorLabel.text = $"By {mapMetadata.authorName}";
                descriptionLabel.text = $"Description:\n{mapMetadata.description}";
                createDateLabel.text = $"Created {Helpers.FormatShortDateString(mapMetadata.dateCreated)}";
                saveDateLabel.text = $"Last saved {Helpers.FormatShortDateString(mapMetadata.dateSaved)}";
                deleteButton.gameObject.SetActive(true);

                var keys = Content.Current.Packs
                    .Where(x => mapMetadata.contentPacks.Contains(x.Name))
                    .Select(x => x.PreloadItem);
                Addressables.GetDownloadSizeAsync(keys).Completed += handle =>
                {
                    downloadStatusLabel.text = handle.Result > 0
                        ? $"{Helpers.FormatFileSizeString(handle.Result)} to download"
                        : "All content downloaded";
                    Addressables.Release(handle);
                };
            }
            else
            {
                _id = null;
                nameLabel.text = string.Empty;
                authorLabel.text = string.Empty;
                descriptionLabel.text = string.Empty;
                createDateLabel.text = string.Empty;
                saveDateLabel.text = string.Empty;
                deleteButton.gameObject.SetActive(false);
            }
        }

        #endregion
        
    }
}