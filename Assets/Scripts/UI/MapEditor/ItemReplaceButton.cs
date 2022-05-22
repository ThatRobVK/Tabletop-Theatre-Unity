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

using System;
using TT.MapEditor;
using TT.World;
using UnityEngine;
using UnityEngine.UI;

namespace TT.UI.MapEditor
{
    public class ItemReplaceButton : MonoBehaviour
    {
        #region Events

        public Action OnReplace;

        #endregion


        #region Editor fields
        #pragma warning disable IDE0044

        [SerializeField] private Image icon;
        [SerializeField] private Toggle toggle;

        #pragma warning restore IDE0044
        #endregion


        #region Public properties

        public string PrefabAddress { get; private set; }

        #endregion


        #region Private fields

        private bool _automatedUpdate;

        #endregion
        

        #region Lifecycle events
#pragma warning disable IDE0051 // Unused members

        void Update()
        {
            if (WorldObjectBase.Current is RamObject)
            {
                _automatedUpdate = true;
                toggle.isOn = WorldObjectBase.Current.PrefabAddress.Equals(PrefabAddress);
                toggle.interactable = !toggle.isOn;
            }
        }

        void LateUpdate()
        {
            _automatedUpdate = false;
        }

#pragma warning restore IDE0051 // Unused members
        #endregion


        #region Event handlers

        /// <summary>
        /// Called when the toggle is changed. Hooked up via the editor.
        /// </summary>
        /// <param name="toggled">The new state of the toggle.</param>
        public void OnToggleChanged(bool toggled)
        {
            if (_automatedUpdate) return;

            if (toggled)
            {
                // If this was toggled on, replace the world object
                ReplaceWorldObject();
            }

            // Don't allow the user to switch off the toggle
            toggle.interactable = !toggled;
        }

        #endregion


        #region Public methods

        /// <summary>
        /// Initialise this button for the specified content item.
        /// </summary>
        /// <param name="prefab">The content item to instantiate when this button is clicked.</param>
        /// <param name="toggleGroup">The group to join the toggle to, to prevent multiple buttons from being enabled.</param>
        /// <param name="parent">The transform to join this button to once initialised.</param>
        public void Initialise(string prefab, ToggleGroup toggleGroup, Transform parent)
        {
            PrefabAddress = prefab;

            LoadSprite();

            toggle.onValueChanged.RemoveListener(OnToggleChanged);
            toggle.group = toggleGroup;
            toggle.isOn = false;
            toggle.interactable = true;
            toggle.onValueChanged.AddListener(OnToggleChanged);

            transform.SetParent(parent);
        }

        #endregion


        #region Private methods

        /// <summary>
        /// Enter placement mode based on this button's content item.
        /// </summary>
        private void ReplaceWorldObject()
        {
            OnReplace?.Invoke();

            UndoController.RegisterAction(ActionType.Replace, WorldObjectBase.Current.ObjectId, WorldObjectBase.Current.PrefabAddress);            

            // If an object is selected, deselect it before spawning a new one
            WorldObjectFactory.Replace(WorldObjectBase.Current, null, PrefabAddress).ConfigureAwait(false);
        }

        /// <summary>
        /// Loads the sprite at previewAddress and sets it on the image.
        /// </summary>
        private async void LoadSprite()
        {
            // Load the preview sprite for the item number within the content item
            var sprites = await Helpers.LoadAddressables<Sprite>(new[] { PrefabAddress });

            if (sprites == null || sprites.Count == 0)
            {
                Debug.LogErrorFormat("ItemReplaceButton :: LoadSprite :: Addressables did not return a sprite for [{0}]", PrefabAddress);
                return;
            }

            icon.sprite = sprites[0];
        }

        #endregion
    }
}