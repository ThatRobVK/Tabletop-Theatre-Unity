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

using System.Threading.Tasks;
using TT.Data;
using TT.Shared.UserContent;
using UnityEngine;

namespace TT.World
{
    public static class WorldObjectFactory
    {
        private const string SCATTER_AREA_PREFAB = "ScatterArea";
        private const string RAM_OBJECT_PREFAB = "RamObject";

        #region Public methods

        /// <summary>
        /// Creates a world object based on a content item
        /// </summary>
        /// <param name="contentItem">The content representation of the object to create.</param>
        /// <param name="itemIndex">The index of the prefab within the content item to instantiate.</param>
        /// <returns>An asynchronous task that instantiates the object.</returns>
        public static async Task<WorldObjectBase> CreateFromContent(ContentItem contentItem, int itemIndex)
        {
            Debug.LogFormat("WorldObjectFactory :: Create({0})", itemIndex);
            
            WorldObjectBase worldObject;

            if (contentItem.Type == WorldObjectType.ScalableObject)
            {
                var gameObject = await Helpers.InstantiateAddressable("/General/Prefabs/FloorZone");
                worldObject = gameObject.GetComponent<ScalableObject>();
                AddStandardComponents(gameObject);
                AddDraggableComponent(gameObject);
                worldObject.Initialise(contentItem, itemIndex);
            }
            else if (contentItem.Type == WorldObjectType.River || contentItem.Type == WorldObjectType.Road)
            {
                var gameObject = await Helpers.InstantiateAddressable(RAM_OBJECT_PREFAB);
                gameObject.transform.position = Vector3.zero;
                worldObject = gameObject.GetComponent<RamObject>();
                AddStandardComponents(gameObject);
                worldObject.Initialise(contentItem, itemIndex);
            }
            else if (contentItem.Type == WorldObjectType.ScatterArea)
            {
                var gameObject = await Helpers.InstantiateAddressable(SCATTER_AREA_PREFAB);
                gameObject.transform.position = Vector3.zero;
                AddStandardComponents(gameObject);
                worldObject = gameObject.GetComponent<PolygonObject>();
                worldObject.Initialise(contentItem, itemIndex);
            }
            else
            {
                var gameObject = await Helpers.InstantiateAddressable(contentItem.IDs[itemIndex]);
                worldObject = gameObject.AddComponent<WorldObject>();
                AddStandardComponents(gameObject);
                AddDraggableComponent(gameObject);
                worldObject.Initialise(contentItem, itemIndex);
            }

            return worldObject;
        }

        /// <summary>
        /// Creates an object from the specified map object.
        /// </summary>
        /// <param name="mapObject">A base map object.</param>
        /// <returns>An instance of the world object, as its base class.</returns>
        public static async Task<WorldObjectBase> CreateFromMapObject(BaseObjectData mapObject)
        {
            if (mapObject is WorldObjectData mapWorldObject)
            {
                return await CreateFromMapObject(mapWorldObject);
            }
            else if (mapObject is SplineObjectData mapRamObject)
            {
                return await CreateFromMapObject(mapRamObject);
            }
            else if (mapObject is ScatterAreaData mapScatterObject)
            {
                return await CreateFromMapObject(mapScatterObject);
            }

            return null;
        }

        /// <summary>
        /// Creates a World Object from a MapWorldObject.
        /// </summary>
        /// <param name="mapObject">The map object representation of the object to create.</param>
        /// <returns>An instance of the object represented by the input map object.</returns>
        public static async Task<WorldObjectBase> CreateFromMapObject(WorldObjectData mapObject)
        {
            var gameObject = await Helpers.InstantiateAddressable(mapObject.prefabAddress);
            var worldObject = gameObject.AddComponent<WorldObject>();
            AddStandardComponents(gameObject);
            AddDraggableComponent(gameObject);
            worldObject.Initialise(mapObject);
            return worldObject;
        }

        /// <summary>
        /// Creates a WorldObject from a MapRamObject.
        /// </summary>
        /// <param name="mapObject">The map object representation of the object to create.</param>
        /// <returns>An instance of the object represented by the input map object.</returns>
        public static async Task<WorldObjectBase> CreateFromMapObject(SplineObjectData mapObject)
        {
            var gameObject = await Helpers.InstantiateAddressable(RAM_OBJECT_PREFAB);
            gameObject.transform.position = Vector3.zero;
            var worldObject = gameObject.GetComponent<RamObject>();
            AddStandardComponents(gameObject);
            await worldObject.Initialise(mapObject);
            return worldObject;
        }

        public static async Task<WorldObjectBase> CreateFromMapObject(ScatterAreaData mapObject)
        {
            var gameObject = await Helpers.InstantiateAddressable(SCATTER_AREA_PREFAB);
            gameObject.transform.position = Vector3.zero;
            AddStandardComponents(gameObject);
            var worldObject = gameObject.GetComponent<PolygonObject>();
            worldObject.Initialise(mapObject);
            return worldObject;
        }

        /// <summary>
        /// Creates a world object based on another world object
        /// </summary>
        /// <param name="fromObject">The original WorldObject to clone.</param>
        /// <returns>An instance of a WorldObject cloned from the input WorldObject.</returns>
        public static async Task<WorldObjectBase> Clone(WorldObjectBase fromObject)
        {
            var gameObject = await Helpers.InstantiateAddressable(fromObject.PrefabAddress);
            var worldObject = gameObject.AddComponent<WorldObject>();
            AddStandardComponents(gameObject);
            AddDraggableComponent(gameObject);
            worldObject.CloneFrom(fromObject);

            return worldObject;
        }

        /// <summary>
        /// Replace a game object with a different prefab.
        /// </summary>
        /// <param name="objectToReplace">The WorldObject that is to be changed.</param>
        /// <param name="contentItem">The contentItem to instantiate as the replacement.</param>
        /// <param name="prefabAddress">The address of the prefab to instantiate in the WorldObject's place.</param>
        /// <returns>An instance of a WorldObject of the new prefab, with any relevant settings cloned.</returns>
        public static async Task<WorldObjectBase> Replace(WorldObjectBase objectToReplace, ContentItem contentItem, string prefabAddress)
        {
            WorldObjectBase worldObject = null;

            if (objectToReplace is WorldObject)
            {
                var objectIsSelected = WorldObjectBase.Current == objectToReplace;

                // Create the new object and clone it from the old one
                var gameObject = await Helpers.InstantiateAddressable(prefabAddress);
                worldObject = gameObject.AddComponent<WorldObject>();
                AddStandardComponents(gameObject);
                AddDraggableComponent(gameObject);
                worldObject.CloneFrom(objectToReplace, prefabAddress, false, contentItem);

                // Destroy the old object and select the new one
                objectToReplace.Destroy();
                if (objectIsSelected)
                {
                    worldObject.Select();
                }
            }
            else if (objectToReplace is ScalableObject)
            {
                await ((ScalableObject)objectToReplace).SetMaterial(prefabAddress);
                worldObject = objectToReplace;
            }
            else if (objectToReplace is RamObject)
            {
                await ((RamObject)objectToReplace).SetProfile(prefabAddress);
                worldObject = objectToReplace;
            }

            return worldObject;
        }

        /// <summary>
        /// Add components that all world objects should have.
        /// </summary>
        /// <param name="gameObject">The GameObject to connect the components to.</param>
        private static void AddStandardComponents(GameObject gameObject)
        {
            // Map editor components
            gameObject.AddComponent<HighlightPlus.HighlightEffect>();
            gameObject.AddComponent<WorldObjectHighlighter>();
        }

        /// <summary>
        /// Adds a component that makes the object draggable.
        /// </summary>
        /// <param name="gameObject">The GameObject to add the component to.</param>
        private static void AddDraggableComponent(GameObject gameObject)
        {
            // Make object draggable if in map editor
            gameObject.AddComponent<DraggableObject>();
        }
        #endregion
    }
}