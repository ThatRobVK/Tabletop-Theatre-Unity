using DuloGames.UI;
using TT.Data;
using UnityEngine;

namespace TT.UI.MainMenu
{
    [RequireComponent(typeof(UISelectField))]
    public class LoadTerrainDropdown : MonoBehaviour
    {
        
        #region Private fields
        
        private UISelectField _selectField;
        
        #endregion
        
        
        #region Lifecycle events

        private void Start()
        {
            _selectField = GetComponent<UISelectField>();

            // Listen for auth and content events
            Helpers.Comms.User.OnLoginSuccess += HandleLoginSuccess;
            Content.OnContentChanged += HandleContentChanged;

            if (Helpers.Comms.User.IsLoggedIn && Content.ContentLoaded)
            {
                // Already logged in and content loaded - load the terrain options
                HandleContentChanged();
            }
            else if (Helpers.Comms.User.IsLoggedIn)
            {
                // Logged in but no content loaded - load the content
                HandleLoginSuccess();
            }
        }

        private void OnDestroy()
        {
            // Stop listening to auth events
            if (Helpers.Comms != null && Helpers.Comms.User != null)
                Helpers.Comms.User.OnLoginSuccess -= HandleLoginSuccess;
            
            // Stop listening for content events
            Content.OnContentChanged += HandleContentChanged;
        }
        
        #endregion
        
        
        #region Event handlers

        /// <summary>
        /// Called when the user logs in. Check content is loaded.
        /// </summary>
        private void HandleLoginSuccess()
        {
            if (!Content.ContentLoaded)
            {
                Content.Load().ConfigureAwait(false);
            }
            else
            {
                HandleContentChanged();
            }
        }
        
        /// <summary>
        /// Called when new content is loaded. Update the select field with the terrain options.
        /// </summary>
        private void HandleContentChanged()
        {
            // Add each terrain to the drop down
            _selectField.options.Clear();
            for (int i = 0; i < Content.Current.Combined.TerrainLayers.Length; i++)
                _selectField.AddOption(Content.Current.Combined.TerrainLayers[i].Name);

            _selectField.SelectOptionByIndex(0);
        }
        
        #endregion
        
    }
}
