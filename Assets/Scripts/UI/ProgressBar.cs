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
        
        [SerializeField][Tooltip("The background area filled by the progress bar.")] private RectTransform progressArea;
        [SerializeField][Tooltip("The progress bar that is resized based on percentage.")] private RectTransform progressBar;
        [SerializeField][Tooltip("The label to show the percentage in.")] private TMP_Text percentageLabel;

        private float _rectWidth = 0;

        /// <summary>
        /// Updates the progress bar and percentage text based on the specified progress.
        /// </summary>
        /// <param name="progress">The progress as a float between 0 and 1.</param>
        /// <param name="showText">If true, the percentage will be shown next to the progress bar.</param>
        public void SetProgress(float progress, bool showText = true)
        {
            if (_rectWidth == 0) _rectWidth = progressArea.rect.width;
            progressBar.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, progress * _rectWidth);
            percentageLabel.text = showText ? $"{Mathf.Round(progress * 100)}%" : string.Empty;
        }
    }
}
