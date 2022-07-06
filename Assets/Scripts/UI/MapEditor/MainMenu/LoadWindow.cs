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

using System.Text;
using DuloGames.UI;
using TMPro;
using TT.Data;
using TT.InputMapping;
using TT.MapEditor;
using TT.State;
using TT.UI.Load;
using UnityEngine;
using UnityEngine.UI;

namespace TT.UI.MapEditor.MainMenu
{
    [RequireComponent(typeof(UIWindow))]
    public class LoadWindow : MonoBehaviour
    {
        
        #region Editor fields
        
        [SerializeField] private Toggle currentMapToggle;
        [SerializeField] private Toggle differentMapToggle;
        [SerializeField] private TMP_Text currentMapSavedText;
        [SerializeField] private Button loadButton;
        [SerializeField] private MapList mapList;
        
        #endregion
        
        
        #region Private fields

        private UIWindow _window;
        private UIModalBox _loadingModal;
        
        #endregion
        
        
        #region Lifecycle methods

        private void Start()
        {
            _window = GetComponent<UIWindow>();
            _window.onTransitionBegin.AddListener(HandleWindowTransitionBegin);
            loadButton.onClick.AddListener(HandleLoadButtonClick);
        }

        private void Update()
        {
            if (InputMapper.Current.GeneralInput.Load)
            {
                // Handle load key combination
                currentMapToggle.isOn = true;
                HandleLoadButtonClick();
            }
            else if (InputMapper.Current.GeneralInput.LoadOther)
            {
                // Handle Load Other key combination
                _window.Show();
            }
        }

        private void OnDestroy()
        {
            if (_window != null)
                _window.onTransitionBegin.RemoveListener(HandleWindowTransitionBegin);
            
            if (loadButton != null)
                loadButton.onClick.RemoveListener(HandleLoadButtonClick);
        }
        
        #endregion
        
        
        #region Event handlers

        /// <summary>
        /// Called when the window starts to show or hide. Update the display on show.
        /// </summary>
        /// <param name="window">The window that is transitioning.</param>
        /// <param name="targetState">The state it is transitioning to.</param>
        /// <param name="instant">Whether the transition is instant.</param>
        private void HandleWindowTransitionBegin(UIWindow window, UIWindow.VisualState targetState, bool instant)
        {
            // Don't update when window is closing
            if (targetState != UIWindow.VisualState.Shown)
                return;
            
            if (Map.Current.DateSaved < Map.Current.DateCreated)
            {
                // Map hasn't been saved yet, can't load current map
                differentMapToggle.isOn = true;
                currentMapToggle.enabled = false;
                currentMapSavedText.text = "The current map hasn't been saved yet, so cannot be loaded.";
                loadButton.GetComponent<ToggledButton>().Enabled = false;
                return;
            }

            // Build up message about state of current map
            StringBuilder currentMapText = new StringBuilder(Map.Current.Name);
            currentMapText.Append(" - saved ").Append(Helpers.FormatShortDateString(Map.Current.DateSaved));
            currentMapText.Append($"- {UndoController.NumChangesSinceLastSave} unsaved changes.");

            currentMapSavedText.text = currentMapText.ToString();
            currentMapToggle.enabled = true;
            currentMapToggle.isOn = true;
            loadButton.GetComponent<ToggledButton>().Enabled = true;
        }

        /// <summary>
        /// Called when the Load button is clicked. Load the selected map, but warn user of unsaved changes first.
        /// </summary>
        private void HandleLoadButtonClick()
        {
            if (UndoController.NumChangesSinceLastSave == 0)
            {
                // No changes made, just reload the map
                LoadMap();
                return;
            }
            
            // Show a modal confirming whether the user wants to load and lose their changes
            _loadingModal = UIModalBoxManager.Instance.Create(gameObject);
            _loadingModal.SetText1("Unsaved changes");
            string savedOrLoadedText = Map.Current.DateSaved > Map.Current.DateLoaded ? "last saved" : "loaded";
            _loadingModal.SetText2($"You have made {UndoController.NumChangesSinceLastSave} since the map was " +
                                   $"{savedOrLoadedText}. These changes will be lost. Are you sure you want to load?");
            _loadingModal.SetConfirmButtonText("YES, LOAD");
            _loadingModal.SetCancelButtonText("CANCEL");
            _loadingModal.onConfirm.AddListener(LoadMap);
            _loadingModal.Show();
        }

        #endregion
        
        
        #region Private methods

        /// <summary>
        /// Loads the selected map.
        /// </summary>
        private async void LoadMap()
        {
            // Don't act if the load button is disabled
            if (!loadButton.GetComponent<ToggledButton>().Enabled)
                return;
            
            var mapId = currentMapToggle.isOn ? Map.Current.Id.ToString() : mapList.SelectedMap.id;
            
            Debug.Log($"LoadWindow :: LoadMap :: Loading map {mapId}");
            
            // Unload the map and load the saved data
            Map.Current.Unload();
            await Map.Load(mapId);
            await Map.Current.Render();
            
            StateController.Current.ChangeState(StateType.EditorIdleState);
            
            UIWindow.GetWindow(UIWindowID.GameMenu).Hide();
            _window.Hide();
        }
        
        #endregion

    }
}