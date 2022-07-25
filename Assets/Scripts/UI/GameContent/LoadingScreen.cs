using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using TT.Data;

namespace TT.UI.GameContent
{
    /// <summary>
    /// Attached to the loading UI. Orchestrates downloading, scene switching and map rendering. This must be on an
    /// object flagged with DontDestroyOnLoad as this switches scenes part-way through the process.
    /// </summary>
    public class LoadingScreen : MonoBehaviour
    {
        [SerializeField] private ProgressBar progressBar;
        [SerializeField] private TMP_Text headlineLabel;
        [SerializeField] private TMP_Text detailLabel;

        #region Lifecycle events

        private void Update()
        {
            // While this screen is visible, keep setting the wait cursor
            CursorController.Current.Wait = true;
        }

        #endregion
        
        #region Public methods

        /// <summary>
        /// Loads the required assets, switches to the scene index set in SceneIndex and renders the map. 
        /// </summary>
        /// <param name="loadMapId">The unique ID of the map to load. If null, no map will be loaded.</param>
        /// <param name="sceneName">The name of the scene to switch to. If this value is null, the scene switch will be
        ///     skipped.</param>
        /// <param name="renderMap">If true the map will be re-rendered. If false this step will be skipped.</param>
        public void LoadAndRender(string loadMapId, string sceneName, bool renderMap)
        {
            gameObject.SetActive(true);
            StartCoroutine(LoadAndRenderCoroutine(loadMapId, sceneName, renderMap));
        }

        #endregion


        #region Private methods

        /// <summary>
        /// Loads the required assets, switches to the scene index set in SceneIndex and renders the map.
        /// </summary>
        /// <param name="loadMapId">The unique ID of the map to load. If null, no map will be loaded.</param>
        /// <param name="sceneName">The name of the scene to switch to. If this value is null, the scene switch will be
        ///     skipped.</param>
        /// <param name="renderMap">If true the map will be re-rendered. If false this step will be skipped.</param>
        /// <returns>A coroutine handle.</returns>
        private IEnumerator LoadAndRenderCoroutine(string loadMapId, string sceneName, bool renderMap)
        {
            yield return null;

            if (!string.IsNullOrEmpty(loadMapId))
            {
                SetProgress(0, "Loading map");
                
                if (Map.Current != null) Map.Current.Unload();
                var handle = Map.Load(loadMapId);

                while (handle.Status != TaskStatus.RanToCompletion)
                    yield return null;
            }

            // Check if download is necessary
            var preloadKeys = new List<string>();
            preloadKeys.AddRange(Content.Current.Packs.Where(x => x.Selected)
                .Select(x => x.PreloadItem));
            var downloadSizeHandle = Addressables.GetDownloadSizeAsync(preloadKeys);
            while (!downloadSizeHandle.IsDone) yield return null;
            var downloadSize = downloadSizeHandle.Result;

            if (downloadSize > 0)
            {
                // Download required, set it off and then start the UI update coroutine
                var downloadHandle = Addressables.DownloadDependenciesAsync(preloadKeys);
                while (downloadHandle.Status == AsyncOperationStatus.None)
                {
                    var downloadStatus = downloadHandle.GetDownloadStatus();
                    SetProgress(downloadStatus.Percent, "Downloading content", $"Downloaded {downloadStatus.DownloadedBytes} / {downloadStatus.TotalBytes} bytes");
                    yield return null;
                }

                Addressables.Release(downloadHandle);
            }

            // Load the asset bundles
            float objectsToLoad = preloadKeys.Count;
            float objectsLoaded = 0;
            var preloadHandle = Addressables.LoadAssetsAsync<GameObject>(preloadKeys, 
                loadedObject => { objectsLoaded++; }, Addressables.MergeMode.Union);
            while (!preloadHandle.IsDone)
            {
                SetProgress(objectsLoaded / objectsToLoad, "Loading content packs");
                yield return null;
            }
            SetProgress(1, "Loading content packs");

            // Reset the combined content packs based on what the user has selected
            Content.CombinePacks();

            // Load the specified scene
            if (!string.IsNullOrEmpty(sceneName))
            {
                var sceneLoadHandle = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
                while (!sceneLoadHandle.isDone) yield return null;
            }

            if (renderMap)
            {
                // Start the map render asynchronously
                Map.Current.Render().ConfigureAwait(false);

                // Wait for the map to fully render
                while (Map.Current.RenderStatusPercentage < 1)
                {
                    SetProgress(Map.Current.RenderStatusPercentage, "Rendering map");
                    yield return null;
                }
            }

            gameObject.SetActive(false);
        }

        /// <summary>
        /// Updates the progress bar and text.
        /// </summary>
        /// <param name="percentage">The percentage completion of the current task.</param>
        /// <param name="headline">The main task title to show.</param>
        /// <param name="detail">Detail text to show.</param>
        private void SetProgress(float percentage, string headline, string detail = null)
        {
            progressBar.SetProgress(percentage);
            if (!string.IsNullOrEmpty(headline)) headlineLabel.text = headline;
            detailLabel.text = detail;
        }

        #endregion
    }
}