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

using System.Collections.Generic;
using DuloGames.UI;
using TT.Data;
using TT.World;
using UnityEngine;

namespace TT.UI.MapEditor.TerrainMenu
{
    public class TerrainOption : MonoBehaviour
    {
        #region Editor fields
#pragma warning disable IDE0044 // Make fields read-only

        [SerializeField] private WorldObjectOption option;
        [SerializeField] private string displayName;
        [SerializeField][Tooltip("The dropdown to control")] private UISelectField selectField;

#pragma warning restore IDE0044
        #endregion


        #region Private fields

        private bool _updatedThisFrame;

        #endregion


        #region Lifecycle events
#pragma warning disable IDE0051 // Unused members

        void Start()
        {
            _updatedThisFrame = true;

            // Set up the values and initialise the select field
            List<string> options = new List<string>();
            options.Add("None");
            foreach (var layer in Content.Current.Combined.TerrainLayers)
            {
                options.Add(layer.Name);
            }
            Initialise(option, displayName, options.ToArray(), 0);

            // Start listening for change events after initialising
            selectField.onChange.AddListener(HandleChange);
        }

        void Update()
        {
            if (WorldObjectBase.Current != null && !_updatedThisFrame)
            {
                _updatedThisFrame = true;
                selectField.SelectOptionByIndex(GetDropdownIndexFromWorldObject());
            }
        }

        void LateUpdate()
        {
            _updatedThisFrame = false;
        }


#pragma warning restore IDE0051 // Unused members
        #endregion


        #region Event handlers

        /// <summary>
        /// Called when the dropdown selection is changed. Updates the lights on the selected WorldObject.
        /// </summary>
        /// <param name="index">The index that was selected.</param>
        /// <param name="value">The text that was selected.</param>
        private void HandleChange(int index, string value)
        {
            if (_updatedThisFrame) return;

            if (WorldObjectBase.Current)
                WorldObjectBase.Current.SetOption(option, index - 1);
        }

        #endregion


        #region Public methods

        /// <summary>
        /// Sets the dropdown up to work with the current WorldObject.
        /// </summary>
        /// <param name="controlledOption">The option to be controlled by the dropdown.</param>
        /// <param name="textToDisplay">The name to display to the user for this option.</param>
        /// <param name="dropdownOptions">A list of strings representing the options to show in the dropdown.</param>
        /// <param name="value">The initial option to select.</param>
        public void Initialise(WorldObjectOption controlledOption, string textToDisplay, string[] dropdownOptions, object value)
        {
            _updatedThisFrame = true;

            selectField.options.Clear();
            selectField.options.AddRange(dropdownOptions);
            selectField.SelectOptionByIndex((int)value);
        }

        #endregion



        #region Private methods

        /// <summary>
        /// Returns a dropdown index for the current WorldObject's lighting options
        /// </summary>
        /// <returns></returns>
        private int GetDropdownIndexFromWorldObject()
        {
            if (WorldObjectBase.Current != null)
            {
                if (WorldObjectBase.Current.OptionValues.ContainsKey(option))
                {
                    return (int)WorldObjectBase.Current.OptionValues[option] + 1;
                }
            }

            return 0;
        }

        #endregion



    }
}