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

using System.Collections;
using HighlightPlus;
using TT.Data;
using UnityEngine;

namespace TT.World
{
    public class WorldObjectHighlighter : MonoBehaviour
    {
        #region Static members

        private static WorldObjectHighlighter _highlighted;
        private static WorldObjectHighlighter _selected;


        public static void Select(WorldObjectBase worldObject)
        {
            if (_selected)
                _selected.StopHighlight(true);
            
            _selected = worldObject != null ? worldObject.GetComponent<WorldObjectHighlighter>() : null;
            
            if (_selected)
                _selected.Highlight(true);
        }

        public static void Highlight(WorldObjectBase worldObject)
        {
            if (_highlighted)
                _highlighted.StopHighlight();
            
            _highlighted = worldObject != null ? worldObject.GetComponent<WorldObjectHighlighter>() : null;
            
            if (_highlighted)
                _highlighted.Highlight(WorldObjectBase.Current == worldObject);
        }

        public static void Denied(WorldObjectBase worldObject)
        {
            if (worldObject)
            {
                var highlighter = worldObject.GetComponent<WorldObjectHighlighter>();
                highlighter.StartCoroutine(highlighter.ShowDenied());
            }
        }

        #endregion


        #region Private fields

        private SettingsObject _settings;
        private HighlightPlus.HighlightEffect _highlighter;
        private WorldObjectBase _worldObjectBase;
        private WorldObjectBase WorldObjectBase
        {
            get
            {
                // Lazy loading of the world object to avoid fragility in the order of creating the components on new objects
                if (_worldObjectBase == null)
                {
                    _worldObjectBase = GetComponent<WorldObjectBase>();

                    if (_worldObjectBase == null)
                    {
                        Debug.LogWarningFormat("No WorldObjectBase found on {0}", gameObject.name);
                    }
                }

                return _worldObjectBase;
            }
        }

        #endregion


        #region Lifecycle events
#pragma warning disable IDE0051 // Unused members

        private void OnEnable()
        {
            _highlighter = GetComponent<HighlightPlus.HighlightEffect>();
            _settings = FindObjectOfType<SettingsObject>();            

            if (_highlighter == null)
            {
                Debug.LogWarningFormat("No highlighter found on {0}", gameObject.name);
            }
            if (_settings == null)
            {
                Debug.LogWarning("No settings object found.");
            }
        }

#pragma warning restore IDE0051 // Unused members
        #endregion


        #region Private methods

        private void Highlight(bool selected = false)
        {
            // If new selection, OR already selected, then highlight with selection highlighter, otherwise hover highlighter
            _highlighter.ProfileLoad(selected || WorldObjectBase.Current == WorldObjectBase ? _settings.editorSettings.selectedHighlightProfile : _settings.editorSettings.hoverHighlightProfile);
            
            // For polygon objects, only highlight the base mesh, otherwise highlight the whole object 
            _highlighter.effectGroup = GetComponent<WorldObjectBase>() is PolygonObject 
                ? TargetOptions.OnlyThisObject 
                : TargetOptions.RootToChildren;
                
            _highlighter.highlighted = true;
        }

        private void StopHighlight(bool deselected = false)
        {
            // Occasionally this gets called after being destroyed - don't do anything
            if (this == null) return;

            // If not selected, or told to deselect, remove the highlighting
            if (WorldObjectBase.Current != WorldObjectBase || deselected)
            {
                _highlighter.highlighted = false;
            }
        }

        private IEnumerator ShowDenied()
        {
            _highlighter.ProfileLoad(_settings.editorSettings.deniedHighlightProfile);

            // For polygon objects, only highlight the base mesh, otherwise highlight the whole object 
            _highlighter.effectGroup = GetComponent<WorldObjectBase>() is PolygonObject 
                ? TargetOptions.OnlyThisObject 
                : TargetOptions.RootToChildren;

            _highlighter.highlighted = true;

            yield return new WaitForSeconds(0.5f);

            _highlighter.highlighted = false;
        }

        #endregion

    }
}