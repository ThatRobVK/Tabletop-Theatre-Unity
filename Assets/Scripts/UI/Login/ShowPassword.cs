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

namespace TT.UI.Login
{
    [RequireComponent(typeof(Toggle))]
    public class ShowPassword : MonoBehaviour
    {
        [SerializeField][Tooltip("The textbox used as the password field.")] private Textbox passwordTextbox;

        private Toggle _toggle;
        
        void OnEnable()
        {
            // Initialise
            _toggle = GetComponent<Toggle>();
            SetPasswordType(_toggle.isOn);

            // Add handler
            _toggle.onValueChanged.AddListener(SetPasswordType);
        }

        private void OnDisable()
        {
            // Remove handler
            if (_toggle) _toggle.onValueChanged.RemoveListener(SetPasswordType);
        }

        private void SetPasswordType(bool value)
        {
            if (passwordTextbox)
            {
                // Set the content type and force the textbox label to update
                passwordTextbox.contentType =
                    value ? InputField.ContentType.Standard : InputField.ContentType.Password;
                passwordTextbox.ForceLabelUpdate();
            }
        }
    }
}
