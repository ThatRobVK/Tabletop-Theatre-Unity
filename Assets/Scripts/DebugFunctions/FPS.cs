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

namespace TT.DebugFunctions
{
    [RequireComponent(typeof(Text))]
    public class FPS : MonoBehaviour
    {

        #region Editor fields
#pragma warning disable IDE0044 // Make fields read-only

        [SerializeField][Tooltip("How long in seconds to wait between updates.")] private float UpdateInterval = 0.5f;

#pragma warning restore IDE0044
        #endregion


        #region Private fields

        private Text _text;
        private int _avgFrameRate;
        private float _timePass;
        private bool _isOn = true;

        #endregion


        #region Lifecycle events
#pragma warning disable IDE0051 // Unused members

        // Start is called before the first frame update
        void Start()
        {
            _text = GetComponent<Text>();
        }

        private void Update() 
        {
            if (_isOn != Helpers.Settings.editorSettings.showFpsCounter)
            {
                _isOn = Helpers.Settings.editorSettings.showFpsCounter;
                var canvasGroup = GetComponentInParent<CanvasGroup>();
                if (canvasGroup != null)
                {
                    canvasGroup.alpha = _isOn ? 1 : 0;
                }
            }

            if (!_isOn) return;

            float current = (int)(1f / Time.unscaledDeltaTime);
    		_avgFrameRate = (int)current;
    
    		_timePass += Time.deltaTime;
    		if(_timePass > UpdateInterval) {
    			_timePass = 0;
    			_text.text = string.Format("{0} FPS", _avgFrameRate);
    		}
    	}

#pragma warning restore IDE0051 // Unused members
        #endregion

    }
}