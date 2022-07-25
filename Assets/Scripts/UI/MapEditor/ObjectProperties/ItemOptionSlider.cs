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
using DuloGames.UI;
using TMPro;
using TT.Data;
using TT.MapEditor;
using TT.Shared.World;
using TT.World;
using UnityEngine;
using UnityEngine.Serialization;

namespace TT.UI.MapEditor.ObjectProperties
{
    public class ItemOptionSlider : PropertySlider
    {
        #region Editor fields
#pragma warning disable IDE0044 // Make fields read-only

        [SerializeField][Tooltip("The slider controlled by this option.")] private UISliderExtended slider;
        [FormerlySerializedAs("InitialiseFromEditor")] [SerializeField] private bool initialiseFromEditor;
        [SerializeField] private WorldObjectOption option;
        [SerializeField][Tooltip("The Text field to display the heading text.")] private TMP_Text headerText;
        [SerializeField][Tooltip("Whether to snap to whole numbers (true) or allow fractional numbers (false).")] private bool wholeNumbers;

#pragma warning restore IDE0044
        #endregion


        #region Private fields

        private bool _initialised;

        #endregion


        #region Lifecycle events
#pragma warning disable IDE0051 // Unused members

        protected override void Start()
        {
            slider.onValueChanged.AddListener(UpdateFromSlider);

            // Initialise from editor fields rather than code
            if (initialiseFromEditor)
            {
                Initialise(option, headerText.text, minValue, maxValue, wholeNumbers);
            }

            base.Start();
        }

#pragma warning restore IDE0051 // Unused members
        #endregion


        #region Public methods

        public void Initialise(WorldObjectOption controlledOption, string header, float sliderMinValue, float sliderMaxValue, bool sliderWholeNumbers)
        {
            headerText.text = header;
            this.option = controlledOption;

            UpdatedThisFrame = true;

            this.minValue = sliderMinValue;
            this.maxValue = sliderMaxValue;
            this.wholeNumbers = sliderWholeNumbers;

            slider.minValue = sliderMinValue;
            slider.maxValue = sliderMaxValue;
            slider.wholeNumbers = sliderWholeNumbers;

            _initialised = true;
        }

        protected override float GetWorldObjectValue()
        {
            if (!_initialised) return 0;

            // Get property from world object
            if (WorldObjectBase.Current)
            {
                if (WorldObjectBase.Current.OptionValues.ContainsKey(option))
                {
                    return (float)WorldObjectBase.Current.OptionValues[option];
                }
            }

            return 0;
        }

        protected override float ConvertSliderToWorldObject(float sliderValue)
        {
            // No conversion required
            return sliderValue;
        }

        protected override float ConvertWorldObjectToSlider(float worldObjectValue)
        {
            // No conversion required
            return worldObjectValue;
        }

        protected override void UpdateSlider(float sliderValue)
        {
            if (!_initialised) return;

            // Set the slider's value
            slider.value = sliderValue;
        }

        protected override void UpdateWorldObject(float worldObjectValue)
        {
            if (!_initialised) return;

            // Set the option on the world object
            if (WorldObjectBase.Current)
            {
                WorldObjectBase.Current.SetOption(option, worldObjectValue);
            }
        }

        protected override void SetWholeNumbers(bool snapToGrid)
        {
            // Apply the whole numbers property
            slider.wholeNumbers = wholeNumbers;
        }

        protected override void RegisterUndo(float undoValue)
        {
            if (WorldObjectBase.Current == null) return;
            UndoController.RegisterAction(ActionType.Option, WorldObjectBase.Current.ObjectId, new KeyValuePair<WorldObjectOption, object>(option, undoValue));
        }

        #endregion

    }
}