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

using TT.Data;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TT.UI.Load
{
    [RequireComponent(typeof(Button))]
    public class LoadMapButton : MonoBehaviour
    {
        
        #region Constants
        
        private const string MAIN_MENU_SCENE = "MainMenu";
        private const string EDITOR_SCENE = "MapEditor";
        
        #endregion
        
        
        #region Editor fields
        
        [SerializeField][Tooltip("The list exposing the selected map.")] private MapList mapList;
        
        #endregion
        
        
        #region Private fields

        private Button _button;
        
        #endregion
        
        
        #region Lifecycle events

        private void Start()
        {
            // Listen for button click events
            _button = GetComponent<Button>();
            _button.onClick.AddListener(HandleButtonClick);
        }
        
        

        private void OnDestroy()
        {
            // Stop listening for button click events
            if (_button != null)
                _button.onClick.RemoveListener(HandleButtonClick);
        }
        
        #endregion
        
        
        #region Private methods

        /// <summary>
        /// Called when the attached button is clicked. Load the selected map.
        /// </summary>
        private async void HandleButtonClick()
        {
            // Load the selected map
            await Map.Load(mapList.SelectedMap.id);

            if (SceneManager.GetActiveScene().name == MAIN_MENU_SCENE)
            {
                // If in main menu, switch to editor, which will render the map for us
                SceneManager.LoadScene(EDITOR_SCENE, LoadSceneMode.Single);
            }
            else
            {
                // If not in the main menu, render the map once loaded
                await Map.Current.Render();
            }
        }
        
        #endregion
        
    }
}