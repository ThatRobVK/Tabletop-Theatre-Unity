using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TT.UI.MainMenu
{
    [RequireComponent(typeof(ScrollRect))]
    public class LoadMapList : MonoBehaviour
    {
        [SerializeField] private MapListItem mapItemPrefab;
        [SerializeField] private Transform inactiveUIElements;
        
        private Transform _content;
        private List<MapListItem> _items = new List<MapListItem>();
        
        // Start is called before the first frame update
        private void Start()
        {
            _content = GetComponent<ScrollRect>().content.transform;

            // Listen for auth events
            Helpers.Comms.User.OnLoginSuccess += HandleLoginSuccess;
            Helpers.Comms.User.OnLogout += HandleLogout;
            
            // If already logged in, populate the list
            if (Helpers.Comms.User.IsLoggedIn) HandleLoginSuccess();
        }

        private void OnDestroy()
        {
            if (Helpers.Comms != null && Helpers.Comms.User != null)
            {
                // Stop listening for auth events
                Helpers.Comms.User.OnLoginSuccess += HandleLoginSuccess;
                Helpers.Comms.User.OnLogout += HandleLogout;
            }
        }

        /// <summary>
        /// Called when the user successfully logs in. Re-populate the list for this user.
        /// </summary>
        private void HandleLoginSuccess()
        {
            ClearList();
            PopulateListAsync();
        }

        /// <summary>
        /// Called when the user is logged out. Clear the list.
        /// </summary>
        private void HandleLogout()
        {
            ClearList();
        }

        /// <summary>
        /// Removes all items from the list by moving them to the inactive UI elements parent and deactivating them.
        /// </summary>
        private void ClearList()
        {
            while (_content.childCount > 0)
            {
                var child = _content.GetChild(0);
                child.gameObject.SetActive(false);
                child.SetParent(inactiveUIElements);
            }
        }

        /// <summary>
        /// Loads the map index and populates the list based on it.
        /// </summary>
        private async void PopulateListAsync()
        {
            var maps = await Helpers.Comms.UserContent.GetMapIndex();
            foreach (var map in maps)
            {
                var button = Helpers.GetAvailableButton<MapListItem>(mapItemPrefab, _items, inactiveUIElements);
                button.Initialise(map.name, DateTime.FromFileTimeUtc(map.dateSaved));
                button.transform.SetParent(_content);
            }
        }
    }
}
