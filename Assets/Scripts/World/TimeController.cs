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
using UnityEngine;

namespace TT.World
{
    public class TimeController : MonoBehaviour
    {
        #region Events

        public static Action<float> TimeChanged;

        #endregion


        #region Editor fields
#pragma warning disable IDE0044 // Make fields read-only

        [SerializeField] private Light sun;
        [SerializeField] private Light moon;
        [SerializeField] private Light waterLight;
        [SerializeField] private Light editorLight;
        [SerializeField] private Skybox skybox;
        [SerializeField] private Material[] skyboxMaterials = new Material[24];
        [SerializeField] private float latitude = 53.231009f;
        [SerializeField] private float longitude = -2.606448f;
        [SerializeField] private Color ambientLight = new Color(0.4627451f, 0.5176471f, 0.6784314f);

#pragma warning restore IDE0044
        #endregion


        #region Public properties

        /// <summary>
        /// Singleton instance.
        /// </summary>
        public static TimeController Current { get; private set; }


        private float _currentTime;
        /// <summary>
        /// The current time as a float ranging from 0 to 24
        /// </summary>
        public float CurrentTime
        {
            get => _currentTime;
            set
            {
                _currentTime = value;
                UpdateLights();
                UpdateSkybox();

                TimeChanged?.Invoke(_currentTime);
            }
        }

        private LightingMode _lightingMode = LightingMode.Ambient;
        /// <summary>
        /// The type of lighting to use.
        /// </summary>
        public LightingMode LightingMode
        {
            get => _lightingMode;
            set
            {
                _lightingMode = value;
                UpdateLights();

                // Tell objects with lights to re-evaluate their light settings
                TimeChanged?.Invoke(_currentTime);
            }
        }

        private bool _editorLighting;
        /// <summary>
        /// Whether the editor light is on.
        /// </summary>
        public bool EditorLighting
        {
            get => _editorLighting;
            set
            {
                _editorLighting = value;
                Debug.LogFormat("Setting editor lighting to {0}", value);
                UpdateLights();
            }
        }

        /// <summary>
        /// True when the current time is considered daytime.
        /// </summary>
        public bool Daytime { get => CurrentTime >= 5 && CurrentTime <= 19; }

        /// <summary>
        /// True when the moon is visible. Note the moon may be visible during Daytime.
        /// </summary>
        public bool MoonOut { get => CurrentTime <= 7 || CurrentTime >= 17; }

        #endregion


        #region Lifecycle events
#pragma warning disable IDE0051 // Unused members

        void Awake()
        {
            Current = this;
        }

        void Start()
        {
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientSkyColor = ambientLight;
            UpdateLights();
        }

#pragma warning restore IDE0051 // Unused members
        #endregion


        #region Private methods

        /// <summary>
        /// Enable / disable lights based on time and lighting mode.
        /// </summary>
        private void UpdateLights()
        {
            sun.enabled = Daytime && _lightingMode == LightingMode.Ambient && !_editorLighting;
            moon.enabled = MoonOut && _lightingMode == LightingMode.Ambient && !_editorLighting;
            editorLight.enabled = _editorLighting;
            RenderSettings.ambientSkyColor = _lightingMode == LightingMode.Indoor && !_editorLighting ? Color.black : ambientLight;

            if (sun.enabled) SetSunRotation();
            if (moon.enabled) SetMoonRotation();

            SetWaterLight();
        }

        private void SetWaterLight()
        {
            if (moon.enabled)
            {
                waterLight.transform.rotation = moon.transform.rotation;
                waterLight.color = moon.color;
            }
            else
            {
                waterLight.transform.localRotation = sun.transform.localRotation;
                waterLight.color = sun.color;
            }

            waterLight.intensity = _lightingMode == LightingMode.Ambient ? .2f : .01f;
        }

        /// <summary>
        /// Updates the sun light rotation based on the current time.
        /// </summary>
        private void SetSunRotation()
        {
            // Create a datetime in mid-year with the current time
            var hour = (int)_currentTime;
            var minutes = (int)((_currentTime - hour) * 60);
            DateTime time = new DateTime(DateTime.Now.Year, 6, 15, hour, minutes, 0);

            // Set the sun position and the water sun opposite
            SunPosition.CalculateSunPosition(time, latitude, longitude, out double azi, out double alt);
            Vector3 angles = new Vector3()
            {
                x = (float)alt * Mathf.Rad2Deg + 20, // Add 20 to get longer daylight
                y = (float)azi * Mathf.Rad2Deg
            };
            sun.transform.localRotation = Quaternion.Euler(angles);

            // Wind down intensity at the start and end of day
            sun.intensity = Mathf.InverseLerp(0, 20, angles.x) * 1.3f;
        }

        /// <summary>
        /// Updates the moon light rotation based on the current time.
        /// </summary>
        private void SetMoonRotation()
        {
            // Calculate moon position as X going from 0 to 180 degrees and Y from 60 to 0 and back to 60
            var nightStart = 17;
            var nightEnd = 7;
            var timeIntoNight = (_currentTime > nightStart ? _currentTime - nightStart : _currentTime + nightEnd) / (24 - nightStart + nightEnd);
            var x = Mathf.Lerp(0, 180, timeIntoNight);
            var y = Mathf.Abs(Mathf.Lerp(-60, 60, timeIntoNight));

            moon.transform.rotation = Quaternion.Euler(x, y, 0);
        }

        /// <summary>
        /// Sets the skybox based on the current time.
        /// </summary>
        private void UpdateSkybox()
        {
            skybox.material = skyboxMaterials[(int)_currentTime];
        }

        #endregion

    }
}