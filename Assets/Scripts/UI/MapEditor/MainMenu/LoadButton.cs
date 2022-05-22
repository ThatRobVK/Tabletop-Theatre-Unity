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
using TT.Data;
using TT.InputMapping;
using TT.State;
using UnityEngine;
using UnityEngine.UI;

namespace TT.UI.MapEditor.MainMenu
{
    [RequireComponent(typeof(Button))]
    public class LoadButton : MonoBehaviour
    {
        private UIModalBox _savingModal;

        #region Lifecycle events
#pragma warning disable IDE0051 // Unused members

        void Start()
        {
            GetComponent<Button>().onClick.AddListener(LoadMap);
        }

        void Update()
        {
            if (InputMapper.Current.GeneralInput.Load) LoadMap();
        }

#pragma warning restore IDE0051 // Unused members
        #endregion


        #region Event handlers

        private void LoadMap()
        {
            // Listen for the map loaded event
            Map.OnMapLoaded += HandleMapLoaded;

            // Show a modal to the user
            _savingModal = UIModalBoxManager.Instance.Create(gameObject);
            _savingModal.onConfirm.AddListener(OnModalConfirm);
            _savingModal.SetText1("Loading");
            _savingModal.SetText2("Please wait while your map is being loaded.");
            _savingModal.Show();

            Map.Unload();

            string json;

            if (Helpers.Settings.editorSettings.compressSaves)
            {
                var filePath = string.Format("{0}\\map.gz", Application.persistentDataPath);
                Debug.LogFormat("LoadButton :: LoadMap :: Loading from [{0}]", filePath);
                json = Helpers.Decompress(System.IO.File.ReadAllBytes(filePath));
            }
            else
            {
                var filePath = string.Format("{0}\\map.json", Application.persistentDataPath);
                Debug.LogFormat("LoadButton :: LoadMap :: Loading from [{0}]", filePath);
                json = System.IO.File.ReadAllText(filePath);
            }
            
            Map.Load(json);
 
            StateController.Current.ChangeState(StateType.EditorIdleState);

            
        }

        private void HandleMapLoaded()
        {
            // Update text on modal and unhook from event
            if (_savingModal)
                _savingModal.SetText2("Map loaded");
            Map.OnMapLoaded -= HandleMapLoaded;
        }

        /// <summary>
        /// Called when the user clicks "OK" on the modal window.
        /// </summary>
        private void OnModalConfirm()
        {
            UIWindow.GetWindow(UIWindowID.GameMenu).Hide();
        }

        #endregion

    }
}