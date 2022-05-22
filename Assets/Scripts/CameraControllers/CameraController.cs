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

using TT.InputMapping;
using TT.UI;
using TT.World;
using UnityEngine;

namespace TT.CameraControllers
{
    /// <summary>
    /// Based on original code from GameDevGuide: https://youtu.be/rnqF6S7PfFA with modifications made specifically for TT.
    /// </summary>
    public class CameraController : MonoBehaviour
    {

        #region Editor fields
#pragma warning disable IDE0044 // Make fields read-only

        [SerializeField][Tooltip("The transform of the rig itself. Used for movement and rotation.")] private Transform rigTransform;
        [SerializeField][Tooltip("The transform within the rig that the camera is attached to. Used for panning up and down.")] private Transform rigArmTransform;
        [SerializeField][Tooltip("The transform of the camera itself, used for zooming.")] private Transform cameraTransform;
        [SerializeField][Tooltip("Standard movement speed.")] private float normalSpeed = 0.5f;
        [SerializeField][Tooltip("Fast movement speed when Shift is held down.")] private float fastSpeed = 3f;
        [SerializeField][Tooltip("How quickly the movement lerps to the final position. Higher is faster.")] private float movementTime = 8f;
        [SerializeField][Tooltip("How quickly the camera rotates.")] private float rotationAmount = 2f;
        [SerializeField][Tooltip("How quickly the camera zooms.")] private float zoomAmount = 4f;
        [SerializeField][Tooltip("The factor by which movement slows down when zoomed in.")] private float zoomMovementSpeedMultiplier = 50f;

#pragma warning restore IDE0044
        #endregion


        #region Private fields

        private const float ORTHOGRAPHIC_SIZE_CONVERSION = 2.1448f; // approximate conversion keeping the view the same at most zoom levels (not quite zoomed very close in)

        private Vector3 _newPosition;
        private Quaternion _newRigRotation;
        private Quaternion _newRigArmRotation;
        private Vector3 _newZoom;
        private float _newOrthoSize;
        private Vector3 _rotateStartPosition;
        private Quaternion _topDownAngle;
        private Quaternion _isometricAngle;
        private float _movementSpeed;
        private Plane _plane;
        private Vector3 _dragStartPosition;

        #endregion


        #region Public properties

        /// <summary>
        /// The currently active CameraController
        /// </summary>
        public static CameraController Current { get; private set; }

        private bool _topDown;
        /// <summary>
        /// True locks the camera to top-down, without panning.
        /// </summary>
        public bool TopDown
        {
            get => _topDown;
            set
            {
                _topDown = value;
                _newRigArmRotation = _topDown ? _topDownAngle : _isometricAngle;

                // 2D is only allowed in topdown, so if not topdown and in 2D, toggle to 3D
                if (!_topDown && !PerspectiveMode) PerspectiveMode = true;
            }
        }

        private Camera _cameraObject;
        /// <summary>
        /// True sets the camera to perspective or 3D mode, false sets orthographic or 2D mode.
        /// </summary>
        public bool PerspectiveMode
        {
            get => !_cameraObject.orthographic;
            set
            {
                // Toggle orthographic only if in top down mode
                _cameraObject.orthographic = !value;

                // 2D is only allowed in topdown, so if switched to 2D and not in TopDown, toggle TopDown on
                if (_cameraObject.orthographic && !TopDown) TopDown = true;
            }
        }

        #endregion


        #region Lifecycle events
#pragma warning disable IDE0051 // Unused members

        void Awake()
        {
            _cameraObject = cameraTransform.GetComponent<Camera>();
        }

        void Start()
        {
            // On load, flag this as the active controller
            Current = this;

            _newPosition = rigTransform.position;
            _newRigRotation = rigTransform.rotation;
            _newZoom = cameraTransform.localPosition;
            _newOrthoSize = -_newZoom.z / ORTHOGRAPHIC_SIZE_CONVERSION;
            _newRigArmRotation = rigArmTransform.rotation;

            _topDownAngle = Quaternion.Euler(90, 0, 0);
            _isometricAngle = Quaternion.Euler(60, 0, 0);

            _plane = new Plane(Vector3.up, new Vector3(0, GameTerrain.Current.OffsetAltitude(0), 0));

            _newRigArmRotation = _topDownAngle;
        }

        void OnEnable()
        {
            // When enabled, switch Current to this controller
            Current = this;
        }

        void Update()
        {
            _movementSpeed = InputMapper.Current.CameraInput.Fast ? fastSpeed : normalSpeed;

            HandleMouseInput();
            HandleKeyboardInput();
            ApplyPositionsAndRotations();
        }

#pragma warning restore IDE0051 // Unused members
        #endregion


        #region Public methods

        /// <summary>
        /// Sends the camera to a specific location on the map.
        /// </summary>
        /// <param name="position">A position to move to. Only the X and Z coordinates are used.</param>
        public void MoveTo(Vector3 position)
        {
            _newPosition.x = position.x;
            _newPosition.z = position.z;
        }

        #endregion


        #region Private methods

        /// <summary>
        /// Update the Transforms, lerping towards the current values of the positions and rotations.
        /// </summary>
        private void ApplyPositionsAndRotations()
        {
            rigTransform.position = Vector3.Lerp(rigTransform.position, _newPosition, Time.deltaTime * movementTime);
            rigTransform.rotation = Quaternion.Lerp(rigTransform.rotation, _newRigRotation, Time.deltaTime * movementTime);
            cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, _newZoom, Time.deltaTime * movementTime);
            _cameraObject.orthographicSize = Mathf.Lerp(_cameraObject.orthographicSize, _newOrthoSize, Time.deltaTime * movementTime);
            rigArmTransform.localRotation = Quaternion.Lerp(rigArmTransform.localRotation, _newRigArmRotation, Time.deltaTime * movementTime);
        }

        /// <summary>
        /// Move and rotate the camera based on keyboard input.
        /// </summary>
        private void HandleKeyboardInput()
        {
            // Camera rig movement
            var movementVector = InputMapper.Current.CameraInput.GetCameraMovementVector();
            var heightMultiplier = _newZoom.z / zoomMovementSpeedMultiplier * -1;
            _newPosition += _movementSpeed * movementVector.z * heightMultiplier * rigTransform.forward;
            _newPosition += _movementSpeed * movementVector.x * heightMultiplier * rigTransform.right;

            // Camera rig rotation
            if (InputMapper.Current.CameraInput.RotateLeft)
            {
                _newRigRotation *= Quaternion.Euler(Vector3.up * rotationAmount);
            }
            if (InputMapper.Current.CameraInput.RotateRight)
            {
                _newRigRotation *= Quaternion.Euler(Vector3.up * -rotationAmount);
            }
        }

        /// <summary>
        /// Rotate and pan the camera based on mouse input.
        /// </summary>
        private void HandleMouseInput()
        {
            if (InputMapper.Current.CameraInput.Drag)
            {
                CursorController.Current.MoveCamera = true;

                if (Input.GetMouseButtonDown(0))
                {
                    // Start dragging (button pressed this frame)
                    var ray = Helpers.MainCamera.ScreenPointToRay(Input.mousePosition);
                    if (_plane.Raycast(ray, out float entry))
                    {
                        _dragStartPosition = ray.GetPoint(entry);
                    }
                }
                if (Input.GetMouseButton(0))
                {
                    CursorController.Current.MoveCamera2 = true;

                    // Dragging (button still down)
                    var ray = Helpers.MainCamera.ScreenPointToRay(Input.mousePosition);
                    if (_plane.Raycast(ray, out float entry))
                    {
                        // Apply the position directly to avoid camera shake
                        var dragCurrentPosition = ray.GetPoint(entry);
                        _newPosition = _newPosition + _dragStartPosition - dragCurrentPosition;
                        rigTransform.position = _newPosition;
                    }
                }
            }
            if (Input.mouseScrollDelta.y != 0 && !Helpers.IsPointerOverUIElement())
            {
                // Handle zoom on Z axis for perspective mode, and orthographic size for orthographic mode.
                _newZoom.z = Mathf.Clamp(_newZoom.z + Input.mouseScrollDelta.y * zoomAmount * _movementSpeed, -100, -2);
                _newOrthoSize = -_newZoom.z / ORTHOGRAPHIC_SIZE_CONVERSION;
            }

            // Handle rotation
            if (Input.GetMouseButtonDown(2))
            {
                _rotateStartPosition = Input.mousePosition;
            }

            if (Input.GetMouseButton(2))
            {
                // Calculate the left to right rotation of the main rig
                var difference = _rotateStartPosition - Input.mousePosition;
                _newRigRotation *= Quaternion.Euler(Vector3.up * (-difference.x / 5f));

                if (!TopDown)
                {
                    // Calculate the up / down rotation of the rig arm
                    var angles = _newRigArmRotation.eulerAngles;
                    angles.x = Mathf.Clamp(angles.x + difference.y / 5f, 0, 90);
                    _newRigArmRotation.eulerAngles = angles;
                }

                _rotateStartPosition = Input.mousePosition;

                CursorController.Current.RotateCamera = true;
            }
        }

        #endregion

    }
}