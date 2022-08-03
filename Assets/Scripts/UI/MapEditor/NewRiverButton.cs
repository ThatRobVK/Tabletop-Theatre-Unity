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
using TT.Data;
using TT.Shared.GameContent;
using TT.Shared.World;
using TT.State;
using TT.World;

namespace TT.UI.MapEditor
{
    [RequireComponent(typeof(Button))]
    public class NewRiverButton : MonoBehaviour
    {
        #region Private fields

        private Button _button;

        #endregion


        #region Lifecycle events

        void Start()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(HandleButtonClick);
        }

        #endregion


        #region Private methods

        private void HandleButtonClick()
        {
            ContentItem item = new ContentItem()
            {
                Name = "River",
                Type = WorldObjectType.River,
                IDs = Content.Current.Combined.RiversRoads.Rivers
            };

            // If an object is selected, deselect it before spawning a new one
            if (WorldObjectBase.Current)
                WorldObjectBase.Current.Deselect();

            StateController.Current.ChangeState(StateType.RamObjectPlacement);
            (StateController.CurrentState as RamObjectPlacementState)?.Initialise(item, 0);
        }

        #endregion
    }
}