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
using TT.State;
using UnityEngine;

namespace TT.UI.MapEditor
{
    [RequireComponent(typeof(UIWindow))]
    public class ChangeStateOnWindowHide : MonoBehaviour
    {

        #region Editor fields
#pragma warning disable IDE0044 // Make fields read-only

        [SerializeField][Tooltip("A list of states from which to change when the window closes. Any other states will be ignored.")] private List<StateType> statesToChangeFrom = new List<StateType>();

#pragma warning restore IDE0044
        #endregion


        #region Private fields

        private UIWindow _window;

        #endregion


        #region Lifecycle events
#pragma warning disable IDE0051 // Unused members

        // Start is called before the first frame update
        void Start()
        {
            _window = GetComponent<UIWindow>();
            _window.onTransitionBegin.AddListener(HandleTransitionBegin);
        }

#pragma warning restore IDE0051 // Unused members
        #endregion


        #region Event handlers

        private void HandleTransitionBegin(UIWindow arg0, UIWindow.VisualState newState, bool arg2)
        {
            if (newState == UIWindow.VisualState.Hidden && statesToChangeFrom.Contains(StateController.CurrentStateType))
            {
                StateController.Current.ChangeState(StateType.EditorIdleState);
            }
        }

        #endregion

    }
}