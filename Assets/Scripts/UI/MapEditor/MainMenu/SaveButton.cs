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

using UnityEngine;
using UnityEngine.UI;
using DuloGames.UI;
using TT.Data;
using TT.InputMapping;

namespace TT.UI.MapEditor.MainMenu
{
    [RequireComponent(typeof(Button))]
    public class SaveButton : MonoBehaviour
    {

        #region Private fields
        
        private Button _button;
        private UIModalBox _savingModal;
        
        #endregion
        
        
        #region Lifecycle events

        private void Start()
        {
            // Listen for button click events
            _button = GetComponent<Button>(); 
            _button.onClick.AddListener(HandleButtonClick);
        }

        private void Update()
        {
            // Call save based on key binding
            if (InputMapper.Current.GeneralInput.Save) HandleButtonClick();
        }

        private void OnDestroy()
        {
            // Remove event handler
            if (_button != null) _button.onClick.RemoveListener(HandleButtonClick);
        }

        #endregion


        #region Event handlers

        /// <summary>
        /// Called when the button is clicked or the SaveMap shortcut is used.
        /// </summary>
        private void HandleButtonClick()
        {
            _savingModal = UIModalBoxManager.Instance.Create(gameObject);
            _savingModal.SetText1("Saving");
            _savingModal.SetText2("Please wait while your map is being saved.");
            _savingModal.Show();

            SaveMapAsync();
        }

        /// <summary>
        /// Called when the confirm button on a modal box is clicked.
        /// </summary>
        private void HandleModalConfirm()
        {
            UIWindow.GetWindow(UIWindowID.GameMenu).Hide();
        }
        
        #endregion
        
        
        #region Private methods
        
        /// <summary>
        /// Saves the current map.
        /// </summary>
        private async void SaveMapAsync()
        {
            var success = await Map.Current.Save();
            _savingModal.Close();

            if (success)
            {
                // If successful, return
                HandleModalConfirm();
                return;
            }

            // If not successful, show an error message
            _savingModal = UIModalBoxManager.Instance.Create(gameObject);
            _savingModal.SetText1("Error");
            _savingModal.SetText2("There was a problem saving your map. Please try again.");
            _savingModal.SetConfirmButtonText("OK");
            _savingModal.onConfirm.AddListener(HandleModalConfirm);
        }

        #endregion

    }
}