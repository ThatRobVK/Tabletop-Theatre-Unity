using System;
using UnityEngine;
using TMPro;
using UnityEngine.Animations;

namespace TT.UI
{
    /// <summary>
    /// Attached to the progress bar. Exposes a method to update the progress bar percentage.
    /// </summary>
    public class ProgressBar : MonoBehaviour
    {
        #region Editor fields
        
        [SerializeField][Tooltip("The background area filled by the progress bar.")] private RectTransform progressArea;
        [SerializeField][Tooltip("The progress bar that is resized based on percentage.")] private RectTransform progressBar;
        [SerializeField][Tooltip("The label to show the percentage in.")] private TMP_Text percentageLabel;
        [SerializeField][Tooltip("How quickly the progress bar lerps to the final position. Higher is faster.")] private float movementTime = 8f;
        
        #endregion
        
        
        #region Private fields

        private float _rectWidth;
        private float _targetWidth;
        
        #endregion
        
        
        #region Lifecycle events

        private void Update()
        {
            if (_targetWidth < progressBar.rect.width || Mathf.Approximately(_rectWidth, _targetWidth))
            {
                // If going down or going to 100%, just go immediately to the target value
                progressBar.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _targetWidth);
            }
            else if (progressBar.rect.width < _targetWidth)
            {
                // If going up, go smoothly
                var newWidth = Mathf.Lerp(progressBar.rect.width, _targetWidth, Time.deltaTime * movementTime);
                progressBar.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, newWidth);
            }
        }
        
        #endregion
        
        
        #region Public methods
        
        /// <summary>
        /// Updates the progress bar and percentage text based on the specified progress.
        /// </summary>
        /// <param name="progress">The progress as a float between 0 and 1.</param>
        /// <param name="showText">If true, the percentage will be shown next to the progress bar.</param>
        public void SetProgress(float progress, bool showText = true)
        {
            if (_rectWidth == 0) _rectWidth = progressArea.rect.width;
            _targetWidth = progress * _rectWidth;
            
            percentageLabel.text = showText ? $"{Mathf.Round(progress * 100)}%" : string.Empty;
        }
        
        #endregion
        
    }
}
