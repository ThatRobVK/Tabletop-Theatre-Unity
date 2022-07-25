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
using DuloGames.UI;
using TMPro;
using TT.Shared.GameContent;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

namespace TT.UI.GameContent
{
    /// <summary>
    /// Attached to an item representing a single content pack in the content pack list.
    /// </summary>
    public class ContentPackListItem : MonoBehaviour
    {
        
        #region Events
        
        public event Action<ContentPack, long> OnTick;
        public event Action<ContentPack, long> OnUntick;

        #endregion
        
        
        #region Editor fields

        [SerializeField] private Toggle toggle;
        [SerializeField] private TMP_Text packNameText;
        [SerializeField] private TMP_Text downloadSizeText;
        [SerializeField] private CanvasGroup activeOverlay;
        [SerializeField] private UITooltipShow tooltip;
        
        #endregion
        
        
        #region Private fields

        private ContentPack _contentPack;
        private long _downloadSize;
        
        #endregion
        
        
        #region Lifecycle events

        private void Start()
        {
            toggle.onValueChanged.AddListener(OnToggleChanged);
            activeOverlay.alpha = 0;
        }

        private void OnDestroy()
        {
            if (toggle != null)
                toggle.onValueChanged.AddListener(OnToggleChanged);
        }
        
        #endregion
        
        
        #region Public methods

        /// <summary>
        /// Shows the specified map metadata in this list item.
        /// </summary>
        /// <param name="contentPack">The map to show.</param>
        public void Initialise(ContentPack contentPack, bool interactable = true)
        {
            _contentPack = contentPack;

            toggle.interactable = interactable;
            toggle.isOn = _contentPack.Selected;

            // Add tooltip
            tooltip.contentLines = new[]
            {
                new UITooltipLineContent
                {
                    LineStyle = UITooltipLines.LineStyle.Default, 
                    IsSpacer = false, 
                    Content = contentPack.Description
                }
            };
            
            packNameText.text = contentPack.Name;
            SetDownloadSizeText(contentPack);
        }

        #endregion
        

        #region Event handlers
        
        /// <summary>
        /// Called when the toggle is changed. Update the content pack and notify listeners of the toggle .
        /// </summary>
        /// <param name="isOn"></param>
        private void OnToggleChanged(bool isOn)
        {
            _contentPack.Selected = isOn;
            
            if (isOn)
                OnTick?.Invoke(_contentPack, _downloadSize);
            else
                OnUntick?.Invoke(_contentPack, _downloadSize);
        }

        #endregion
        

        #region Private methoads

        /// <summary>
        /// Gets the download size for the selected pack from addressables and sets the download size text.
        /// </summary>
        /// <param name="contentPack">The content pack to check the size for.</param>
        private async void SetDownloadSizeText(ContentPack contentPack)
        {
            var downloadSize = await Addressables.GetDownloadSizeAsync(contentPack.PreloadItem).Task;
            downloadSizeText.text = Helpers.FormatFileSizeString(downloadSize) ?? "downloaded";
            
            OnToggleChanged(contentPack.Selected);
        }
        
        #endregion
        
    }
}