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
using TMPro;
using TT.Shared.UserContent;
using UnityEngine;
using UnityEngine.UI;

namespace TT.UI.Load
{
    /// <summary>
    /// Attached to an item representing a single map in the MapList.
    /// </summary>
    [RequireComponent(typeof(Toggle))]
    public class MapListItem : MonoBehaviour
    {
        
        #region Events
        
        public event Action<MapMetadata> OnClick;
        
        #endregion
        
        
        #region Editor fields
        
        [SerializeField] private TMP_Text mapNameText;
        [SerializeField] private TMP_Text saveDateText;
        [SerializeField] private CanvasGroup activeOverlay;
        
        #endregion
        
        
        #region Private fields

        private MapMetadata _mapMetadata;
        private Toggle _toggle;
        private ToggleGroup _toggleGroup;
        
        #endregion
        
        
        #region Lifecycle events

        private void Start()
        {
            _toggle = GetComponent<Toggle>();
            _toggle.onValueChanged.AddListener(OnToggleChanged);
            _toggle.group = _toggleGroup;
            _toggle.isOn = false;
            activeOverlay.alpha = 0;
        }

        private void OnDestroy()
        {
            if (_toggle != null)
                _toggle.onValueChanged.AddListener(OnToggleChanged);
        }
        
        #endregion
        
        
        #region Public methods

        /// <summary>
        /// Shows the specified map metadata in this list item.
        /// </summary>
        /// <param name="mapMetadata">The map to show.</param>
        /// <param name="toggleGroup">The toggle group to bind this toggle to.</param>
        public void Initialise(MapMetadata mapMetadata, ToggleGroup toggleGroup)
        {
            _mapMetadata = mapMetadata;
            _toggleGroup = toggleGroup;

            mapNameText.text = mapMetadata.name;
            saveDateText.text = Helpers.FormatShortDateString(mapMetadata.dateSaved);
        }
        
        #endregion
        
        
        #region Private methoads

        /// <summary>
        /// Called when the toggle is changed. Notify listeners of the click.
        /// </summary>
        /// <param name="isOn"></param>
        private void OnToggleChanged(bool isOn)
        {
            OnClick?.Invoke(_mapMetadata);
        }
        
        #endregion
        
    }
}