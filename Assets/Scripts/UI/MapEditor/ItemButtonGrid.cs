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
using System.Linq;
using DuloGames.UI;
using TT.Data;
using TT.World;
using UnityEngine;
using UnityEngine.UI;

namespace TT.UI.MapEditor
{
    [RequireComponent(typeof(GridLayoutGroup))]
    [RequireComponent(typeof(ToggleGroup))]
    public class ItemButtonGrid : MonoBehaviour
    {

        #region Editor fields
#pragma warning disable IDE0044 // Make fields read-only

        [SerializeField][Tooltip("A prefab for the item replace button.")] private ItemButton itemButtonPrefab;
        [SerializeField][Tooltip("The parent for UI objects that are temporarily not required.")] private Transform inactiveUIElementParent;

#pragma warning restore IDE0044
        #endregion


        #region Private fields

        private UIWindow _window;
        private readonly List<ItemButton> _itemButtons = new List<ItemButton>();
        private Transform _itemGrid;
        private ToggleGroup _toggleGroup;
        private ContentItemCategory _currentCategory;
        private ContentItem _currentContentItem;
        private WorldObjectBase _currentObject;
        private Toggle _randomiserToggle;

        #endregion


        #region Lifecycle events
#pragma warning disable IDE0051 // Unused members

        void Start()
        {
            _itemGrid = GetComponent<GridLayoutGroup>().transform;
            _toggleGroup = GetComponent<ToggleGroup>();
            _window = GetComponentInParent<UIWindow>();
        }

        void Update()
        {
            // Don't update if the window isn't visible
            if (!_window.IsVisible) return;

            // Allow switching off in placement mode, not in replace mode
            _toggleGroup.allowSwitchOff = WorldObjectBase.Current == null;

            if (WorldObjectBase.Current && WorldObjectBase.Current.ContentItem.Category == null && WorldObjectBase.Current.ContentItem != _currentContentItem)
            {
                // Different type of object selected with no category, initialise on the type of item
                _currentContentItem = WorldObjectBase.Current.ContentItem;
                _currentCategory = null;
                _currentObject = WorldObjectBase.Current;
                InitButtons(Content.GetItemsByType(WorldObjectBase.Current.Type));
            }
            else if (WorldObjectBase.Current && (WorldObjectBase.Current.ContentItem.Category != _currentCategory || WorldObjectBase.Current.ContentItem != _currentContentItem))
            {
                // Different category selected, initialise on the category
                _currentCategory = WorldObjectBase.Current.ContentItem.Category;
                _currentContentItem = WorldObjectBase.Current.ContentItem;
                _currentObject = WorldObjectBase.Current;
                InitButtons(_currentCategory);
            }
            else if (WorldObjectBase.Current && WorldObjectBase.Current != _currentObject)
            {
                // Different object selected, highlight its button
                _currentCategory = WorldObjectBase.Current.ContentItem.Category;
                _currentContentItem = WorldObjectBase.Current.ContentItem;
                _currentObject = WorldObjectBase.Current;
                _itemButtons.Where(x => x.PrefabAddress == _currentObject.PrefabAddress).FirstOrDefault()?.Select();
            }
            else if (_currentObject != null && !WorldObjectBase.Current)
            {
                // Deselected, clear the selected object
                _currentObject = null;
            }
        }

#pragma warning restore IDE0051 // Unused members
        #endregion

        public void InitButtons(ContentItemCategory category)
        {
            _currentCategory = category;

            var randomiserOn = _randomiserToggle != null && _randomiserToggle.isOn;

            // Remove all buttons
            while (_itemGrid.childCount > 0)
            {
                var button = _itemGrid.GetChild(0);
                button.SetParent(inactiveUIElementParent);
            }

            // Set up the button
            var randomiserButton = Helpers.GetAvailableButton(itemButtonPrefab, _itemButtons, inactiveUIElementParent);
            randomiserButton.InitialiseRandomiser(category, _toggleGroup, _itemGrid.transform, randomiserOn);
            _randomiserToggle = randomiserButton.GetComponent<Toggle>();

            // Add item buttons based on the content item and the items within it
            foreach (var contentItem in category.Items)
            {
                var selected = !randomiserOn && WorldObjectBase.Current != null && new List<string>(contentItem.IDs).Contains(WorldObjectBase.Current.PrefabAddress);

                // Set up the button
                var button = Helpers.GetAvailableButton(itemButtonPrefab, _itemButtons, inactiveUIElementParent);
                button.Initialise(contentItem, _toggleGroup, _itemGrid.transform, selected);
            }
        }

        public void InitButtons(ContentItem[] contentItems)
        {
            // This can be called before Start so ensure it's initialised properly
            if (_toggleGroup == null || _itemGrid == null) Start();

            // Remove all buttons
            while (_itemGrid.childCount > 0)
            {
                var button = _itemGrid.GetChild(0);
                button.SetParent(inactiveUIElementParent);
            }

            // Add item buttons based on the content item and the items within it
            foreach (var contentItem in contentItems)
            {
                var selected = WorldObjectBase.Current != null && new List<string>(contentItem.IDs).Contains(WorldObjectBase.Current.PrefabAddress);

                // Set up the button
                var button = Helpers.GetAvailableButton(itemButtonPrefab, _itemButtons, inactiveUIElementParent);
                button.Initialise(contentItem, _toggleGroup, _itemGrid.transform, selected);
            }

        }
    }
}