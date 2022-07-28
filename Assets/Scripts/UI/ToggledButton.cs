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
using UnityEngine;
using UnityEngine.UI;

namespace TT.UI
{
    [RequireComponent(typeof(Button))]
    public class ToggledButton : MonoBehaviour
    {
        #region Events

        /// <summary>
        /// Event raised when the button is clicked whilst enabled.
        /// </summary>
        public Action OnClick;

        #endregion


        #region Editor fields

        [SerializeField][Tooltip("The overlay to show when the button is disabled.")] private GameObject disableOverlay;
        [SerializeField][Tooltip("The highlight transition to control based on the Enabled state.")] private UIHighlightTransition highlightTransition;
        [SerializeField][Tooltip("The highlight image to control based on the Enabled state.")] private Image highlightImage;
        [SerializeField][Tooltip("Enabled buttons show a hover overlay and trigger the OnClick event. If not Enabled, the button will be darker, no hover overlay and clicking won't trigger OnClick.")] private bool buttonEnabled;

        #endregion

        #region Private fields

        private Button _button;
        
        #endregion

        #region Public properties

        /// <summary>
        /// Enabled buttons show a hover overlay and trigger the OnClick event. If not Enabled, the button will be darker, no hover overlay and clicking won't trigger OnClick.
        /// </summary>
        public bool Enabled
        {
            get => buttonEnabled;
            set
            {
                buttonEnabled = value;
                ToggleState();
            }
        }

        public bool Highlight { get; set; }

        #endregion


        #region Lifecycle events

        void OnEnable()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(HandleButtonClicked);
            ToggleState();
        }

        private void OnDisable()
        {
            _button.onClick.RemoveListener(HandleButtonClicked);
        }

        #endregion


        #region Private methods

        /// <summary>
        /// Called when the Button is clicked. If Enabled, invoke the OnClick event.
        /// </summary>
        private void HandleButtonClicked()
        {
            if (buttonEnabled) OnClick?.Invoke();
        }

        /// <summary>
        /// Toggles the visual display of the button based on the Enabled state.
        /// </summary>
        private void ToggleState()
        {
            highlightTransition.enabled = buttonEnabled && !Highlight;
            _button.interactable = buttonEnabled;
            disableOverlay.SetActive(!buttonEnabled);
        }

        #endregion
    }
}