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
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace TT.UI.MapEditor.GameMenu
{
    /// <summary>
    /// Attached to the Exit button. Returns the user to the main menu when clicked.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class ExitToMenu : MonoBehaviour
    {
        
        #region Editor fields
        
        [FormerlySerializedAs("MainMenuSceneName")] public string mainMenuSceneName = "MainMenu";
        
        #endregion
        
        
        #region Lifecycle events

        void OnEnable()
        {
            // Handle button clicks
            GetComponent<Button>().onClick.AddListener(HandleClick);
        }

        void OnDisable()
        {
            // Remove event handler
            GetComponent<Button>().onClick.RemoveListener(HandleClick);
        }
        
        #endregion
        
        
        #region Public methods

        /// <summary>
        /// Exits to the main menu scene.
        /// </summary>
        public void Exit()
        {
            HandleClick();
        }
        
        #endregion
        
        
        #region Private methods

        /// <summary>
        /// Called when the button is clicked. Return to main menu scene.
        /// </summary>
        private void HandleClick()
        {
            SceneManager.LoadScene(mainMenuSceneName, LoadSceneMode.Single);
        }
        
        #endregion
        
    }
}