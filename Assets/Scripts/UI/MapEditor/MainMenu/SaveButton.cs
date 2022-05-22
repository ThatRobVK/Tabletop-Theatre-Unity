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
using TT.World;
using UnityEngine;
using UnityEngine.UI;

namespace TT.UI.MapEditor.MainMenu
{
    [RequireComponent(typeof(Button))]
    public class SaveButton : MonoBehaviour
    {

        #region Lifecycle events
#pragma warning disable IDE0051 // Unused members

        void Start()
        {
            GetComponent<Button>().onClick.AddListener(SaveMap);

        }

        void Update()
        {
            if (InputMapper.Current.GeneralInput.Save) SaveMap();
            if (InputMapper.Current.GeneralInput.SaveAs) Debug.Log("SaveButton :: Update :: Save As not yet supported");
        }

#pragma warning restore IDE0051 // Unused members
        #endregion


        #region Event handlers

        /// <summary>
        /// Called when the button is clicked or the SaveMap shortcut is used.
        /// </summary>
        private void SaveMap()
        {
            var savingModal = UIModalBoxManager.Instance.Create(gameObject);
            savingModal.onConfirm.AddListener(OnModalConfirm);
            savingModal.SetText1("Saving");
            savingModal.SetText2("Please wait while your map is being saved.");
            savingModal.Show();

            if (Map.Current == null)
            {
                // Create a new map if one doesn't exist
                Map.Create("new map", CommsLib.User.Username, GameTerrain.Current.TerrainTextureAddress);
            }

            var json = Map.Current.Save();
            string filePath;

            if (Helpers.Settings.editorSettings.compressSaves)
            {
                filePath = string.Format("{0}\\map.gz", Application.persistentDataPath);
                Debug.LogFormat("SaveButton :: SaveMap :: Saving to [{0}]", filePath);
                System.IO.File.WriteAllBytes(filePath, Helpers.Compress(json));
            }
            else
            {
                filePath = string.Format("{0}\\map.json", Application.persistentDataPath);
                Debug.LogFormat("SaveButton :: SaveMap :: Saving to [{0}]", filePath);
                System.IO.File.WriteAllText(filePath, json);
            }

            savingModal.SetText2(string.Format("Map saved in {0}", filePath));
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