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

#pragma warning disable IDE0090 // "Simplify new expression" - implicit object creation is not supported in the .NET version used by Unity 2020.3

using TT.State;
using UnityEngine;
using UnityEngine.UI;

namespace TT.UI.MapEditor.TerrainMenu
{
    [RequireComponent(typeof(Button))]
    public class TerrainPaintButton : MonoBehaviour
    {

        #region Editor fields
#pragma warning disable IDE0044 // Make fields read-only

        [SerializeField][Tooltip("The dropdown that exposes the currently selected terrain layer.")] private TerrainPaintLayerDropdown layerDropdown;
        [SerializeField][Tooltip("The slider that exposes the selected brush size.")] private BrushSizeSlider brushSizeSlider;
        [SerializeField][Tooltip("The slider that exposes the selected softness.")] private SoftnessSlider softnessSlider;
        [SerializeField][Tooltip("The slider that exposes the selected softness.")] private OpacitySlider opacitySlider;

#pragma warning restore IDE0044
        #endregion


        #region Private fields

        private Button _button;

        #endregion


        #region Lifecycle events
#pragma warning disable IDE0051 // Unused members

        void Start()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(HandleButtonClick);
        }

#pragma warning restore IDE0051 // Unused members
        #endregion


        #region Private methods

        private void HandleButtonClick()
        {
            var newState = StateController.CurrentStateType != StateType.TerrainPaint ? StateType.TerrainPaint : StateType.EditorIdleState;
            StateController.Current.ChangeState(newState);

            if (StateController.CurrentState is TerrainPaintState paintState)
            {
                // Set paint options if the state has changed correctly
                paintState.PaintLayer = layerDropdown.SelectedIndex;
                paintState.BrushRadius = brushSizeSlider.BrushSize;
                paintState.Smoothness = softnessSlider.Softness;
                paintState.Opacity = opacitySlider.Opacity;
            }
        }

        #endregion
    }
}