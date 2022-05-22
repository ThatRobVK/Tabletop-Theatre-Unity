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
using TT.MapEditor;
using TT.World;
using UnityEngine;

namespace TT.UI.MapEditor.ObjectProperties
{
    [RequireComponent(typeof(UISliderExtended))]
    public class WorldObjectOptionSlider : PropertySlider
    {
        #region Editor fields
#pragma warning disable IDE0044 // Make fields read-only

        [SerializeField][Tooltip("The option this slider is for.")] private WorldObjectOption option;

#pragma warning restore IDE0044
        #endregion


        #region Private fields

        private UISliderExtended _slider;

        #endregion


        #region Lifecycle events
#pragma warning disable IDE0051 // Unused members

        protected override void Start()
        {
            _slider = GetComponent<UISliderExtended>();
            _slider.onValueChanged.AddListener(UpdateFromSlider);

            UpdatedThisFrame = true;
            _slider.minValue = minValue;
            _slider.maxValue = maxValue;
            _slider.value = GetWorldObjectValue();

            base.Start();
        }

#pragma warning restore IDE0051 // Unused members
        #endregion


        #region Override methods

        protected override float GetWorldObjectValue()
        {
            return (WorldObjectBase.Current != null && WorldObjectBase.Current.OptionValues.ContainsKey(option)) ? (float)WorldObjectBase.Current.OptionValues[option] : 5f;
        }

        protected override float ConvertSliderToWorldObject(float sliderValue)
        {
            return sliderValue;
        }

        protected override float ConvertWorldObjectToSlider(float worldObjectValue)
        {
            return worldObjectValue;
        }

        protected override void UpdateSlider(float sliderValue)
        {
            _slider.value = sliderValue;
        }

        protected override void UpdateWorldObject(float worldObjectValue)
        {
            if (WorldObjectBase.Current != null && WorldObjectBase.Current.OptionValues.ContainsKey(option))
            {
                WorldObjectBase.Current.SetOption(option, worldObjectValue);
            }
        }

        protected override void SetWholeNumbers(bool snapToGrid)
        {
            // Snapping to grid is not supported for option sliders
            _slider.wholeNumbers = false;
        }

        protected override void RegisterUndo(float undoValue)
        {
            if (WorldObjectBase.Current == null) return;
            UndoController.RegisterAction(ActionType.Option, WorldObjectBase.Current.ObjectId, new KeyValuePair<WorldObjectOption, object>(option, undoValue));
        }

        #endregion

    }
}