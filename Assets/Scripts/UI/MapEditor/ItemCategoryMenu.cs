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

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TT.Data;
using TT.Shared.World;
using TT.State;

namespace TT.UI.MapEditor
{
    public class ItemCategoryMenu : MonoBehaviour
    {
        #region Editor fields

        [SerializeField] private VerticalLayoutGroup categoriesGrid;
        [SerializeField] private VerticalLayoutGroup subCategoriesGrid;
        [SerializeField] private ItemButtonGrid itemsGrid;
        [SerializeField] private CategoryButton categoryButtonPrefab;
        [SerializeField] private Transform inactiveUIElementParent;
        [SerializeField] private WorldObjectType objectType;

        #endregion


        #region Private fields

        private readonly List<CategoryButton> _categoryButtons = new List<CategoryButton>();

        #endregion


        #region Lifecycle events

        void Start()
        {
            Content.OnContentChanged += HandleContentChanged;

            // Stop if no content loaded
            if (Content.Current != null) HandleContentChanged();
        }

        void OnEnable()
        {
            // Reset the buttons when switching to this panel
            if (Content.ContentLoaded) HandleContentChanged();
        }

        void OnDisable()
        {
            // Clearing on disable stops bug #266 where selected categories didn't deselect completely
            Clear(categories: true, subCategories: true, items: true, placementState: true);
        }

        #endregion


        #region Event handlers

        private void HandleContentChanged()
        {
            Clear(categories: true, subCategories: true, items: true, placementState: true);

            var categories = Content.GetCategoryByType(objectType);

            if (categories.Length > 0)
            {
                var toggleGroup = categoriesGrid.GetComponentInChildren<ToggleGroup>();

                // Add sub category buttons
                for (int i = 0; i < categories.Length; i++)
                {
                    // Init the button and add it to the grid
                    var button = GetAvailableButton(categoryButtonPrefab, _categoryButtons);
                    button.Initialise(categories[i], toggleGroup, categoriesGrid.transform);
                    button.OnClick += HandleCategoryClicked;
                }
            }
            else
            {
                var contentItems = Content.GetItemsByType(objectType);
                itemsGrid.InitButtons(contentItems);
            }
        }

        /// <summary>
        /// Called when a subcategory button is clicked. Populate the item categories for this subcategory.
        /// </summary>
        /// <param name="clickedButton">The button that was clicked.</param>
        private void HandleCategoryClicked(CategoryButton clickedButton)
        {
            var buttonToggle = clickedButton.GetComponent<Toggle>();
            if (buttonToggle.isOn)
            {
                // If toggled on, show the next level
                var category = clickedButton.Category;
                if (category.Categories.Length > 0)
                {
                    // Category has sub-categories
                    Clear(categories: false, subCategories: true, items: true, placementState: true);
                    var toggleGroup = subCategoriesGrid.GetComponentInChildren<ToggleGroup>();
                    for (int i = 0; i < category.Categories.Length; i++)
                    {
                        var button = GetAvailableButton(categoryButtonPrefab, _categoryButtons);
                        button.Initialise(category.Categories[i], toggleGroup, subCategoriesGrid.transform);
                        button.OnClick += HandleCategoryClicked;
                    }
                }
                else
                {
                    // Category has items
                    Clear(categories: false, subCategories: false, items: true, placementState: true);
                    itemsGrid.InitButtons(category);
                }
            }
            else
            {
                // If toggled off, clear the next level
                if (clickedButton.Category.Items.Length > 0) Clear(categories: false, subCategories: false, items: true, placementState: true);
                if (clickedButton.Category.Categories.Length > 0) Clear(categories: false, subCategories: true, items: true, placementState: true);
            }
        }

        #endregion


        #region Private methods

        /// <summary>
        /// Moves all children from the target transform to the inactive UI elements transform.
        /// </summary>
        /// <param name="targetTransform">The transform whose children to remove.</param>
        private void ClearTransform(Transform targetTransform)
        {
            while (targetTransform.childCount > 0)
            {
                var button = targetTransform.GetChild(0);
                if (button.GetComponent<CategoryButton>() is { } catButton)
                {
                    catButton.OnClick -= HandleCategoryClicked;
                }
                button.SetParent(inactiveUIElementParent);
            }
        }

        /// <summary>
        /// Finds an available button in the collection. When none are found, a new one is instantiated, added to the collection and returned.
        /// </summary>
        /// <typeparam name="T">Any object that is a UnityEngine.MonoBehaviour.</typeparam>
        /// <param name="prefab">The prefab to instantiate if no available buttons are found.</param>
        /// <param name="collection">A collection to be searched for an available button.</param>
        /// <returns>A button of type T that is available for use.</returns>
        private T GetAvailableButton<T>(T prefab, List<T> collection)
        {
            for (int i = 0; i < collection.Count; i++)
            {
                if ((collection[i] as MonoBehaviour)?.transform.parent == inactiveUIElementParent)
                {
                    return collection[i];
                }
            }

            var genericPrefab = prefab as MonoBehaviour;
            var genericButton = Instantiate(genericPrefab);
            var newButton = genericButton.GetComponent<T>();
            collection.Add(newButton);
            return newButton;
        }

        /// <summary>
        /// Removes all items from the specified grids and optionally cancels placement mode.
        /// </summary>
        /// <param name="categories">If true, the subcategories grid is cleared.</param>
        /// <param name="subCategories">If true, the item categories grid is cleared.</param>
        /// <param name="items">If true, the items grid is cleared.</param>
        /// <param name="placementState">If true and if the editor is in placement mode, it is cancelled.</param>
        private void Clear(bool categories, bool subCategories, bool items, bool placementState)
        {
            // Clear transforms
            if (categories && categoriesGrid) ClearTransform(categoriesGrid.transform);
            if (subCategories && subCategoriesGrid) ClearTransform(subCategoriesGrid.transform);
            if (items && itemsGrid) ClearTransform(itemsGrid.transform);

            // Cancel placement mode
            if (placementState && StateController.CurrentState != null && StateController.CurrentState.IsPlacementState)
            {
                StateController.CurrentState.ToIdle();
            }

        }
        #endregion

    }
}