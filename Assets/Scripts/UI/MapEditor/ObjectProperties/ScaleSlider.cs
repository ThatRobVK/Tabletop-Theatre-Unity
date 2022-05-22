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
using TT.InputMapping;
using TT.MapEditor;
using TT.World;
using UnityEngine;

namespace TT.UI.MapEditor.ObjectProperties
{
    [RequireComponent(typeof(UISliderExtended))]
    public class ScaleSlider : PropertySlider
    {

        #region Private fields

        private UISliderExtended _slider;
        private UIWindow _window;

        #endregion


        #region Lifecycle events
#pragma warning disable IDE0051 // Unused members

        protected override void Start()
        {
            _window = GetComponentInParent<UIWindow>();

            _slider = GetComponent<UISliderExtended>();
            _slider.onValueChanged.AddListener(UpdateFromSlider);

            base.Start();
        }

        protected override void Update()
        {
            if (WorldObjectBase.Current && _window.IsOpen)
            {
                // Hook keyboard controls into undo
                if (InputMapper.Current.WorldObjectInput.StartScale) UndoValue = GetWorldObjectValue();
                if (InputMapper.Current.WorldObjectInput.StopScale) RegisterUndo(UndoValue);
                
                var scale = InputMapper.Current.WorldObjectInput.GetScaleAmount();
                if (scale != 0)
                {
                    WorldObjectBase.Current.Scale(scale);
                }
            }


            base.Update();
        }

#pragma warning restore IDE0051 // Unused members
        #endregion


        #region Override methods

        protected override float GetWorldObjectValue()
        {
            return (WorldObjectBase.Current != null) ? WorldObjectBase.Current.transform.localScale.x : 1;
        }

        protected override float ConvertSliderToWorldObject(float sliderValue)
        {
            return sliderValue / 2 + 0.5f;
        }

        protected override float ConvertWorldObjectToSlider(float worldObjectValue)
        {
            return (worldObjectValue - 0.5f) * 2;
        }

        protected override void UpdateSlider(float sliderValue)
        {
            _slider.value = sliderValue;
        }

        protected override void UpdateWorldObject(float worldObjectValue)
        {
            if (WorldObjectBase.Current)
                WorldObjectBase.Current.ScaleTo(worldObjectValue);
        }

        protected override void SetWholeNumbers(bool snapToGrid)
        {
            // Snapping to grid for scale is not done via the slider
            _slider.wholeNumbers = false;
        }

        protected override void RegisterUndo(float undoValue)
        {
            if (WorldObjectBase.Current == null) return;
            UndoController.RegisterAction(ActionType.Scale, WorldObjectBase.Current.ObjectId, undoValue);
        }

        #endregion

    }
}