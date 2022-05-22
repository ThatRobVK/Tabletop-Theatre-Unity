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

using UnityEngine;
using UnityEngine.Serialization;

namespace TT.World
{
    public class WindController : MonoBehaviour
    {

        #region Editor fields
#pragma warning disable IDE0044 // Make fields read-only

        [SerializeField][Tooltip("The Unity wind zone")] private WindZone wind;
        [FormerlySerializedAs("nM_Wind")] [SerializeField][Tooltip("The NatureManufactura wind component")] private NM_Wind nMWind;

#pragma warning restore IDE0044
        #endregion


        #region Public properties

        /// <summary>
        /// Singleton instance.
        /// </summary>
        public static WindController Current { get; private set; }

        private float _currentWind;
        /// <summary>
        /// The wind strength.
        /// </summary>
        public float CurrentWind
        {
            get => _currentWind;
            set
            {
                _currentWind = value;
                ConfigureWind();
            }
        }

        public float Rotation
        {
            get => wind.transform.rotation.eulerAngles.y;
            set
            {
                wind.transform.rotation = Quaternion.Euler(0, value, 0);
            }
        }

        #endregion


        #region Lifecycle events
#pragma warning disable IDE0051 // Unused members

        void Awake()
        {
            Current = this;
            _currentWind = wind.windMain;
        }

#pragma warning restore IDE0051 // Unused members
        #endregion


        #region Private methods

        /// <summary>
        /// Sets the wind zone and NM wind component up based on the wind strength.
        /// </summary>
        private void ConfigureWind()
        {
            wind.windMain = CurrentWind;
            wind.windTurbulence = CurrentWind / 2;
            wind.windPulseMagnitude = CurrentWind / 2;
            wind.windPulseFrequency = .5f;

            nMWind.WindSpeed = Mathf.Clamp(CurrentWind * 100, 0, 80);
            nMWind.Turbulence = .4f;
        }

        #endregion

    }
}