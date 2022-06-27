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
using DuloGames.UI;
using TT.Data;

namespace TT.UI.MapEditor.MainMenu
{
    /// <summary>
    /// Assigned to the Save As window to handle user input and save as if confirmed.
    /// </summary>
    [RequireComponent(typeof(UIWindow))]
    public class SaveAsWindow : MonoBehaviour
    {
        
        #region Editor fields
        
        [SerializeField][Tooltip("The textbox containing the new name for the copy.")] private Textbox nameTextbox;
        [SerializeField][Tooltip("The button the user clicks to confirm saving a copy.")] private Button confirmButton;
        [SerializeField] [Tooltip("The panel to show while waiting for the operation to complete.")]
        private GameObject waitPanel;
        
        #endregion
        
        
        #region Private fields
        
        private UIWindow _window;
        private UIModalBox _savingModal;
        
        #endregion

        
        #region Lifecycle events
        
        private void Start()
        {
            // Listen for events
            _window = GetComponent<UIWindow>();
            _window.onTransitionBegin.AddListener(HandleWindowTransitionBegin);
            confirmButton.onClick.AddListener(HandleConfirmButtonClick);
        }

        private void OnDestroy()
        {
            // Stop listening for events
            if (_window != null)
                _window.onTransitionBegin.RemoveListener(HandleWindowTransitionBegin);
            if (confirmButton != null)
                confirmButton.onClick.RemoveListener(HandleConfirmButtonClick);
        }
        
        #endregion
        
        
        #region Event handlers

        /// <summary>
        /// Called when the window shows or hides. Set initial state on show.
        /// </summary>
        /// <param name="window">The window that is showing or hiding.</param>
        /// <param name="newState">The new state of the window.</param>
        /// <param name="instant">Whether the change is instant.</param>
        private void HandleWindowTransitionBegin(UIWindow window, UIWindow.VisualState newState, bool instant)
        {
            if (newState == UIWindow.VisualState.Shown)
            {
                nameTextbox.text = Map.Current.Name;
            }
        }

        /// <summary>
        /// Called when the confirm button is clicked. Save the map and close the window.
        /// </summary>
        private async void HandleConfirmButtonClick()
        {
            // If the index contains a map with a different ID but the same name, then warn the user of a clash
            var mapIndex = await Helpers.Comms.UserContent.GetMapIndex();
            if (mapIndex.Any(x => !x.id.Equals(Map.Current.Id.ToString()) &&
                                    x.name.Equals(nameTextbox.text, StringComparison.InvariantCultureIgnoreCase)))
            {
                _savingModal = UIModalBoxManager.Instance.Create(gameObject);
                _savingModal.SetText1("Name already in use");
                _savingModal.SetText2("The name you have specified is already in use for another map. You can have multiple maps with the same name, but you are recommended to use a unique name to avoid confusion.");
                _savingModal.SetConfirmButtonText("Save anyway");
                _savingModal.SetCancelButtonText("Cancel");
                _savingModal.onConfirm.AddListener(DoSave);
                _savingModal.Show();
            }
            else
            {
                DoSave();
            }
        }

        /// <summary>
        /// Saves a copy of the current map and hides the window.
        /// </summary>
        private async void DoSave()
        {
            waitPanel.SetActive(true);
            await Map.Current.SaveCopy(nameTextbox.text);
            waitPanel.SetActive(false);
            _window.Hide();

            // Unhook event
            if (_savingModal != null)
                _savingModal.onConfirm.RemoveListener(DoSave);
        }
        
        #endregion
        
    }
}
