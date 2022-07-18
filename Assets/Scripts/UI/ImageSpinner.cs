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
using UnityEngine.UI;

namespace TT.UI
{
    /// <summary>
    /// Attached to an image on the UI, will spin the image around continuously.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class ImageSpinner : MonoBehaviour
    {
        
        #region Editor fields
        
        [SerializeField][Tooltip("The number of degrees to rate each second.")] private float rotationSpeed = 360;
        
        #endregion
        
        
        #region Private fields

        private Image _image;
        private Vector3 _rotationDirection;
        
        #endregion
        
        
        #region Lifecycle events
        
        // Start is called before the first frame update
        void Start()
        {
            _image = GetComponent<Image>();
            _rotationDirection = new Vector3(0, 0, -1);
        }

        private void Update()
        {
            // TODO: Stop this spinning when the image isn't visible
            _image.transform.Rotate(_rotationDirection, rotationSpeed * Time.deltaTime);
        }
        
        #endregion
        
    }
}
