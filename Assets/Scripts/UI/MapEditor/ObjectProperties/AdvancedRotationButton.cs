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
using UnityEngine;
using UnityEngine.UI;

namespace TT.UI.MapEditor.ObjectProperties
{
    [RequireComponent(typeof(Button))]
    public class AdvancedRotationButton : MonoBehaviour
    {

        #region Editor fields
#pragma warning disable IDE0044 // Make fields read-only

        [SerializeField][Tooltip("The advanced rotation panel to show when this button is clicked.")] private GameObject advancedRotationPanel;
        [SerializeField] private Image openCloseImage;
        [SerializeField][Tooltip("The sprite to show when the panel is closed.")] private Sprite closedSprite;
        [SerializeField][Tooltip("The sprite to show when the panel is open.")] private Sprite openSprite;

#pragma warning restore IDE0044
        #endregion


        #region Private fields

        private Button _button;
        private UIWindow _window;

        #endregion


        #region Lifecycle events
#pragma warning disable IDE0051 // Unused members

        void Start()
        {
            _window = GetComponentInParent<UIWindow>();
            _window.onTransitionBegin.AddListener(HandleWindowTransition);

            _button = GetComponent<Button>();
            _button.onClick.AddListener(TogglePanel);
        }

#pragma warning restore IDE0051 // Unused members
        #endregion


        #region Event handlers

        private void TogglePanel()
        {
            // Toggle the panel and update the button image
            advancedRotationPanel.SetActive(!advancedRotationPanel.activeSelf);
            openCloseImage.sprite = advancedRotationPanel.activeSelf ? openSprite : closedSprite;
        }

        /// <summary>
        /// Called when the window opens or closes.
        /// </summary>
        /// <param name="arg0">Ignored.</param>
        /// <param name="state">The target state of the window.</param>
        /// <param name="arg2">Ignored.</param>
        private void HandleWindowTransition(UIWindow arg0, UIWindow.VisualState state, bool arg2)
        {
            if (state == UIWindow.VisualState.Shown && advancedRotationPanel.activeSelf)
            {
                // Default to closed
                TogglePanel();
            }
        }

        #endregion

    }
}