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
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using TT.Data;
using TT.Shared.World;
using TT.World;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace TT
{
    /// <summary>
    /// Methods and properties that are used in many places.
    /// </summary>
    public static class Helpers
    {
        
        #region Private fields
        
        private static Camera _mainCamera;
        private static SettingsObject _settings;
        private static CommsObject _comms;
        private static readonly List<AsyncOperationHandle> AsyncOperationHandles = new List<AsyncOperationHandle>();


        #endregion


        #region Public properties

        /// <summary>
        /// The first camera in the scene.
        /// </summary>
        public static Camera MainCamera
        {
            get
            {
                if (!_mainCamera) _mainCamera = Object.FindObjectOfType<Camera>();
                return _mainCamera;
            }
        }

        /// <summary>
        /// The settings to be applied to the current game.
        /// </summary>
        public static SettingsObject Settings
        {
            get
            {
                if (!_settings) _settings = Object.FindObjectOfType<SettingsObject>();
                return _settings;
            }
        }

        /// <summary>
        /// Library for communicating with the outside world.
        /// </summary>
        public static CommsObject Comms
        {
            get
            {
                if (!_comms) _comms = Object.FindObjectOfType<CommsObject>();
                return _comms;
            }
        }

        /// <summary>
        /// The layer IgnoreRaycast.
        /// </summary>
        public static int IgnoreRaycastLayer => LayerMask.NameToLayer("Ignore Raycast");
        /// <summary>
        /// The layer Traversable.
        /// </summary>
        public static int TraversableLayer => LayerMask.NameToLayer("Traversable");
        /// <summary>
        /// The layer Impassable.
        /// </summary>
        public static int ImpassableLayer => LayerMask.NameToLayer("Impassable");
        /// <summary>
        /// The layer Water.
        /// </summary>
        public static int WaterLayer => LayerMask.NameToLayer("Water");


        /// <summary>
        /// A layer mask for the Terrain.
        /// </summary>
        public static int TerrainMask => LayerMask.GetMask("Terrain");
        /// <summary>
        /// A layer mask for Water.
        /// </summary>
        public static int WaterMask => LayerMask.GetMask("Water");
        /// <summary>
        /// A layer mask for Terrain and Water.
        /// </summary>
        public static int TerrainAndWaterMask => LayerMask.GetMask("Terrain", "Water");
        /// <summary>
        /// A layer mask for objects that are stackable.
        /// </summary>
        public static int StackableMask => LayerMask.GetMask("Terrain", "Impassable");
        /// <summary>
        /// A layer mask for areas that are impassable.
        /// </summary>
        public static int ImpassableMask => LayerMask.GetMask("Water", "Impassable");
        /// <summary>
        /// A layer mask for areas that are traversable.
        /// </summary>
        public static int TraversableMask => LayerMask.GetMask("Terrain", "Traversable");
        /// <summary>
        /// A layer mask for objects that are selectable.
        /// </summary>
        public static int SelectableMask => LayerMask.GetMask("Impassable", "Traversable");


        /// <summary>
        /// The PlayerPrefs key used to store the signed in user's e-mail address.
        /// </summary>
        public const string PrefsEmailKey = "Email";
        /// <summary>
        /// The PlayerPrefs key used to store the user's auth refresh token.
        /// </summary>
        public const string PrefsRefreshTokenKey = "Refresh";
        /// <summary>
        /// The PlayerPrefs key used to store whether the user wanted their auth to be remembered.
        /// </summary>
        public const string PrefsRememberMeKey = "RememberMe";


        public const string MapEditorSceneName = "MapEditor";
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
            if (!Physics.Raycast(MainCamera.ScreenPointToRay(Input.mousePosition), out var hit, Mathf.Infinity,
                    raycastLayerMask))
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
            var plane = new Plane(Vector3.up, new Vector3(0, elevation, 0));
            var ray = MainCamera.ScreenPointToRay(Input.mousePosition);
            plane.Raycast(ray, out var enter);
            return ray.GetPoint(enter);
        }


        /// <summary>
        /// Gets the preferred object of the right type at the mouse position. If no types are specified, all types are
        /// returned.
        /// </summary>
        /// <param name="types">A list of types to return. If null or an empty list is specified, this returns any
        ///     type.</param>
        /// <param name="raycastLayerMask">The layer mask of objects to hit.</param>
        /// <param name="returnRoot">If true the root object is returned. If false the object that was hit is returned,
        ///     which could for example be a handle, or a sub-component of an object.</param>
        /// <returns>The object under the cursor, in the order of a drag handle on the current object, the currently
        ///     selected object, an object of a WorldObjectType in types, in that order.</returns>
        public static GameObject GetObjectAtMouse(List<WorldObjectType> types, int raycastLayerMask = 1,
            bool returnRoot = true)
        {
            // Get all objects at the pointer
            var hits = Physics.RaycastAll(MainCamera.ScreenPointToRay(Input.mousePosition), 
                Mathf.Infinity, raycastLayerMask);
            GameObject newObjectHit = null;
            GameObject dragHandleHit = null;
            GameObject currentObjectHit = null;
            var builder = new StringBuilder();
            builder.Append($"Raycasting for [{string.Join(",", types)}] with mask [{raycastLayerMask}]\n");
            builder.Append(string.Format("Hit {0} objects\n", hits.Length));
            foreach (var hit in hits)
            {
                // Try to get a WorldObjectBase from the object we've hit
                var worldObjectHit = hit.collider.transform.root.gameObject.GetComponent<WorldObjectBase>();

                builder.Append(string.Format("Hit {0} ", hit.collider.gameObject.name));
                if (types.Count > 0 && !types.Contains(worldObjectHit.Type))
                {
                    // Types have been specified and this is not of the right type
                    builder.Append($"which isn't part of a {string.Join(",", types)}.\n");
                    continue;
                }

                if ((hit.collider.transform.GetComponentInParent<DraggableObject>() ||
                     hit.collider.transform.GetComponent<DraggableObject>()) &&
                    worldObjectHit == WorldObjectBase.Current)
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
            var chosenObject = dragHandleHit != null ? dragHandleHit :
                currentObjectHit != null ? currentObjectHit : newObjectHit;
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
            var eventData = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition
            };
            var raycastResults = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, raycastResults);

            // UI layer to check
            var uiLayer = LayerMask.NameToLayer("UI");

            foreach (var raycastResult in raycastResults)
                if (raycastResult.gameObject.layer == uiLayer)
                    // If the result is on a UI layer
                    return true;
            // No hits on the UI
            return false;
        }

        #endregion


        #region Addressables

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
            var locatorsHandle = Addressables.LoadResourceLocationsAsync((IEnumerable<string>) addresses,
                Addressables.MergeMode.Union, typeof(T));
            await locatorsHandle.Task;
            var locators = locatorsHandle.Result;

            if (locators.Count == 0)
            {
                Debug.LogErrorFormat("Helpers :: LoadAddressables :: No addressable found for ['{0}']",
                    string.Join("', '", addresses));
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
        /// <returns>A GameObject instance of the prefab at the given address, or null if no prefab exists at the given
        /// address.</returns>
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
        /// <remarks>DO NOT call this until all the addressables have been released, or this may destroy objects used
        /// by said addressables.</remarks>
        public static void ReleaseHandles()
        {
            if (AsyncOperationHandles != null && AsyncOperationHandles.Count > 0)
                // Tell Addressables to release every handle
                AsyncOperationHandles.ForEach(x => Addressables.Release(x));
        }

        #endregion


        #region UI

        /// <summary>
        /// Finds an available button in the collection. When none are found, a new one is instantiated, added to the
        /// collection and returned.
        /// </summary>
        /// <typeparam name="T">Any object that is a UnityEngine.MonoBehaviour.</typeparam>
        /// <param name="prefab">The prefab to instantiate if no available buttons are found.</param>
        /// <param name="collection">A collection to be searched for an available button.</param>
        /// <param name="inactiveUIElementParent">A transform to which all inactive UI elements are parented.</param>
        /// <returns>A button of type T that is available for use.</returns>
        public static T GetAvailableButton<T>(T prefab, List<T> collection, Transform inactiveUIElementParent)
        {
            for (var i = 0; i < collection.Count; i++)
                if ((collection[i] as MonoBehaviour)?.transform.parent == inactiveUIElementParent)
                    return collection[i];

            var genericPrefab = prefab as MonoBehaviour;
            var genericButton = Object.Instantiate(genericPrefab);
            var newButton = genericButton.GetComponent<T>();
            collection.Add(newButton);
            return newButton;
        }

        /// <summary>
        /// Returns a string representing the specified datetime, in the local timezone, in the format "[time] [date]"
        /// where the date is double digit numbers for day, month and year, formatted as per the current UI culture.
        /// </summary>
        /// <param name="fileTimeUtc">The date and time in a UTC file time format.</param>
        /// <returns>A string representing the passed in date and time.</returns>
        public static string FormatShortDateString(long fileTimeUtc)
        {
            return FormatShortDateString(DateTime.FromFileTimeUtc(fileTimeUtc));
        }

        /// <summary>
        /// Returns a string representing the specified datetime, in the format "[time] [date]" where the date is
        /// double digit numbers for day, month and year, formatted as per the current UI culture.
        /// </summary>
        /// <param name="dateTime">The date and time to format.</param>
        /// <returns>A string representing the passed in date and time.</returns>
        public static string FormatShortDateString(DateTime dateTime)
        {
            var formatInfo = CultureInfo.CurrentUICulture.DateTimeFormat;
            var shorterDateString = formatInfo.ShortDatePattern.Replace("yyyy", "yy");
            return string.Concat(dateTime.ToLocalTime().ToString("t"), " ", dateTime.ToString(shorterDateString));
        }

        /// <summary>
        /// Formats a file size in bytes to be displayed to the user.
        /// </summary>
        /// <param name="sizeInBytes">The size in bytes.</param>
        /// <returns>The size in GB, MB, KB or bytes. When sizeInBytes is 0, null is returned.</returns>
        public static string FormatFileSizeString(long sizeInBytes)
        {
            if (sizeInBytes == 0)
                return null;

            if (sizeInBytes > 1000000000)
                return (Mathf.Round((float) sizeInBytes / 100000000) / 10).ToString("#.# GB",
                        CultureInfo.CurrentUICulture);
            
            if (sizeInBytes > 1000000)
                return (Mathf.Round((float) sizeInBytes / 100000) / 10).ToString("#.# MB", 
                    CultureInfo.CurrentUICulture);

            if (sizeInBytes > 1000)
                return (Mathf.Round((float) sizeInBytes / 100) / 10).ToString("#.# KB", 
                    CultureInfo.CurrentUICulture);

            return sizeInBytes.ToString("# bytes", CultureInfo.CurrentUICulture);
        }

        #endregion


        #region GameObjects

        /// <summary>
        /// Sets the layer on the given object and all its child objects.
        /// </summary>
        /// <param name="obj">The object to set the layer on.</param>
        /// <param name="targetLayer">The layer to set.</param>
        public static void SetLayerRecursive(GameObject obj, int targetLayer)
        {
            if (!obj.CompareTag("DoNotChangeLayer")) obj.layer = targetLayer;

            foreach (Transform child in obj.transform)
                if (child != null && !child.CompareTag("DoNotChangeLayer"))
                    SetLayerRecursive(child.gameObject, targetLayer);
        }

        #endregion

    }
}