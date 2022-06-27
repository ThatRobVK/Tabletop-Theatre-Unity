using System;
using DuloGames.UI;
using UnityEngine;
using UnityEngine.UI;

namespace TT.UI
{
    /// <summary>
    /// Assigned to the modal box. Hides the confirm and cancel buttons when they don't have any text assigned.
    /// </summary>
    [RequireComponent(typeof(UIModalBox))]
    public class ModalWindowButtonToggle : MonoBehaviour
    {
        
        #region Editor fields
        
        [SerializeField][Tooltip("The confirm button on the modal box.")] private Button confirmButton;
        [SerializeField][Tooltip("The text element on the confirm button.")] private Text confirmText;
        [SerializeField][Tooltip("The cancel button on the modal box.")] private Button cancelButton;
        [SerializeField][Tooltip("The text element on the cancel button.")] private Text cancelText;
        
        #endregion
        
        
        #region Private fields
        
        private UIWindow _modalBox;
        
        #endregion
        
        
        #region Lifecycle events
        
        private void Start()
        {
            SetConfirmText(confirmText.text);
            SetCancelText(cancelText.text);
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Shows the confirm button with the specified text, or hides it if the text is null or empty.
        /// </summary>
        /// <param name="text">The text to show on the confirm button, or null or empty to hide it.</param>
        public void SetConfirmText(string text)
        {
            confirmButton.gameObject.SetActive(!string.IsNullOrEmpty(text));
            confirmText.text = text;
        }

        /// <summary>
        /// Shows the cancel button with the specified text, or hides it if the text is null or empty.
        /// </summary>
        /// <param name="text">The text to show on the cancel button, or null or empty to hide it.</param>
        public void SetCancelText(string text)
        {
            cancelButton.gameObject.SetActive(!string.IsNullOrEmpty(text));
            cancelText.text = text;
        }
        
        #endregion
        
    }
}
