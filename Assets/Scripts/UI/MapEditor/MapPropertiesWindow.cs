using System.Net.Mime;
using Amazon.CognitoIdentity.Model;
using UnityEngine;
using DuloGames.UI;
using TMPro;
using TT.Data;

namespace TT.UI.MapEditor
{
    /// <summary>
    /// Class for the Map Properties window to sync the UI and the Map object.
    /// </summary>
    [RequireComponent(typeof(UIWindow))]
    public class MapPropertiesWindow : MonoBehaviour
    {
        
        #region Editor fields
        
        [SerializeField][Tooltip("The textbox used for the map name.")] private Textbox mapNameTextbox;
        [SerializeField][Tooltip("The textbox used for the description.")] private Textbox descriptionTextbox;
        [SerializeField][Tooltip("The textbox used for the map author.")] private TMP_Text authorLabel;
        [SerializeField][Tooltip("The textbox used for the date created.")] private TMP_Text dateCreatedLabel;
        [SerializeField][Tooltip("The textbox used for the date saved.")] private TMP_Text dateSavedLabel;
        
        #endregion
        
        
        #region Private fields
        
        private UIWindow _window;
        
        #endregion
        
        
        #region Lifecycle events
        
        private void Start()
        {
            _window = GetComponent<UIWindow>();
            _window.onTransitionBegin.AddListener(HandleWindowTransition);
            
            mapNameTextbox.onValueChanged.AddListener(HandleMapNameChanged);
            descriptionTextbox.onValueChanged.AddListener(HandleDescriptionChanged);
        }

        private void OnDestroy()
        {
            if (_window != null)
                _window.onTransitionBegin.RemoveListener(HandleWindowTransition);
            
            if (mapNameTextbox != null)
                mapNameTextbox.onValueChanged.RemoveListener(HandleMapNameChanged);
            
            if (descriptionTextbox)
                descriptionTextbox.onValueChanged.RemoveListener(HandleDescriptionChanged);
        }
        
        #endregion
        
        
        #region Event handlers

        /// <summary>
        /// Called when the window shows or hides. Update the textboxes on show.
        /// </summary>
        /// <param name="window"></param>
        /// <param name="targetState"></param>
        /// <param name="instant"></param>
        private void HandleWindowTransition(UIWindow window, UIWindow.VisualState targetState, bool instant)
        {
            if (targetState == UIWindow.VisualState.Shown)
            {
                mapNameTextbox.text = Map.Current.Name;
                descriptionTextbox.text = Map.Current.Description;
                authorLabel.text = Map.Current.Author;
                dateCreatedLabel.text = Map.Current.DateCreated.ToLocalTime().ToString("g");
                dateSavedLabel.text = Map.Current.DateSaved > Map.Current.DateCreated
                    ? Map.Current.DateSaved.ToLocalTime().ToString("g")
                    : "never";
            }
        }

        /// <summary>
        /// Called when the value in the map name textbox has changed. Update the Map object.
        /// </summary>
        /// <param name="newValue"></param>
        private void HandleMapNameChanged(string newValue)
        {
            Map.Current.Name = newValue;
        }

        /// <summary>
        /// Called when the value in the description textbox has changed. Update the Map object.
        /// </summary>
        /// <param name="newValue"></param>
        private void HandleDescriptionChanged(string newValue)
        {
            Map.Current.Description = newValue;
        }
        
        #endregion
        
    }
}
