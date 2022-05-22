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

using DuloGames.UI;
using TT.World;
using UnityEngine;
using UnityEngine.UI;

namespace TT.UI.MapEditor.ActionBar
{
    [RequireComponent(typeof(Button))]
    public class TimelapseButton : MonoBehaviour
    {

        #region Editor fields
#pragma warning disable IDE0044 // Make fields read-only

        [SerializeField] private UISliderExtended slider;
        [SerializeField] private Sprite playImage;
        [SerializeField] private Sprite stopImage;

#pragma warning restore IDE0044
        #endregion


        #region Private fields

        private Button _button;
        private Image _image;
        private float _userSetTime;
        private bool _timelapseActive;
        private float _timeLapseTime;

        #endregion


        #region Lifecycle events
#pragma warning disable IDE0051 // Unused members

        void Start()
        {
            _button = GetComponent<Button>();
            _image = GetComponent<Image>();
            _button.onClick.AddListener(HandleButtonClick);
        }


        void Update()
        {
            if (_timelapseActive)
            {
                // Move time forward
                _timeLapseTime += slider.value * Time.deltaTime;
                if (_timeLapseTime >= 24) _timeLapseTime -= 24;
                TimeController.Current.CurrentTime = _timeLapseTime;
            }
        }

#pragma warning restore IDE0051 // Unused members
        #endregion


        #region Event handlers

        private void HandleButtonClick()
        {
            if (_timelapseActive)
            {
                _timelapseActive = false;
                TimeController.Current.CurrentTime = _userSetTime;
                _image.sprite = playImage;
            }
            else
            {
                _timelapseActive = true;
                _userSetTime = TimeController.Current.CurrentTime;
                _timeLapseTime = _userSetTime;
                _image.sprite = stopImage;
            }
        }

        #endregion

    }
}