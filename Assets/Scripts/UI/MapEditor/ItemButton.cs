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
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DuloGames.UI;
using TT.MapEditor;
using TT.Shared.GameContent;
using TT.State;
using TT.World;

namespace TT.UI.MapEditor
{
    public class ItemButton : MonoBehaviour, IPointerClickHandler
    {
        #region Private fields

        private ContentItemCategory _category;
        private ContentItem _contentItem;
        private Sprite _previewSprite;
        private ColorSchemeElement _colorSchemeElement;

        #endregion


        #region Editor fields

        [SerializeField] private Image icon;
        [SerializeField] private Toggle toggle;
        [SerializeField] private Button randomiseButton;
        [SerializeField] private Sprite randomiserSprite;
        [SerializeField] private Text itemIDText;

        #endregion


        #region Public properties

        public int ItemNumber { get; private set; }
        public string PrefabAddress { get; private set; }
        public bool Active { get; set; }
        
        #endregion


        #region Lifecycle events

        void OnEnable()
        {
            // Store the original sprite
            _previewSprite = icon.sprite;

            _colorSchemeElement = icon.GetComponent<ColorSchemeElement>();
        }

        void Update()
        {
            itemIDText.text = Helpers.Settings.editorSettings.showItemIDs ? PrefabAddress : string.Empty;

            // Don't update the toggle when in placement mode
            if (StateController.CurrentStateType != StateType.ItemPlacement)
            {
                toggle.isOn = WorldObjectBase.Current != null && WorldObjectBase.Current.ContentItem == _contentItem;
            }
        }
        
        #endregion


        #region Event handlers

        /// <summary>
        /// Called when the 'random' button is clicked. Restart placement to get a new random item.
        /// </summary>
        public void HandleRandomButtonClicked()
        {
            if (!toggle.isOn)
            {
                // Button pressed while toggle was off - switch on to start placement
                toggle.isOn = true;
            }
            else
            {
                // Destroy the current version being placed
                WorldObjectBase.Current.Destroy();

                // Button pressed while toggle was on - restart placement to get a new random item.
                StartPlacementMode();
            }
        }

        /// <summary>
        /// Called when the item button is clicked.
        /// </summary>
        /// <param name="eventData">Ignored</param>
        public void OnPointerClick(PointerEventData eventData)
        {
            if (toggle.isOn)
            {
                if (WorldObjectBase.Current)
                {
                    ReplaceObject();
                }
                else
                {
                    // If toggled on, start placement
                    StartPlacementMode();
                }
            }
            else
            {
                // Toggle turned off by user clicking
                if (StateController.CurrentStateType == StateType.NatureObjectPlacement)
                {
                    // If toggled off, cancel placement
                    CancelPlacementMode();
                }
            }
        }

        #endregion


        #region Public methods

        /// <summary>
        /// Initialise this button for the specified content item.
        /// </summary>
        /// <param name="item">The content item to instantiate when this button is clicked.</param>
        /// <param name="toggleGroup">The group to join the toggle to, to prevent multiple buttons from being enabled.</param>
        /// <param name="parent">The transform to join this button to once initialised.</param>
        /// <param name="selected">Whether to select the button by default.</param>
        public void Initialise(ContentItem item, ToggleGroup toggleGroup, Transform parent, bool selected = false)
        {
            _contentItem = item;
            PrefabAddress = _contentItem.IDs[0];

            LoadSprite();

            toggle.group = toggleGroup;
            toggle.isOn = selected;
            randomiseButton.gameObject.SetActive(false);
            transform.SetParent(parent);
        }

        /// <summary>
        /// Show this button as a randomiser for the specified category.
        /// </summary>
        /// <param name="category">The category to randomise items on.</param>
        /// <param name="toggleGroup">The group to join the toggle to, to prevent multiple buttons from being enabled.</param>
        /// <param name="parent">The transform to join this button to once initialised.</param>
        /// <param name="selected">Where to toggle the button on.</param>
        public void InitialiseRandomiser(ContentItemCategory category, ToggleGroup toggleGroup, Transform parent, bool selected = false)
        {
            this._category = category;
            _contentItem = null;
            PrefabAddress = string.Empty;

            LoadSprite();

            toggle.group = toggleGroup;
            toggle.isOn = selected;
            randomiseButton.gameObject.SetActive(true);
            transform.SetParent(parent);
        }

        public void Select()
        {
            toggle.isOn = true;
        }
        #endregion


        #region Private methods

        /// <summary>
        /// Exits placement mode if it is currently active.
        /// </summary>
        private void CancelPlacementMode()
        {
            StateController.CurrentState.ToIdle();
        }

        /// <summary>
        /// Enter placement mode based on this button's content item.
        /// </summary>
        private void StartPlacementMode()
        {
            // If an object is selected, deselect it before spawning a new one
            if (WorldObjectBase.Current)
                WorldObjectBase.Current.Deselect();

            StateController.CurrentState.ToPlacement();
            if (_contentItem != null)
            {
                (StateController.CurrentState as WorldObjectPlacementState)?.Initialise(_contentItem);
            }
            else
            {
                (StateController.CurrentState as WorldObjectPlacementState)?.Initialise(_category);
            }
        }

        /// <summary>
        /// Replace this object with another.
        /// </summary>
        private async void ReplaceObject()
        {
            if (!WorldObjectBase.Current) return;

            UndoController.RegisterAction(ActionType.Replace, WorldObjectBase.Current.ObjectId, new KeyValuePair<string, ContentItem>(WorldObjectBase.Current.PrefabAddress, WorldObjectBase.Current.ContentItem));

            ContentItem contentItemToPlace = _contentItem ?? _category.Items[Random.Range(0, _category.Items.Length)];
            ItemNumber = contentItemToPlace.IDs.Length > 1 ? Random.Range(0, contentItemToPlace.IDs.Length) : 0;
            await WorldObjectFactory.Replace(WorldObjectBase.Current, contentItemToPlace, contentItemToPlace.IDs[ItemNumber]);

            if (StateController.CurrentStateType == StateType.ItemPlacement)
            {
                // If we were placing the object when it was replaced, pick up the new object
                WorldObjectBase.Current.PickUp();
            }
        }

        /// <summary>
        /// Loads the sprite at previewAddress and sets it on the image.
        /// </summary>
        private async void LoadSprite()
        {
            icon.sprite = _previewSprite;
            ColorSchemeManager.Instance.activeColorScheme.ApplyToElement(_colorSchemeElement);

            if (_category != null)
            {
                icon.sprite = randomiserSprite;
            }
            else
            {
                // Load the preview sprite for the item number within the content item
                var previewAddress = _contentItem.IDs[0];
                var sprites = await Helpers.LoadAddressables<Sprite>(new[] { previewAddress });

                if (sprites == null || sprites.Count == 0)
                {
                    Debug.LogErrorFormat("ItemButton :: LoadSprite :: Addressables did not return a sprite for [{0}]", previewAddress);
                    return;
                }

                icon.sprite = sprites[0];
                _colorSchemeElement.Apply(Color.white);
            }
        }

        #endregion
    }
}