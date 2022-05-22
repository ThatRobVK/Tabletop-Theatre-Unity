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
#pragma warning disable IDE0083 // "Use patern matching" - not supported in the .NET version used by Unity 2020.3

using System;
using System.Collections.Generic;
using System.Linq;
using DuloGames.UI;
using TT.Data;
using TT.InputMapping;
using TT.MapEditor;
using TT.UI;
using TT.World;
using UnityEngine;

namespace TT.State
{
    public class EditorIdleState : StateBase
    {
        private bool _changingToPlacement;
        private WorldObjectBase _currentWorldObject;
        private readonly List<DraggableObject> _draggableObjects = new List<DraggableObject>();
        private readonly List<WorldObjectType> _editableTypes = new List<WorldObjectType>();
        private readonly List<WorldObjectType> _notEditableTypes = new List<WorldObjectType>();
        private Vector3 _undoValue;

        public override bool IsPlacementState => false;

        public EditorIdleState(StateController stateController, List<WorldObjectType> types) : base(stateController)
        {
            _editableTypes = types ?? _editableTypes;

            // Make a list of all types that aren't editable
            var allValues = Enum.GetValues(typeof(WorldObjectType));
            foreach (var value in allValues)
            {
                if (!_editableTypes.Contains((WorldObjectType)value))
                {
                    _notEditableTypes.Add((WorldObjectType)value);
                }
            }
        }

        public override void Enable()
        {
            Debug.LogFormat("EditorIdleState({0}) :: Enable", string.Join(",", _editableTypes));

            _changingToPlacement = false;

            WorldObjectBase.All.Where(x => _editableTypes.Contains(x.Type)).ToList().ForEach(x => x.ShowControls());
        }

        public override void Disable()
        {
            Debug.LogFormat("EditorIdleState({0}) :: Disable", string.Join(",", _editableTypes));

            if (!_changingToPlacement)
            {
                // Hide controls and deselect objects - unless moving to placement state
                WorldObjectBase.All.Where(x => _editableTypes.Contains(x.Type)).ToList().ForEach(x => x.HideControls());
                if (WorldObjectBase.Current && _editableTypes.Contains(WorldObjectBase.Current.Type)) WorldObjectBase.Current.Deselect();
            }
        }

        public override void Update()
        {
            // If another object was selected, change state
            if (WorldObjectBase.Current && !_editableTypes.Contains(WorldObjectBase.Current.Type))
            {
                StateController.Current.ChangeState(TypeToIdleStateMap[WorldObjectBase.Current.Type]);
            }

            if (WorldObjectBase.Current && WorldObjectBase.Current != _currentWorldObject)
            {
                // Cache the draggable objects to prevent performance issues on objects with many children e.g. scatter areas
                _draggableObjects.Clear();
                _draggableObjects.AddRange(WorldObjectBase.Current.GetComponentsInChildren<DraggableObject>());
                _currentWorldObject = WorldObjectBase.Current;
            }

            // Check if any objects are dragging
            bool objectDragging = _draggableObjects.Count(x => x.IsDragging) > 0;

            // No highlighting etc when dragging or over UI
            if (!objectDragging && !Helpers.IsPointerOverUIElement())
            {
                var objectUnderPointer = Helpers.GetObjectAtMouse(_editableTypes, Helpers.SelectableMask, false);
                WorldObjectBase worldObjectUnderPointer = objectUnderPointer != null ? objectUnderPointer.GetComponentInParent<WorldObjectBase>() : null;

                if (worldObjectUnderPointer && Input.GetMouseButtonDown(0))
                {
                    // Click on a world object
                    worldObjectUnderPointer.Click(objectUnderPointer, Helpers.GetWorldPointFromMouse(Helpers.SelectableMask));
                }
                else if (worldObjectUnderPointer)
                {
                    // Hover over a world object
                    worldObjectUnderPointer.MouseOver(objectUnderPointer,
                        Helpers.GetWorldPointFromMouse(Helpers.SelectableMask));
                }
                else
                {
                    // Stop highlighting any previously highlighted objects
                    WorldObjectHighlighter.Highlight(null);

                    // See if we're hovering over an object that's not supported in the current mode
                    var notEditableObject =
                        Helpers.GetObjectAtMouse(_notEditableTypes, Helpers.SelectableMask, false);
                    WorldObjectBase notEditableWorldObject =
                        notEditableObject != null ? notEditableObject.GetComponentInParent<WorldObjectBase>() : null;
                    if (notEditableWorldObject)
                    {
                        // Show denied cursor, on click show denied highlight effect on the object
                        CursorController.Current.Denied = true;
                        if (Input.GetMouseButtonDown(0))
                        {
                            WorldObjectHighlighter.Denied(notEditableWorldObject);
                        }
                    }
                }
            }

            if (WorldObjectBase.Current != null)
            {
                if (WorldObjectBase.Current.IsDraggable)
                {
                    // Hook keyboard controls into undo
                    if (InputMapper.Current.WorldObjectInput.StartMove) _undoValue = WorldObjectBase.Current.Position;
                    if (InputMapper.Current.WorldObjectInput.StopMove) UndoController.RegisterAction(ActionType.Move, WorldObjectBase.Current.ObjectId, _undoValue);

                    // Apply keyboard movement
                    var movementAmount = InputMapper.Current.WorldObjectInput.GetMoveAmount(Helpers.Settings.editorSettings.snapToGrid);
                    if (movementAmount != Vector3.zero) WorldObjectBase.Current.Move(movementAmount);
                }
            }

            // On escape take one of these actions
            if (InputMapper.Current.WorldObjectInput.Cancel)
            {
                if (WorldObjectBase.Current)
                {
                    // If something is selected, deselect it
                    WorldObjectBase.Current.Deselect();
                }
                else if (_editableTypes.Count > 0)
                {
                    // If in a specific idle state, return to general idle state
                    StateController.Current.ChangeState(StateType.EditorIdleState);
                }
                else
                {
                    // If in general idle state and nothing selected, open the main menu
                    UIWindow.GetWindow(UIWindowID.GameMenu).Show();
                }
            }


        }

        public override void ToIdle()
        { }

        public override void ToPlacement()
        {
            _changingToPlacement = true;
            
            StateController.Current.ChangeState(StateType.ItemPlacement);
        }
    }
}