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

using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;
using TT.Data;
using TT.World;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace TT
{
    public static class Helpers
    {
        #region Private properties

        private static Camera _mainCamera;
        public static Camera MainCamera
        {
            get
            {
                if (_mainCamera == null) _mainCamera = Object.FindObjectOfType<Camera>();
                return _mainCamera;
            }
        }

        private static SettingsObject _settings;
        public static SettingsObject Settings
        {
            get
            {
                if (_settings == null) _settings = Object.FindObjectOfType<SettingsObject>();
                return _settings;
            }
        }

        private static CommsObject _comms;

        public static CommsObject Comms
        {
            get
            {
                if (_comms == null) _comms = Object.FindObjectOfType<CommsObject>();
                return _comms;
            }
        }

        // Layers
        public static int IgnoreRaycastLayer { get => LayerMask.NameToLayer("Ignore Raycast"); }
        public static int TraversableLayer { get => LayerMask.NameToLayer("Traversable"); }
        public static int ImpassableLayer { get => LayerMask.NameToLayer("Impassable"); }
        public static int WaterLayer { get => LayerMask.NameToLayer("Water"); }

        // Layer masks
        public static int TerrainMask { get => LayerMask.GetMask("Terrain"); }
        public static int WaterMask { get => LayerMask.GetMask("Water"); }
        public static int TerrainAndWaterMask { get => LayerMask.GetMask(new string[] { "Terrain", "Water" }); }
        public static int StackableMask { get => LayerMask.GetMask(new string[] { "Terrain", "Impassable" }); }
        public static int ImpassableMask { get => LayerMask.GetMask(new string[] { "Water", "Impassable" }); }
        public static int TraversableMask { get => LayerMask.GetMask(new string[] { "Terrain", "Traversable" }); }
        public static int SelectableMask { get => LayerMask.GetMask(new string[] { "Impassable", "Traversable" }); }

        #endregion


        #region Game world

        /// <summary>
        /// Gets the position of the mouse in world coordinates
        /// </summary>
        /// <param name="raycastLayerMask">A LayerMask for all layers to return objects for.</param>
        /// <returns>A Vector3 containing coordinates in world space where the mouse position raycast hit.</returns>
        public static Vector3 GetWorldPointFromMouse(int raycastLayerMask = -1)
        {
            if (raycastLayerMask == -1) raycastLayerMask = TerrainMask;

            // If the ray doesn't hit anything, return nothing
            if (!Physics.Raycast(MainCamera.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, Mathf.Infinity, raycastLayerMask))
                return new Vector3();

            // Return the hit point
            return hit.point;
        }

        /// <summary>
        /// Gets the position of the mouse in world coordinates
        /// </summary>
        /// <param name="elevation">A fixed elevation to return the ray hit at.</param>
        /// <returns>A Vector3 containing coordinates in world space where the mouse position raycast hit.</returns>
        public static Vector3 GetWorldPointFromMouse(float elevation)
        {
            Plane plane = new Plane(Vector3.up, new Vector3(0, elevation, 0));
            var ray = MainCamera.ScreenPointToRay(Input.mousePosition);
            plane.Raycast(ray, out float enter);
            return ray.GetPoint(enter);
        }


        /// <summary>
        /// Gets the preferred object of the right type at the mouse position. If no types are specified, all types are returned.
        /// </summary>
        /// <param name="types">A list of types to return. If null or an empty list is specified, this returns any type.</param>
        /// <param name="raycastLayerMask">The layer mask of objects to hit.</param>
        /// <param name="returnRoot">If true the root object is returned. If false the object that was hit is returned, which could for example be a handle, or a sub-component of an object.</param>
        /// <returns>The object under the cursor, in the order of a drag handle on the current object, the currently selected object, an object of a WorldObjectType in types, in that order.</returns>
        public static GameObject GetObjectAtMouse(List<WorldObjectType> types, int raycastLayerMask = 1, bool returnRoot = true)
        {
            // Get all objects at the pointer
            var hits = Physics.RaycastAll(MainCamera.ScreenPointToRay(Input.mousePosition), Mathf.Infinity, raycastLayerMask);
            GameObject newObjectHit = null;
            GameObject dragHandleHit = null;
            GameObject currentObjectHit = null;
            StringBuilder builder = new StringBuilder();
            builder.Append($"Raycasting for [{string.Join(",", types)}] with mask [{raycastLayerMask}]\n");
            builder.Append(string.Format("Hit {0} objects\n", hits.Length));
            foreach (var hit in hits)
            {
                // Try to get a WorldObjectBase from the object we've hit
                WorldObjectBase worldObjectHit = hit.collider.transform.root.gameObject.GetComponent<WorldObjectBase>();

                builder.Append(string.Format("Hit {0} ", hit.collider.gameObject.name));
                if (types.Count > 0 && !types.Contains(worldObjectHit.Type))
                {
                    // Types have been specified and this is not of the right type
                    builder.Append($"which isn't part of a {string.Join(",", types)}.\n");
                    continue;
                }

                if ((hit.collider.transform.GetComponentInParent<DraggableObject>() || hit.collider.transform.GetComponent<DraggableObject>()) && worldObjectHit == WorldObjectBase.Current)
                {
                    builder.Append("which is draggable and on the currently selected object.\n");
                    // If the object is draggable, pick it over others
                    dragHandleHit = hit.collider.gameObject;
                }
                else if (worldObjectHit == WorldObjectBase.Current)
                {
                    builder.Append("which is the currently selected object.\n");
                    // If the object is selected, it's the one we want so pick it and break out of the loop
                    currentObjectHit = hit.collider.gameObject;
                }
                else if (newObjectHit == null)
                {
                    builder.Append("which is the first object we've hit.\n");
                    // If the object isn't selected but is a WorldObjectBase, pick the first one we hit
                    newObjectHit = hit.collider.gameObject;
                }
                else
                {
                    builder.Append("which is new but we've already hit something else.\n");
                }
            }

            // Priority = 1. Drag handle / 2. Current object / 3. New object
            GameObject chosenObject = dragHandleHit != null ? dragHandleHit : currentObjectHit != null ? currentObjectHit : newObjectHit;
            if (chosenObject != null)
            {
                builder.Append(string.Format("Returning {0} as the chosen object.", chosenObject.name));
                if (Settings.editorSettings.debugRaycasts) Debug.Log(builder.ToString());
                return returnRoot ? chosenObject.transform.root.gameObject : chosenObject;
            }

            if (Settings.editorSettings.debugRaycasts) Debug.Log(builder.ToString());
            return null;
        }

        /// <summary>
        /// Returns true if the mouse pointer is over a UI element.
        /// </summary>
        /// <returns>True if the mouse pointer is over an object on the UI layer, false if not.</returns>
        public static bool IsPointerOverUIElement()
        {
            // Raycast to the mouse position based on the event system
            PointerEventData eventData = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition
            };
            List<RaycastResult> raycastResults = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, raycastResults);

            // UI layer to check
            var uiLayer = LayerMask.NameToLayer("UI");

            foreach (var raycastResult in raycastResults)
            {
                if (raycastResult.gameObject.layer == uiLayer)
                {
                    // If the result is on a UI layer
                    return true;
                }
            }
            // No hits on the UI
            return false;
        }

        #endregion


        #region Addressables

        private static readonly List<AsyncOperationHandle> AsyncOperationHandles = new List<AsyncOperationHandle>();

        /// <summary>
        /// Initialises the Addressables system to speed up asset loading once required.
        /// </summary>
        public static void InitialiseAddressables()
        {
            Addressables.InitializeAsync();
        }

        /// <summary>
        /// Loads a set of addressables and returns them.
        /// </summary>
        /// <typeparam name="T">The Type of the object to load, e.g. Texture2D, GameObject, etc.</typeparam>
        /// <param name="addresses">A list of addresses to load.</param>
        /// <returns>A list of objects returned by the Addressables system.</returns>
        public static async Task<IList<T>> LoadAddressables<T>(string[] addresses)
        {
            // Get resource locators for all items in the array
            var locatorsHandle = Addressables.LoadResourceLocationsAsync((IEnumerable<string>)addresses, Addressables.MergeMode.Union, typeof(T));
            await locatorsHandle.Task;
            var locators = locatorsHandle.Result;

            if (locators.Count == 0)
            {
                Debug.LogErrorFormat("Helpers :: LoadAddressables :: No addressable found for ['{0}']", string.Join("', '", addresses));
                return null;
            }

            // Get the objects for the locators
            var objectsHandle = Addressables.LoadAssetsAsync<T>(locators, null);
            await objectsHandle.Task;
            var objects = objectsHandle.Result;

            // Store the handles so they can be released later
            AsyncOperationHandles.Add(locatorsHandle);
            AsyncOperationHandles.Add(objectsHandle);

            return objects;
        }

        /// <summary>
        /// Instantiates a GameObject from Addressables and returns it.
        /// </summary>
        /// <param name="address">The address of the prefab to instantiate.</param>
        /// <returns>A GameObject instance of the prefab at the given address, or null if no prefab exists at the given address.</returns>
        public static async Task<GameObject> InstantiateAddressable(string address)
        {
            var objectHandle = Addressables.InstantiateAsync(address);
            await objectHandle.Task;

            if (objectHandle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogErrorFormat("Unable to load object {0}", address);
                return null;
            }

            return objectHandle.Result;
        }

        /// <summary>
        /// Releases all async operation handles used for loading addressables.
        /// </summary>
        /// <remarks>DO NOT call this until all the addressables have been released, or this may destroy objects used by said addressables.</remarks>
        public static void ReleaseHandles()
        {
            if (AsyncOperationHandles != null && AsyncOperationHandles.Count > 0)
            {
                // Tell Addressables to release every handle
                AsyncOperationHandles.ForEach(x => Addressables.Release(x));
            }
        }

        #endregion


        #region UI

        /// <summary>
        /// Finds an available button in the collection. When none are found, a new one is instantiated, added to the collection and returned.
        /// </summary>
        /// <typeparam name="T">Any object that is a UnityEngine.MonoBehaviour.</typeparam>
        /// <param name="prefab">The prefab to instantiate if no available buttons are found.</param>
        /// <param name="collection">A collection to be searched for an available button.</param>
        /// <param name="inactiveUIElementParent">A transform to which all inactive UI elements are parented.</param>
        /// <returns>A button of type T that is available for use.</returns>
        public static T GetAvailableButton<T>(T prefab, List<T> collection, Transform inactiveUIElementParent)
        {
            for (int i = 0; i < collection.Count; i++)
            {
                if ((collection[i] as MonoBehaviour)?.transform.parent == inactiveUIElementParent)
                {
                    return collection[i];
                }
            }

            var genericPrefab = prefab as MonoBehaviour;
            var genericButton = Object.Instantiate(genericPrefab);
            var newButton = genericButton.GetComponent<T>();
            collection.Add(newButton);
            return newButton;
        }


        #endregion


        #region GameObjects

        // Sets the layer on the given object and all its child objects
        public static void SetLayerRecursive(GameObject obj, int targetLayer)
        {
            if (!obj.CompareTag("DoNotChangeLayer")) obj.layer = targetLayer;

            foreach (Transform child in obj.transform)
            {
                if (child != null && !child.CompareTag("DoNotChangeLayer")) SetLayerRecursive(child.gameObject, targetLayer);
            }
        }


        #endregion


        #region Data

        public static byte[] Compress(string text)
        {
            using var outStream = new MemoryStream();
            using (var tinyStream = new GZipStream(outStream, CompressionMode.Compress))
            using (var mStream = new MemoryStream(Encoding.UTF8.GetBytes(text)))
                mStream.CopyTo(tinyStream);

            return outStream.ToArray();
        }

        public static string Decompress(byte[] bytes)
        {
            using var inStream = new MemoryStream(bytes);
            using var bigStream = new GZipStream(inStream, CompressionMode.Decompress);
            using var bigStreamOut = new MemoryStream();
                bigStream.CopyTo(bigStreamOut);

            return Encoding.UTF8.GetString(bigStreamOut.ToArray());
        }
        #endregion

    }
}