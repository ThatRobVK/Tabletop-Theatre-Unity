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
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
using TT.Data;
using TT.Shared;
using TT.State;
using TT.World;

namespace TT.MapEditor
{
    public class EditorController : MonoBehaviour
    {

        #region Private fields

        private StateController _stateController;
        private IUser _user;

        #endregion


        #region Lifecycle events

        void Awake()
        {
            // Limit the application to the screen's refresh rate so we don't hammer the GPU for no reason
            Application.targetFrameRate = Screen.currentResolution.refreshRate;
            Debug.LogFormat("EditorController :: Awake :: Setting target framerate to {0}", Application.targetFrameRate.ToString());

            // Instantiate state controller
            _stateController = new StateController();

            LoadContent();
        }

        private async void LoadContent()
        {
            // Load content definitions
            Debug.Log("EditorController :: Awake :: Loading content");
            if (!Content.ContentLoaded)
                await Content.Load();

            // Load content catalog
            var location = await CommsLib.GameContent.GetContentCatalogLocationAsync();
            AssetBundle.UnloadAllAssetBundles(false);
            var loadContentCatalogAsync = Addressables.LoadContentCatalogAsync(location);
            loadContentCatalogAsync.Completed += OnContentCatalogLoaded;
        }

        private void OnContentCatalogLoaded(AsyncOperationHandle<IResourceLocator> obj)
        {
            Map.Current.Render().ConfigureAwait(false);
            // TODO: Load terrain texture from map data
            // Load the first game terrain as default
            //GameTerrain.Current.LoadDefaultTexture(Content.Current.Combined.TerrainLayers[0].ID);
        }

        void Start()
        {
            // Set state controller to initial idle state
            _stateController.ChangeState(StateType.EditorIdleState);

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