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
using TT.Data;

namespace TT.UI.MainMenu
{
    /// <summary>
    /// Attached to a select field. Loads the available terrain layers on load and every time content is loaded.
    /// </summary>
    [RequireComponent(typeof(UISelectField))]
    public class LoadTerrainDropdown : MonoBehaviour
    {
        
        #region Private fields
        
        private UISelectField _selectField;
        
        #endregion
        
        
        #region Lifecycle events

        private void Start()
        {
            _selectField = GetComponent<UISelectField>();

            // Listen for auth and content events
            Helpers.Comms.User.OnLoginSuccess += HandleLoginSuccess;
            Content.OnContentChanged += HandleContentChanged;

            if (Helpers.Comms.User.IsLoggedIn && Content.ContentLoaded)
            {
                // Already logged in and content loaded - load the terrain options
                HandleContentChanged();
            }
            else if (Helpers.Comms.User.IsLoggedIn)
            {
                // Logged in but no content loaded - load the content
                HandleLoginSuccess();
            }
        }

        private void OnDestroy()
        {
            // Stop listening to auth events
            if (Helpers.Comms != null && Helpers.Comms.User != null)
                Helpers.Comms.User.OnLoginSuccess -= HandleLoginSuccess;
            
            // Stop listening for content events
            Content.OnContentChanged += HandleContentChanged;
        }
        
        #endregion
        
        
        #region Event handlers

        /// <summary>
        /// Called when the user logs in. Check content is loaded.
        /// </summary>
        private void HandleLoginSuccess()
        {
            if (!Content.ContentLoaded)
            {
                Content.Load().ConfigureAwait(false);
            }
            else
            {
                HandleContentChanged();
            }
        }
        
        /// <summary>
        /// Called when new content is loaded. Update the select field with the terrain options.
        /// </summary>
        private void HandleContentChanged()
        {
            // Add each terrain to the drop down
            _selectField.options.Clear();
            for (int i = 0; i < Content.Current.Combined.TerrainLayers.Length; i++)
                _selectField.AddOption(Content.Current.Combined.TerrainLayers[i].Name);

            _selectField.SelectOptionByIndex(0);
        }
        
        #endregion
        
    }
}
