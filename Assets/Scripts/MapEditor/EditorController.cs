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

using System.Collections.Generic;
using Amazon.Util;
using TT.Data;
using TT.Shared;
using TT.State;
using TT.World;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace TT.MapEditor
{
    public class EditorController : MonoBehaviour
    {

        #region Private fields

        private StateController _stateController;
        private IUser _user;

        #endregion


        #region Public properties

        public static readonly List<WorldObjectBase> WorldObjects = new List<WorldObjectBase>();

        #endregion


        #region Lifecycle events
#pragma warning disable IDE0051 // Unused members

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
            Debug.Log("EditorController :: Awake :: Loading content");
            var json = await CommsLib.GameContent.GetContentJsonAsync();
            Content.Load(json);

            var location = await CommsLib.GameContent.GetContentCatalogLocationAsync();
            AssetBundle.UnloadAllAssetBundles(false);
            var loadContentCatalogAsync = Addressables.LoadContentCatalogAsync(location);
            loadContentCatalogAsync.Completed += OnContentCatalogLoaded;
        }

        private void OnContentCatalogLoaded(AsyncOperationHandle<IResourceLocator> obj)
        {
            // Load the first game terrain as default
            GameTerrain.Current.LoadDefaultTexture(Content.Current.Combined.TerrainLayers[0].ID);
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

#pragma warning restore IDE0051 // Unused members
        #endregion

    }
}