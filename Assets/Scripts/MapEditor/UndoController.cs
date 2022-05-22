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

using System;
using System.Collections.Generic;
using System.Linq;
using TT.Data;
using TT.InputMapping;
using TT.World;
using UnityEngine;

namespace TT.MapEditor
{
    public class UndoController : MonoBehaviour
    {
        #region Private fields

        private static readonly List<UndoAction> UndoActions = new List<UndoAction>();
        private static readonly List<UndoAction> RedoActions = new List<UndoAction>();
        private static readonly int MaxActionNum = 100;

        #endregion


        #region Lifecycle events
#pragma warning disable IDE0051 // Unused members

        void Update()
        {
            if (InputMapper.Current.WorldObjectInput.Undo)
            {
                Undo();
            }
            else if (InputMapper.Current.WorldObjectInput.Redo)
            {
                Redo();
            }
        }

#pragma warning restore IDE0051 // Unused members
        #endregion


        #region Public methods

        /// <summary>
        /// Adds an action to the undo list.
        /// </summary>
        /// <param name="action">The action that has been performed.</param>
        /// <param name="objectId">The Id of the WorldObjectBase this relates to.</param>
        /// <param name="value">The value before the undo action, i.e. the value to revert to on undo.</param>
        public static void RegisterAction(ActionType action, Guid objectId, object value)
        {
            // Add object, remove any items over the limit from the start of the list
            Debug.LogFormat("UndoController :: RegisterAction :: Registering undo action {0} on object {1} with value {2}", action.ToString(), objectId, (value != null) ? value.ToString() : "null");
            UndoActions.Add(new UndoAction() { Action = action, ObjectId = objectId, Value = value });
            while (UndoActions.Count > MaxActionNum)
            {
                Debug.LogFormat("UndoController :: RegisterAction :: List count {0} is over limit {1}, removing an item.", UndoActions.Count, MaxActionNum);
                UndoActions.RemoveAt(0);
            }

            // Clear the redo queue now the user has taken a new action
            RedoActions.Clear();
        }

        public static void RegisterRedoAction(ActionType action, Guid objectId, object value)
        {
            Debug.LogFormat("UndoController :: RegisterRedoAction :: Registering redo action {0} on object {1} with value {2}", action.ToString(), objectId, (value != null) ? value.ToString() : "null");
            RedoActions.Add(new UndoAction() { Action = action, ObjectId = objectId, Value = value });
        }

        /// <summary>
        /// Performs an undo the last performed action.
        /// </summary>
        public static async void Undo()
        {
            if (UndoActions.Count == 0) return;

            var undoIndex = UndoActions.Count - 1;
            var undoComplete = false;

            while (undoIndex >= 0 && !undoComplete)
            {
                // Get object this undo action applies to
                var worldObject = WorldObjectBase.All.Where(x => x.ObjectId.Equals(UndoActions[undoIndex].ObjectId)).FirstOrDefault();

                if (worldObject != null)
                {
                    if (WorldObjectBase.Current != worldObject) worldObject.SwitchSelection();

                    Debug.LogFormat("UndoController :: Undo :: Index {0} undoing action {1} on object {2} with value {3}", undoIndex, UndoActions[undoIndex].Action.ToString(), UndoActions[undoIndex].ObjectId, (UndoActions[undoIndex].Value != null) ? UndoActions[undoIndex].Value.ToString() : "null");

                    if (UndoActions[undoIndex].Value != null)
                    {
                        // Object exists, perform undo action
                        switch (UndoActions[undoIndex].Action)
                        {
                            case ActionType.Create:
                                RegisterRedoAction(ActionType.Create, Guid.Empty, worldObject.ToMapObject());
                                worldObject.Destroy();
                                break;
                            case ActionType.Name:
                                RegisterRedoAction(ActionType.Name, worldObject.ObjectId, worldObject.name);
                                worldObject.name = UndoActions[undoIndex].Value.ToString();
                                break;
                            case ActionType.Rotation:
                                RegisterRedoAction(ActionType.Rotation, worldObject.ObjectId,
                                    worldObject.LocalRotation);
                                var rotation = (Vector3) UndoActions[undoIndex].Value;
                                worldObject.RotateTo(rotation.y);
                                worldObject.RotateTo(rotation.x, SnapAxis.X);
                                worldObject.RotateTo(rotation.z, SnapAxis.Z);
                                break;
                            case ActionType.Elevation:
                                RegisterRedoAction(ActionType.Elevation, worldObject.ObjectId, worldObject.Position.y);
                                worldObject.ElevateTo((float) UndoActions[undoIndex].Value);
                                break;
                            case ActionType.Scale:
                                RegisterRedoAction(ActionType.Scale, worldObject.ObjectId,
                                    worldObject.transform.localScale.x);
                                worldObject.ScaleTo((float) UndoActions[undoIndex].Value);
                                break;
                            case ActionType.Replace:
                                RegisterRedoAction(ActionType.Replace, worldObject.ObjectId,
                                    new KeyValuePair<string, ContentItem>(worldObject.PrefabAddress,
                                        worldObject.ContentItem));
                                await WorldObjectFactory.Replace(worldObject,
                                    ((KeyValuePair<string, ContentItem>) UndoActions[undoIndex].Value).Value,
                                    ((KeyValuePair<string, ContentItem>) UndoActions[undoIndex].Value).Key);
                                break;
                            case ActionType.PlaceHandle:
                                if (worldObject is RamObject placeRamObject)
                                {
                                    RegisterRedoAction(ActionType.PlaceHandle, worldObject.ObjectId,
                                        new KeyValuePair<int, Vector3>((int) UndoActions[undoIndex].Value,
                                            placeRamObject.GetHandlePosition((int) UndoActions[undoIndex].Value)));
                                    placeRamObject.RemoveHandle((int) UndoActions[undoIndex].Value);
                                }
                                else if (worldObject is PolygonObject placePolygonObject)
                                {
                                    RegisterRedoAction(ActionType.PlaceHandle, worldObject.ObjectId,
                                        new KeyValuePair<int, Vector3>((int) UndoActions[undoIndex].Value,
                                            placePolygonObject.GetHandlePosition((int) UndoActions[undoIndex].Value)));
                                    placePolygonObject.RemoveHandle((int) UndoActions[undoIndex].Value);
                                }

                                break;
                            case ActionType.Move:
                                RegisterRedoAction(ActionType.Move, worldObject.ObjectId, worldObject.Position);
                                worldObject.MoveTo((Vector3) UndoActions[undoIndex].Value);
                                break;
                            case ActionType.MoveHandle:
                                var moveIndex = ((KeyValuePair<int, Vector3>) UndoActions[undoIndex].Value).Key;
                                var movePosition = ((KeyValuePair<int, Vector3>) UndoActions[undoIndex].Value).Value;
                                if (worldObject is RamObject moveRamObject)
                                {
                                    RegisterRedoAction(ActionType.MoveHandle, worldObject.ObjectId,
                                        new KeyValuePair<int, Vector3>(moveIndex,
                                            moveRamObject.GetHandlePosition(moveIndex)));
                                    moveRamObject.MoveTo(moveIndex, movePosition);
                                }
                                else if (worldObject is PolygonObject movePolygonObject)
                                {
                                    RegisterRedoAction(ActionType.MoveHandle, worldObject.ObjectId,
                                        new KeyValuePair<int, Vector3>(moveIndex,
                                            movePolygonObject.GetHandlePosition(moveIndex)));
                                    movePolygonObject.MoveTo(moveIndex, movePosition);
                                }

                                break;
                            case ActionType.Option:
                                var optionKey = ((KeyValuePair<WorldObjectOption, object>) UndoActions[undoIndex].Value)
                                    .Key;
                                var optionValue =
                                    ((KeyValuePair<WorldObjectOption, object>) UndoActions[undoIndex].Value).Value;
                                RegisterRedoAction(ActionType.Option, worldObject.ObjectId,
                                    new KeyValuePair<WorldObjectOption, object>(optionKey,
                                        worldObject.OptionValues[optionKey]));
                                worldObject.SetOption(optionKey, optionValue);
                                break;
                        }
                    }

                    UndoActions.RemoveAt(undoIndex);
                    undoComplete = true;
                }
                else if (UndoActions[undoIndex].Action == ActionType.Delete)
                {
                    Debug.LogFormat("UndoController :: Undo :: Index {0} undoing delete on object {1}", undoIndex, UndoActions[undoIndex].ObjectId);

                    // Item was deleted, re-create it
                    var rehydratedWorldObject = await WorldObjectFactory.CreateFromMapObject((MapObjectBase)UndoActions[undoIndex].Value);
                    rehydratedWorldObject.SwitchSelection();

                    RegisterRedoAction(ActionType.Delete, rehydratedWorldObject.ObjectId, null);
                    UndoActions.RemoveAt(undoIndex);

                    undoComplete = true;
                }
                else if (UndoActions[undoIndex].Action == ActionType.TerrainPaint)
                {
                    Debug.LogFormat("UndoController :: Undo :: Index {0} undoing terrain paint", undoIndex);

                    // Store the current state as a redo action
                    List<Texture2D> redoAlphaMaps = new List<Texture2D>();
                    var terrainData = Terrain.activeTerrain.terrainData;
                    var alphamapHeight = terrainData.alphamapHeight;
                    var alphamapWidth = terrainData.alphamapWidth;
                    for (int i = 0; i < terrainData.alphamapTextureCount; i++)
                    {
                        var newTexture = new Texture2D(alphamapWidth, alphamapHeight);
                        Graphics.CopyTexture(terrainData.GetAlphamapTexture(i), newTexture);
                        redoAlphaMaps.Add(newTexture);
                    }
                    // If terrain was painted, store the alpha maps for undo
                    var undoData = (PaintUndoData)UndoActions[undoIndex].Value;
                    var data = new PaintUndoData()
                    {
                        AlphamapTextures = redoAlphaMaps.ToArray(),
                        MinX = undoData.MinX,
                        MinY = undoData.MinY,
                        MaxX = undoData.MaxX,
                        MaxY = undoData.MaxY
                    };
                    RegisterRedoAction(ActionType.TerrainPaint, Guid.Empty, data);

                    GameTerrain.Current.LoadAlphamapTextures((PaintUndoData)UndoActions[undoIndex].Value);
                    UndoActions.RemoveAt(undoIndex);
                    undoComplete = true;
                }
                else
                {
                    // Object no longer exists - remove action and proceed up the list
                    Debug.LogFormat("UndoController :: Undo :: Object {0} not found at index {1}", UndoActions[undoIndex].ObjectId, undoIndex);

                    UndoActions.RemoveAt(undoIndex);
                    undoIndex--;
                }
            }
        }

        public static async void Redo()
        {
            if (RedoActions.Count == 0) return;

            var redoIndex = RedoActions.Count - 1;
            var worldObject = WorldObjectBase.All.Where(x => x.ObjectId.Equals(RedoActions[redoIndex].ObjectId)).FirstOrDefault();

            if (worldObject)
            {
                switch (RedoActions[redoIndex].Action)
                {
                    case ActionType.Delete:
                        UndoActions.Add(new UndoAction() { Action = ActionType.Delete, ObjectId = worldObject.ObjectId, Value = worldObject.ToMapObject() });
                        worldObject.Destroy();
                        break;
                    case ActionType.Name:
                        UndoActions.Add(new UndoAction() { Action = ActionType.Name, ObjectId = worldObject.ObjectId, Value = worldObject.name });
                        worldObject.name = RedoActions[redoIndex].Value.ToString();
                        break;
                    case ActionType.Rotation:
                        UndoActions.Add(new UndoAction() { Action = ActionType.Rotation, ObjectId = worldObject.ObjectId, Value = worldObject.LocalRotation });
                        var rotation = (Vector3)RedoActions[redoIndex].Value;
                        worldObject.RotateTo(rotation.y);
                        worldObject.RotateTo(rotation.x, SnapAxis.X);
                        worldObject.RotateTo(rotation.z, SnapAxis.Z);
                        break;
                    case ActionType.Elevation:
                        UndoActions.Add(new UndoAction() { Action = ActionType.Elevation, ObjectId = worldObject.ObjectId, Value = worldObject.Position.y });
                        worldObject.ElevateTo((float)RedoActions[redoIndex].Value);
                        break;
                    case ActionType.Scale:
                        UndoActions.Add(new UndoAction() { Action = ActionType.Scale, ObjectId = worldObject.ObjectId, Value = worldObject.transform.localScale.x });
                        worldObject.ScaleTo((float)RedoActions[redoIndex].Value);
                        break;
                    case ActionType.Replace:
                        UndoActions.Add(new UndoAction() { Action = ActionType.Replace, ObjectId = worldObject.ObjectId, Value = new KeyValuePair<string, ContentItem>(worldObject.PrefabAddress, worldObject.ContentItem) });
                        await WorldObjectFactory.Replace(worldObject, ((KeyValuePair<string, ContentItem>)RedoActions[redoIndex].Value).Value, ((KeyValuePair<string, ContentItem>)RedoActions[redoIndex].Value).Key);
                        break;
                    case ActionType.PlaceHandle:
                        if (worldObject is RamObject ramObject)
                        {
                            UndoActions.Add(new UndoAction() { Action = ActionType.PlaceHandle, ObjectId = worldObject.ObjectId, Value = ((KeyValuePair<int, Vector3>)RedoActions[redoIndex].Value).Key });
                            ramObject.AddHandle(((KeyValuePair<int, Vector3>)RedoActions[redoIndex].Value).Key, ((KeyValuePair<int, Vector3>)RedoActions[redoIndex].Value).Value);
                        }
                        else if (worldObject is PolygonObject polygonObject)
                        {
                            UndoActions.Add(new UndoAction() { Action = ActionType.PlaceHandle, ObjectId = worldObject.ObjectId, Value = ((KeyValuePair<int, Vector3>)RedoActions[redoIndex].Value).Key });
                            polygonObject.AddHandle(((KeyValuePair<int, Vector3>)RedoActions[redoIndex].Value).Key, ((KeyValuePair<int, Vector3>)RedoActions[redoIndex].Value).Value);
                        }
                        break;
                    case ActionType.Move:
                        UndoActions.Add(new UndoAction() { Action = ActionType.Move, ObjectId = worldObject.ObjectId, Value = worldObject.Position });
                        worldObject.MoveTo((Vector3)RedoActions[redoIndex].Value);
                        break;
                    case ActionType.MoveHandle:
                        var moveIndex = ((KeyValuePair<int, Vector3>)RedoActions[redoIndex].Value).Key;
                        var movePosition = ((KeyValuePair<int, Vector3>)RedoActions[redoIndex].Value).Value;
                        if (worldObject is RamObject moveRamObject)
                        {
                            UndoActions.Add(new UndoAction() { Action = ActionType.MoveHandle, ObjectId = worldObject.ObjectId, Value = new KeyValuePair<int, Vector3>(moveIndex, moveRamObject.GetHandlePosition(moveIndex))});
                            moveRamObject.MoveTo(moveIndex, movePosition);
                        }
                        else if (worldObject is PolygonObject movePolygonObject)
                        {
                            UndoActions.Add(new UndoAction() { Action = ActionType.MoveHandle, ObjectId = worldObject.ObjectId, Value = new KeyValuePair<int, Vector3>(moveIndex, movePolygonObject.GetHandlePosition(moveIndex))});
                            movePolygonObject.MoveTo(moveIndex, movePosition);
                        }
                        break;
                    case ActionType.Option:
                        var optionKey = ((KeyValuePair<WorldObjectOption, object>)RedoActions[redoIndex].Value).Key;
                        var optionValue = ((KeyValuePair<WorldObjectOption, object>)RedoActions[redoIndex].Value).Value;
                        UndoActions.Add(new UndoAction() { Action = ActionType.Option, ObjectId = worldObject.ObjectId, Value = new KeyValuePair<WorldObjectOption, object>(optionKey, worldObject.OptionValues[optionKey]) });
                        worldObject.SetOption(optionKey, optionValue);
                        break;
                }
            }
            else if (RedoActions[redoIndex].Action == ActionType.Create)
            {
                var rehydratedWorldObject = await WorldObjectFactory.CreateFromMapObject((MapObjectBase)RedoActions[redoIndex].Value);
                rehydratedWorldObject.SwitchSelection();
                UndoActions.Add(new UndoAction() { Action = ActionType.Create, ObjectId = rehydratedWorldObject.ObjectId, Value = null });
            }
            else if (RedoActions[redoIndex].Action == ActionType.TerrainPaint)
            {
                // Store the current state as a redo action
                List<Texture2D> undoAlphaMaps = new List<Texture2D>();
                var terrainData = Terrain.activeTerrain.terrainData;
                var alphamapHeight = terrainData.alphamapHeight;
                var alphamapWidth = terrainData.alphamapWidth;
                for (int i = 0; i < terrainData.alphamapTextureCount; i++)
                {
                    var newTexture = new Texture2D(alphamapWidth, alphamapHeight);
                    Graphics.CopyTexture(terrainData.GetAlphamapTexture(i), newTexture);
                    undoAlphaMaps.Add(newTexture);
                }
                // If terrain was painted, store the alpha maps for undo
                var redoData = (PaintUndoData)RedoActions[redoIndex].Value;
                var data = new PaintUndoData()
                {
                    AlphamapTextures = undoAlphaMaps.ToArray(),
                    MinX = redoData.MinX,
                    MinY = redoData.MinY,
                    MaxX = redoData.MaxX,
                    MaxY = redoData.MaxY
                };
                UndoActions.Add(new UndoAction() { Action = ActionType.TerrainPaint, ObjectId = Guid.Empty, Value = data });

                GameTerrain.Current.LoadAlphamapTextures((PaintUndoData)RedoActions[redoIndex].Value);
            }

            RedoActions.RemoveAt(redoIndex);
        }


        #endregion

        /// <summary>
        /// Internal representation of an undo action. Only used to stuff the action in a list.
        /// </summary>
        private struct UndoAction
        {
            public ActionType Action;
            public Guid ObjectId;
            public object Value;
        }
    }
}