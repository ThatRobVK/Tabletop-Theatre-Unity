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
using TT.Data;
using TT.MapEditor;
using TT.Shared.World;
using TT.World;
using UnityEngine;
using UnityEngine.UI;

namespace TT.UI.MapEditor.ObjectProperties
{
    [RequireComponent(typeof(Button))]
    public class ItemOptionColor : MonoBehaviour
    {

        #region Editor fields
#pragma warning disable IDE0044 // Make fields read-only

        [SerializeField][Tooltip("The colour picker component.")] private CanvasGroup colorPickerCanvas;
        [SerializeField][Tooltip("The colour picker component.")] private FlexibleColorPicker colorPicker;
        [SerializeField][Tooltip("The hex input field.")] private Textbox hexInput;

#pragma warning restore IDE0044
        #endregion


        #region Private fields

        private Button _button;
        private Color _selectedColor;
        private WorldObjectOption _option;
        private Color _undoColor;

        #endregion


        #region Lifecycle events
#pragma warning disable IDE0051 // Unused members

        void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(OnImageClick);
            hexInput.onValueChanged.AddListener(HandleHexValueChanged);
        }

        void Update()
        {
            if (WorldObjectBase.Current != null)
            {
                if (!colorPicker.color.Equals(_selectedColor))
                {
                    // If the color has changed, update
                    _selectedColor = colorPicker.color;
                    if (WorldObjectBase.Current)
                        WorldObjectBase.Current.SetOption(_option, _selectedColor);
                }
                else if (WorldObjectBase.Current.OptionValues.ContainsKey(_option) && !hexInput.IsSelected)
                {
                    // Set the colour if the user isn't typing a hex code
                    _selectedColor = (Color)WorldObjectBase.Current.OptionValues[_option];
                    colorPicker.color = _selectedColor;
                }
            }
        }

        void OnDestroy()
        {
            _button.onClick.RemoveListener(OnImageClick);
        }

#pragma warning restore IDE0051 // Unused members
        #endregion


        #region Event handlers

        /// <summary>
        /// Called when the colour preview image is clicked. Toggle the colour picker visibility.
        /// </summary>
        public void OnImageClick()
        {
            bool show = colorPickerCanvas.alpha == 0;

            colorPickerCanvas.alpha = show ? 1 : 0;
            colorPickerCanvas.interactable = show;
            colorPickerCanvas.blocksRaycasts = show;

            if (show)
            {
                // When showing, store the current colour for undo
                _undoColor = _selectedColor;
            }
            else if (WorldObjectBase.Current)
            {
                // When hiding, register an undo for the colour
                UndoController.RegisterAction(ActionType.Option, WorldObjectBase.Current.ObjectId, new KeyValuePair<WorldObjectOption, object>(_option, _undoColor));
            }
        }

        /// <summary>
        /// Called when the user types in the hex input. Fix an issue where pasting a full hex doesn't update until you press enter.
        /// </summary>
        /// <param name="hexValue"></param>
        private void HandleHexValueChanged(string hexValue)
        {
            // If pasting in a full hex code, apply the colour - the picker doesn't update until you press enter otherwise
            if (hexValue.Length >= 6 && ColorUtility.TryParseHtmlString(hexValue, out Color newColor))
            {
                colorPicker.color = newColor;
            }
        }

        #endregion


        #region Public methods

        /// <summary>
        /// Set the colour picker up based on the given colour.
        /// </summary>
        /// <param name="option">The option to set on the WorldObject when the colour changes.</param>
        /// <param name="value">The starting colour.</param>
        public void Initialise(WorldObjectOption option, object value)
        {
            this._option = option;

            _selectedColor = (Color)value;
            colorPicker.startingColor = _selectedColor;
            colorPicker.color = _selectedColor;
        }

        #endregion
    }
}