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
using UnityEngine.UI;

namespace TT.UI.GameContent
{
    /// <summary>
    /// Attached to the loading UI. Orchestrates downloading, scene switching and map rendering. This must be on an
    /// object flagged with DontDestroyOnLoad as this switches scenes part-way through the process.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class LoadingScreen : MonoBehaviour
    {
        public event Action LoadAndRenderComplete;
        
        [SerializeField] private ProgressBar progressBarFullscreen;
        [SerializeField] private ProgressBar progressBarOverlay;
        [SerializeField] private TMP_Text headlineLabelFullscreen;
        [SerializeField] private TMP_Text headlineLabelOverlay;
        [SerializeField] private TMP_Text detailLabelFullscreen;
        [SerializeField] private TMP_Text detailLabelOverlay;
        [SerializeField] private GameObject fullscreenPanel;
        [SerializeField] private GameObject overlayPanel;
        [SerializeField] private Image waitSpinner;

        private CanvasGroup _canvasGroup;
        private ProgressBar _currentProgressBar;
        private TMP_Text _currentHeadlineLabel;
        private TMP_Text _currentDetailLabel;
        private bool _visible;
       
        #region Public properties

        public static LoadingScreen Current { get; private set; } 
        
        #endregion
        
        #region Lifecycle events

        private void Awake()
        {
            Current = this;
            _canvasGroup = GetComponent<CanvasGroup>();
        }
        
        private void Update()
        {
            if (_visible)
            {
                // While this screen is visible, keep setting the wait cursor
                CursorController.Current.Wait = true;
            }
        }

        #endregion
        
        #region Public methods

        /// <summary>
        /// Loads the required assets, switches to the scene index set in SceneIndex and renders the map. 
        /// </summary>
        /// <param name="fullscreen">If true a full screen loading screen is shown, otherwise a semi transparent
        ///     overlay with a centered loading bar is shown.</param>
        /// <param name="loadMapId">The unique ID of the map to load. If null, no map will be loaded.</param>
        /// <param name="sceneName">The name of the scene to switch to. If this value is null, the scene switch will be
        ///     skipped.</param>
        /// <param name="renderMap">If true the map will be re-rendered. If false this step will be skipped.</param>
        public void LoadAndRender(bool fullscreen, string loadMapId, string sceneName, bool renderMap)
        {
            StartCoroutine(LoadAndRenderCoroutine(fullscreen, loadMapId, sceneName, renderMap));
        }

        #endregion


        #region Private methods

        /// <summary>
        /// Loads the required assets, switches to the scene index set in SceneIndex and renders the map.
        /// </summary>
        /// <param name="fullscreen">If true a full screen loading screen is shown, otherwise a semi transparent
        ///     overlay with a centered loading bar is shown.</param>
        /// <param name="loadMapId">The unique ID of the map to load. If null, no map will be loaded.</param>
        /// <param name="sceneName">The name of the scene to switch to. If this value is null, the scene switch will be
        ///     skipped.</param>
        /// <param name="renderMap">If true the map will be re-rendered. If false this step will be skipped.</param>
        /// <returns>A coroutine handle.</returns>
        private IEnumerator LoadAndRenderCoroutine(bool fullscreen, string loadMapId, string sceneName, bool renderMap)
        {
            SetProgress(0, "Loading");
            yield return null;

            if (!string.IsNullOrEmpty(loadMapId))
            {
                Debug.Log($"LoadingScreen :: LoadAndRenderCoroutine :: Loading map {loadMapId}");
                
                Show(fullscreen);
                SetProgress(-1, "Preparing map");
                yield return null;
                
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
                Debug.Log($"LoadingScreen :: LoadAndRenderCoroutine :: Downloading content: {downloadSize} bytes");

                Show(fullscreen);
                
                // Download required, set it off and then start the UI update coroutine
                var downloadHandle = Addressables.DownloadDependenciesAsync(preloadKeys, Addressables.MergeMode.Union, false);
                while (downloadHandle.Status == AsyncOperationStatus.None)
                {
                    var downloadStatus = downloadHandle.GetDownloadStatus();
                    var downloadedBytes = downloadStatus.DownloadedBytes / 1000000f;
                    var totalBytes = downloadStatus.TotalBytes / 1000000f;
                    SetProgress(downloadStatus.Percent, "Downloading content", $"Downloaded {downloadedBytes:N1} / {totalBytes:N1} MB");
                    yield return null;
                }

                if (downloadHandle.Status == AsyncOperationStatus.Failed || downloadHandle.OperationException != null)
                {
                    Debug.LogWarning($"LoadingScreen :: LoadAndRenderCoroutine :: Failed to download: {downloadHandle.OperationException}");
                }

                Addressables.Release(downloadHandle);
            }

            // Load the asset bundles
            var preloadKeysNotInMemory = Content.GetContentNotInMemory(preloadKeys);
            if (preloadKeysNotInMemory.Count > 0)
            {
                Debug.Log($"LoadingScreen :: LoadAndRenderCoroutine :: Loading content packs");

                Show(fullscreen);
                
                float objectsToLoad = preloadKeysNotInMemory.Count;
                float objectsLoaded = 0;
                SetProgress(0, "Loading content packs");
                var preloadHandle = Addressables.LoadAssetsAsync<GameObject>(preloadKeysNotInMemory,
                    loadedObject => { objectsLoaded++; }, Addressables.MergeMode.Union);
                while (!preloadHandle.IsDone)
                {
                    SetProgress(objectsLoaded / objectsToLoad, "Loading content packs");
                    yield return null;
                }

                SetProgress(1, "Loading content packs");
            }

            // Reset the combined content packs based on what the user has selected
            Content.CombinePacks();

            // Load the specified scene
            if (!string.IsNullOrEmpty(sceneName))
            {
                Debug.Log($"LoadingScreen :: LoadAndRenderCoroutine :: Switching scene to {sceneName}");

                Show(fullscreen);
                
                var sceneLoadHandle = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
                while (!sceneLoadHandle.isDone) yield return null;
            }

            if (renderMap && Map.Current != null)
            {
                Debug.Log($"LoadingScreen :: LoadAndRenderCoroutine :: Rendering map");

                Show(fullscreen);
                
                // Start the map render asynchronously
                Map.Current.Render().ConfigureAwait(false);

                // Wait for the map to fully render
                while (Map.Current.RenderStatusPercentage < 1)
                {
                    SetProgress(-1, "Finishing up");
                    yield return null;
                }
            }

            Hide();
            
            LoadAndRenderComplete?.Invoke();
        }

        /// <summary>
        /// Shows the loading screen.
        /// </summary>
        /// <param name="fullscreen">If true a full screen loading screen is shown, otherwise a semi transparent
        ///     overlay with a centered loading bar is shown.</param>
        private void Show(bool fullscreen)
        {
            _canvasGroup.alpha = 1;
            _canvasGroup.blocksRaycasts = true;
            
            fullscreenPanel.SetActive(fullscreen);
            overlayPanel.SetActive(!fullscreen);
            _currentProgressBar = fullscreen ? progressBarFullscreen : progressBarOverlay;
            _currentHeadlineLabel = fullscreen ? headlineLabelFullscreen : headlineLabelOverlay;
            _currentDetailLabel = fullscreen ? detailLabelFullscreen : detailLabelOverlay;

            _visible = true;
        }

        /// <summary>
        /// Hides the loading screen.
        /// </summary>
        private void Hide()
        {
            _canvasGroup.alpha = 0;
            _canvasGroup.blocksRaycasts = false;
            waitSpinner.gameObject.SetActive(false);

            _visible = false;
        }

        /// <summary>
        /// Updates the progress bar and text.
        /// </summary>
        /// <param name="percentage">The percentage completion of the current task.</param>
        /// <param name="headline">The main task title to show.</param>
        /// <param name="detail">Detail text to show.</param>
        private void SetProgress(float percentage, string headline, string detail = null)
        {
            if (!_visible) return;
            
            if (percentage >= 0)
            {
                waitSpinner.gameObject.SetActive(false);
                _currentProgressBar.gameObject.SetActive(true);
                _currentProgressBar.SetProgress(percentage);
            }
            else
            {
                _currentProgressBar.gameObject.SetActive(false);
                waitSpinner.gameObject.SetActive(true);
            }

            if (!string.IsNullOrEmpty(headline)) _currentHeadlineLabel.text = headline;
            _currentDetailLabel.text = detail;
        }

        #endregion
    }
}