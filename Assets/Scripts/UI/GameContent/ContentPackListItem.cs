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
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using TMPro;
using DuloGames.UI;
using TT.Shared.GameContent;

namespace TT.UI.GameContent
{
    /// <summary>
    /// Attached to an item representing a single content pack in the content pack list.
    /// </summary>
    public class ContentPackListItem : MonoBehaviour
    {
        
        #region Events

        /// <summary>
        /// Called when the item is clicked.
        /// </summary>
        public event Action<ContentPackListItem> OnClick;

        #endregion
        
        
        #region Editor fields

        [SerializeField] private Button button;
        [SerializeField] private TMP_Text packNameText;
        [SerializeField] private TMP_Text downloadSizeText;
        [SerializeField] private CanvasGroup activeOverlay;
        [SerializeField] private Toggle toggle;
        [SerializeField] private UITooltipShow tooltip;
        
        #endregion
        
        
        #region Public properties
        
        /// <summary>
        /// The download size of this item.
        /// </summary>
        public long DownloadSize { get; private set; }

        /// <summary>
        /// Whether the toggle on this item is on.
        /// </summary>
        public bool IsOn
        {
            get => toggle.isOn;
            set => toggle.isOn = value;
        }

        public ContentPack ContentPack { get; private set; }

        #endregion
        
        
        #region Lifecycle events

        private void Start()
        {
            button.onClick.AddListener(HandleButtonClick);
            activeOverlay.alpha = 0;
        }

        private void OnDestroy()
        {
            if (button != null)
                button.onClick.RemoveListener(HandleButtonClick);
        }
        
        #endregion
        
        
        #region Public methods

        /// <summary>
        /// Shows the specified map metadata in this list item.
        /// </summary>
        /// <param name="contentPack">The map to show.</param>
        public async Task Initialise(ContentPack contentPack)
        {
            ContentPack = contentPack;

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
            toggle.isOn = contentPack.Selected;

            DownloadSize = await Addressables.GetDownloadSizeAsync(contentPack.PreloadItem).Task;
            downloadSizeText.text = Helpers.FormatFileSizeString(DownloadSize) ?? "downloaded";
        }

        #endregion
        

        #region Event handlers

        /// <summary>
        /// Called when this item is clicked. Notify listeners of the click.
        /// </summary>
        private void HandleButtonClick()
        {
            OnClick?.Invoke(this);
        }

        #endregion
                
    }
}