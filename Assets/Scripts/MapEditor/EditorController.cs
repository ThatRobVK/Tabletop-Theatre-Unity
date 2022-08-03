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
using TT.State;
using TT.World;

namespace TT.MapEditor
{
    /// <summary>
    /// Responsible for setting up the Map Editor and managing the StateController. The MapEditor scene must have one
    /// and only one instance of this class running or unexpected behaviour will result.
    /// </summary>
    public class EditorController : MonoBehaviour
    {

        #region Private fields

        private StateController _stateController;

        #endregion


        #region Lifecycle events

        void Awake()
        {
            // Limit the application to the screen's refresh rate so we don't hammer the GPU for no reason
            // TODO: Move this into the main menu and make configurable
            Application.targetFrameRate = Screen.currentResolution.refreshRate;
            Debug.LogFormat("EditorController :: Awake :: Setting target framerate to {0}", 
                Application.targetFrameRate.ToString());

            // Instantiate state controller
            _stateController = new StateController();
        }
        
        void Start()
        {
            // TODO: Can the state controller live on its own?
            // Set state controller to initial idle state
            _stateController.ChangeState(StateType.EditorIdleState);

            // TODO: Set via the map being loaded or initialised
            TimeController.Current.CurrentTime = 12;
        }

        void Update()
        {
            // Tell state to update
            _stateController.Update();
        }

        #endregion

    }
}