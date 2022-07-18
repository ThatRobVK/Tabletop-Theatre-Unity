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
using DuloGames.UI;
using TMPro;
using TT.Data;

namespace TT.UI.MapEditor
{
    /// <summary>
    /// Class for the Map Properties window to sync the UI and the Map object.
    /// </summary>
    [RequireComponent(typeof(UIWindow))]
    public class MapPropertiesWindow : MonoBehaviour
    {
        
        #region Editor fields
        
        [SerializeField][Tooltip("The textbox used for the map name.")] private Textbox mapNameTextbox;
        [SerializeField][Tooltip("The textbox used for the description.")] private Textbox descriptionTextbox;
        [SerializeField][Tooltip("The textbox used for the map author.")] private TMP_Text authorLabel;
        [SerializeField][Tooltip("The textbox used for the date created.")] private TMP_Text dateCreatedLabel;
        [SerializeField][Tooltip("The textbox used for the date saved.")] private TMP_Text dateSavedLabel;
        
        #endregion
        
        
        #region Private fields
        
        private UIWindow _window;
        
        #endregion
        
        
        #region Lifecycle events
        
        private void Start()
        {
            _window = GetComponent<UIWindow>();
            _window.onTransitionBegin.AddListener(HandleWindowTransition);
            
            mapNameTextbox.onValueChanged.AddListener(HandleMapNameChanged);
            descriptionTextbox.onValueChanged.AddListener(HandleDescriptionChanged);
        }

        private void OnDestroy()
        {
            if (_window != null)
                _window.onTransitionBegin.RemoveListener(HandleWindowTransition);
            
            if (mapNameTextbox != null)
                mapNameTextbox.onValueChanged.RemoveListener(HandleMapNameChanged);
            
            if (descriptionTextbox)
                descriptionTextbox.onValueChanged.RemoveListener(HandleDescriptionChanged);
        }
        
        #endregion
        
        
        #region Event handlers

        /// <summary>
        /// Called when the window shows or hides. Update the textboxes on show.
        /// </summary>
        /// <param name="window"></param>
        /// <param name="targetState"></param>
        /// <param name="instant"></param>
        private void HandleWindowTransition(UIWindow window, UIWindow.VisualState targetState, bool instant)
        {
            if (targetState == UIWindow.VisualState.Shown)
            {
                mapNameTextbox.text = Map.Current.Name;
                descriptionTextbox.text = Map.Current.Description;
                authorLabel.text = Map.Current.Author;
                dateCreatedLabel.text = Map.Current.DateCreated.ToLocalTime().ToString("g");
                dateSavedLabel.text = Map.Current.DateSaved > Map.Current.DateCreated
                    ? Map.Current.DateSaved.ToLocalTime().ToString("g")
                    : "never";
            }
        }

        /// <summary>
        /// Called when the value in the map name textbox has changed. Update the Map object.
        /// </summary>
        /// <param name="newValue"></param>
        private void HandleMapNameChanged(string newValue)
        {
            Map.Current.Name = newValue;
        }

        /// <summary>
        /// Called when the value in the description textbox has changed. Update the Map object.
        /// </summary>
        /// <param name="newValue"></param>
        private void HandleDescriptionChanged(string newValue)
        {
            Map.Current.Description = newValue;
        }
        
        #endregion
        
    }
}
