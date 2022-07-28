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
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DuloGames.UI;
using TT.Shared.GameContent;

namespace TT.UI.MapEditor
{
    /// <summary>
    /// Attached to a category item in the scatter area list.
    /// </summary>
    public class ScatterCategory : MonoBehaviour
    {

        #region Editor fields
#pragma warning disable IDE0044 // Make fields read-only

        [SerializeField][Tooltip("The text element that shows the category name.")] private TMP_Text categoryName;
        [SerializeField][Tooltip("The toggle that defines whether this category is enabled or not.")] private Toggle categoryToggle;
        [SerializeField][Tooltip("The button that should toggle the detail panel visibility.")] private Button showPanelButton;
        [SerializeField][Tooltip("The image that shows the open/close state of the detail panel.")] private Image showPanelImage;
        [SerializeField][Tooltip("The sprite to show when the button will open the detail panel.")] private Sprite openPanelSprite;
        [SerializeField][Tooltip("The sprite to show when the button will close the detail panel.")] private Sprite closePanelSprite;
        [SerializeField][Tooltip("The panel containing the detail options for this category.")] private FlexibleWidthGridLayout detailPanel;
        [SerializeField][Tooltip("The label that will show how many items are selected.")] private Text selectedLabel;
        [SerializeField][Tooltip("The slider that defines the scatter density for this category.")] private UISliderExtended densitySlider;
        [SerializeField][Tooltip("A prefab for the sub categories")] private ScatterSubcategory subCategoryPrefab;
        [SerializeField][Tooltip("The transform that parents all inactive UI elements.")] private Transform inactiveUIElements;

#pragma warning restore IDE0044
        #endregion


        #region Private fields

        private const string LABEL_TEXT = "{0} of {1} selected";
        private readonly List<Toggle> _toggles = new List<Toggle>();
        private readonly List<ScatterSubcategory> _subcategories = new List<ScatterSubcategory>();

        #endregion


        #region Public properties

        /// <summary>
        /// The selected density for this category.
        /// </summary>
        public float Density { get => densitySlider.value; }

        #endregion


        #region Lifecycle events
#pragma warning disable IDE0051 // Unused members

        void Start()
        {
            showPanelButton.onClick.AddListener(HandleButtonClicked);
        }

        void Update()
        {
            selectedLabel.text = string.Format(LABEL_TEXT, _toggles.Where(x => x.isOn).Count(), _toggles.Count);
        }

#pragma warning restore IDE0051 // Unused members
        #endregion


        #region Event handlers

        /// <summary>
        /// Called when the panel toggle button is clicked. Show or hide the panel and update the image.
        /// </summary>
        private void HandleButtonClicked()
        {
            var detailPanelObject = detailPanel.gameObject;
            detailPanel.gameObject.SetActive(!detailPanelObject.activeSelf);
            showPanelImage.sprite = detailPanelObject.activeSelf ? closePanelSprite : openPanelSprite;
        }

        #endregion


        #region Public methods

        public void Initialise(ContentItemCategory category)
        {
            categoryName.text = category.Name;
            densitySlider.minValue = category.MinDensity;
            densitySlider.maxValue = category.MaxDensity;
            densitySlider.value = (category.MinDensity + category.MaxDensity) / 2;

            // Clear the list first
            while (detailPanel.transform.childCount > 0)
            {
                var child = detailPanel.transform.GetChild(0);
                child.gameObject.SetActive(false);
                child.SetParent(inactiveUIElements);
            }

            // Then re-populate
            foreach (var subcategory in category.Categories)
            {
                var subcategoryPanel =
                    Helpers.GetAvailableButton(subCategoryPrefab, _subcategories, inactiveUIElements);
                subcategoryPanel.Initialise(subcategory);
                subcategoryPanel.gameObject.SetActive(true);
                subcategoryPanel.transform.SetParent(detailPanel.transform);
                _toggles.Add(subcategoryPanel.GetComponentInChildren<Toggle>());
                _subcategories.Add(subcategoryPanel);
            }
        }

        /// <summary>
        /// Gets a list of addresses based on the selected items under this category.
        /// </summary>
        /// <returns></returns>
        public List<ContentItemCategory> GetCategories()
        {
            List<ContentItemCategory> categories = new List<ContentItemCategory>();

            if (categoryToggle.isOn)
            {
                // Make a list of all addresses from the scatter items and return them as an array
                var scatterSubCategories = detailPanel.GetComponentsInChildren<ScatterSubcategory>();
                foreach (var scatterSubCategory in scatterSubCategories)
                {
                    if (scatterSubCategory.Category != null)
                    {
                        categories.Add(scatterSubCategory.Category);
                    }
                }
            }

            return categories;
        }

        #endregion

   }
}