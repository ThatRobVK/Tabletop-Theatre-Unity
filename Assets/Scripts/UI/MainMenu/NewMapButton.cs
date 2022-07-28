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
using TT.UI.GameContent;
using UnityEngine.SceneManagement;

namespace TT.UI.MainMenu
{
    /// <summary>
    /// Attached to the button on the New Map window in the main menu.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class NewMapButton : MonoBehaviour
    {
        
        #region Editor fields
        
        [SerializeField] private Textbox nameTextbox;
        [SerializeField] private Textbox descriptionTextbox;
        [SerializeField] private UISelectField terrainDropdown;
        [SerializeField] private LoadingScreen loadingPanel;
        
        #endregion
        
        
        #region Private fields
        
        private Button _button;
        private UIModalBox _savingModal;
        
        #endregion
        
        
        #region Lifecycle events

        private void Start()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(HandleButtonClicked);
        }
        
        #endregion
        
        
        #region Event handlers

        /// <summary>
        /// Called when the attached button is clicked. Create a new map and load the map editor scene.
        /// </summary>
        private async void HandleButtonClicked()
        {
            // If the index contains a map with the same name, then warn the user of a clash
            var mapIndex = await Helpers.Comms.UserContent.GetMapList();
            if (mapIndex.Any(x => x.name.Equals(nameTextbox.text, StringComparison.InvariantCultureIgnoreCase)))
            {
                _savingModal = UIModalBoxManager.Instance.Create(gameObject);
                _savingModal.SetText1("Name already in use");
                _savingModal.SetText2("The name you have specified is already in use for another map. You can have multiple maps with the same name, but you are recommended to use a unique name to avoid confusion.");
                _savingModal.SetConfirmButtonText("Continue anyway");
                _savingModal.SetCancelButtonText("Cancel");
                _savingModal.onConfirm.AddListener(LoadNewMap);
                _savingModal.Show();
            }
            else
            {
                LoadNewMap();
            }
        }

        #endregion
        
        
        #region Private methods
        
        /// <summary>
        /// Creates a new map and loads the map editor scene.
        /// </summary>
        private void LoadNewMap()
        {
            var selectedTerrain = terrainDropdown.options[terrainDropdown.selectedOptionIndex];
            var terrainLayer = Content.Current.Combined.TerrainLayers.FirstOrDefault(x => x.Name == selectedTerrain);
            var terrainId = terrainLayer != null ? terrainLayer.ID : "0";
            
            Map.New(nameTextbox.text, descriptionTextbox.text, terrainId);

            loadingPanel.LoadAndRender(true, null, Helpers.MapEditorSceneName, true);
        }
        
        #endregion
        
    }
}