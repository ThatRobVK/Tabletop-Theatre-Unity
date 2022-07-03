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

using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TT.UI
{
    [RequireComponent(typeof(InputField))]
    public class TextboxSizeLimiter : MonoBehaviour
    {
        #region Editor fields

        [SerializeField][Tooltip("The maximum length of text the user is allowed to enter. 0 for no limit.")] private int maxLength;
        [SerializeField][Tooltip("The length at which the user will be warned that they are close to the limit. 0 for no warning.")] private int warnAtLength;
        [SerializeField][Tooltip("A GameObject to enable when the warning length has been reached, and disable when the length goes under again. Can be null.")] private GameObject warningPanel;
        [SerializeField][Tooltip("The label in which to show the warning text. Can be null.")] private TMP_Text warningLabel;
        
        #endregion

        
        #region Private fields
        
        private InputField _inputField;
        
        #endregion
        
        
        #region Lifecycle events
        
        private void Start()
        {
            if (maxLength > 0 || warnAtLength > 0)
            {
                // If either limit set, listen for value change events
                _inputField = GetComponent<InputField>();
                _inputField.onValueChanged.AddListener(HandleValueChanged);
            }
        }
        
        #endregion
        
        
        #region Event handlers
        
        /// <summary>
        /// Called when the value of the textbox changes. Check for max length and warn if over the warning limit.
        /// </summary>
        /// <param name="newValue">The value after the edit.</param>
        private void HandleValueChanged(string newValue)
        {
            // Temporarily stop listening to stop this from triggering itself
            _inputField.onValueChanged.RemoveListener(HandleValueChanged);
            
            // Trim value if too long
            if (maxLength > 0 && newValue.Length > maxLength)
                _inputField.text = newValue.Substring(0, maxLength);

            // Determine appropriate values for the warnings
            var active = false;
            var label = string.Empty;
            if (warnAtLength > 0 && newValue.Length >= warnAtLength)
            {
                active = true;
                label = $"{_inputField.text.Length} / {maxLength}";
            }
            
            // Set the warning objects if they exist
            if (warningPanel != null)
                warningPanel.SetActive(active);

            if (warningLabel != null)
                warningLabel.text = label;
            
            // Listen for the next key press
            _inputField.onValueChanged.AddListener(HandleValueChanged);
        }

        #endregion
        
    }
}