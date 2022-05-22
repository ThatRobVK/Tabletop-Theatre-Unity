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
using UnityEngine.UI;

namespace TT.UI
{
    [RequireComponent(typeof(Button))]
    public class TogglePanelButton : MonoBehaviour
    {

        #region Editor fields
#pragma warning disable IDE0044 // Make fields read-only

        [SerializeField][Tooltip("True if the object is shown, false otherwise.")] private bool state;
        [SerializeField][Tooltip("The objects to show/hide when the button is clicked.")] private GameObject[] toggledObjects = new GameObject[] { };
        [SerializeField][Tooltip("The text to show when the objects are hidden.")] private string showText = "SHOW";
        [SerializeField][Tooltip("The text to show when the objects are shown.")] private string hideText = "HIDE";
        [SerializeField][Tooltip("The key that toggles this panel.")] private KeyCode toggleKey;

#pragma warning restore IDE0044
        #endregion


        #region Private fields

        private Button _button;
        private Text _buttonText;

        #endregion


        #region Lifecycle events
#pragma warning disable IDE0051 // Unused members

        void OnEnable()
        {
            _button = GetComponent<Button>();
            _buttonText = _button.GetComponentInChildren<Text>();
            _button.onClick.AddListener(HandleButtonClick);

            ToggleObjects(state);
        }

        void OnDisable()
        {
            _button.onClick.RemoveListener(HandleButtonClick);
        }

        void Update()
        {
            if (Input.GetKeyDown(toggleKey)) HandleButtonClick();
        }

#pragma warning restore IDE0051 // Unused members
        #endregion


        #region Event handlers

        /// <summary>
        /// Called when the button is clicked. Toggle the objects based on the visibility of the first one.
        /// </summary>
        private void HandleButtonClick()
        {
            if (toggledObjects.Length > 0)
            {
                ToggleObjects(!toggledObjects[0].activeSelf);
            }
        }

        #endregion


        #region Private methods

        /// <summary>
        /// Show or hide the objects in the list.
        /// </summary>
        /// <param name="show">True to show, false to hide the objects.</param>
        private void ToggleObjects(bool show)
        {
            foreach (var toggledObject in toggledObjects)
            {
                toggledObject.SetActive(show);
            }

            if (_buttonText)
            {
                _buttonText.text = show ? hideText : showText;
            }
        }

        #endregion
    }
}