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
using DuloGames.UI;
using TT.Data;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TT.UI.MainMenu
{
    [RequireComponent(typeof(Button))]
    public class NewMapButton : MonoBehaviour
    {
        [SerializeField] private Textbox nameTextbox;
        [SerializeField] private Textbox descriptionTextbox;
        [SerializeField] private UISelectField terrainDropdown;
        
        private Button _button;
        private UIModalBox _savingModal;

        private void Start()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(HandleButtonClicked);
        }

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
                _savingModal.onConfirm.AddListener(DoNewMap);
                _savingModal.Show();
            }
            else
            {
                DoNewMap();
            }
        }

        private void DoNewMap()
        {
            var selectedTerrain = terrainDropdown.options[terrainDropdown.selectedOptionIndex];
            var terrainLayer = Content.Current.Combined.TerrainLayers.FirstOrDefault(x => x.Name == selectedTerrain);
            var terrainId = terrainLayer != null ? terrainLayer.ID : "0";
            
            Map.New(nameTextbox.text, descriptionTextbox.text, terrainId);

            SceneManager.LoadScene("MapEditor", LoadSceneMode.Single);
        }
    }
}