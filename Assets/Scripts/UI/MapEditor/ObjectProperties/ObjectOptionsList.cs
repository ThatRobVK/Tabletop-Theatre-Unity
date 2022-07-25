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

#pragma warning disable IDE0083 // "Use patern matching" - not supported in the .NET version used by Unity 2020.3

using System.Collections.Generic;
using DuloGames.UI;
using TT.Data;
using TT.Shared.World;
using TT.World;
using UnityEngine;

namespace TT.UI.MapEditor.ObjectProperties
{
    public class ObjectOptionsList : MonoBehaviour
    {

        [SerializeField][Tooltip("A prefab for the Lights option.")] private ItemOptionLights lightsPrefab;
        [SerializeField][Tooltip("A prefab for the Open/Close option.")] private ItemOptionBool openClosePrefab;
        [SerializeField][Tooltip("A prefab for a separator which is added at the end of the options.")] private GameObject separatorPrefab;
        [SerializeField][Tooltip("The transform that parents inactive UI elements")] private Transform inactiveUITransform;
        [Tooltip("A map of options to their display names.")] private readonly Dictionary<WorldObjectOption, string> _displayNames = new Dictionary<WorldObjectOption, string>()
        {
            { WorldObjectOption.OpenClose, "Open" },
            { WorldObjectOption.LightsMode, "Lights" }
        };

        private WorldObjectBase _currentObject;
        private UIWindow _window;

        void Start()
        {
            _window = GetComponentInParent<UIWindow>();
        }

        void Update()
        {
            // Don't update if not on a window or not visible
            if (_window == null || !_window.IsVisible) return;

            if (WorldObjectBase.Current != null && WorldObjectBase.Current != _currentObject)
            {
                _currentObject = WorldObjectBase.Current;

                // New item selected, initialise
                InitialiseOptions(WorldObjectBase.Current);
            }
        }


        private void InitialiseOptions(WorldObjectBase worldObject)
        {
            // Remove and destroy all options
            while (transform.childCount > 0)
            {
                var optionObject = transform.GetChild(0);
                optionObject.SetParent(inactiveUITransform);
                Destroy(optionObject.gameObject);
            }

            if (worldObject != null)
            {
                foreach (var option in worldObject.OptionValues)
                {
                    CreateOption(option.Key, option.Value, transform);
                }

                if (transform.childCount > 0 && separatorPrefab)
                {
                    // Add a separator under the options
                    Instantiate(separatorPrefab, transform);
                }
            }
        }


        /// <summary>
        /// Instantiate and initialise an option.
        /// </summary>
        /// <param name="option">The option to initialise.</param>
        /// <param name="value">The default value to show.</param>
        /// <param name="parentTransform">The transform to parent this object under.</param>
        /// <returns>An instantiated option prefab, as its base class.</returns>
        public void CreateOption(WorldObjectOption option, object value, Transform parentTransform)
        {
            switch (option)
            {
                case WorldObjectOption.OpenClose:
                    var openCloseOption = Instantiate(openClosePrefab, parentTransform);
                    openCloseOption.Initialise(option, _displayNames[option]);
                    break;

                case WorldObjectOption.LightsMode:
                    var lightsOption = Instantiate(lightsPrefab, parentTransform);
                    if (lightsOption != null) lightsOption.Initialise(option, _displayNames[option], value);
                    break;
            }
        }
    }
}