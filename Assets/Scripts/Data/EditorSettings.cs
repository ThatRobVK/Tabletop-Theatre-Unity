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

using HighlightPlus;
using UnityEngine;
using UnityEngine.Serialization;

namespace TT.Data
{
    [CreateAssetMenu(fileName = "EditorSettings", menuName = "MOD/Editor Settings", order = 3)]
    public class EditorSettings : ScriptableObject
    {
        [Header("Editor Behaviour")]
        [FormerlySerializedAs("SnapToGrid")] public bool snapToGrid = true;
        [FormerlySerializedAs("ContinuousPlacement")] public bool continuousPlacement;
        [FormerlySerializedAs("ShowPolygons")] public bool showPolygons;

        [Header("Item Controls")]
        [FormerlySerializedAs("RotateSpeed")] public float rotateSpeed = 40f;
        [FormerlySerializedAs("RotateStepSpeed")] public float rotateStepSpeed = 1f;
        [FormerlySerializedAs("RotateSpeedFast")] public float rotateSpeedFast = 120f;
        [FormerlySerializedAs("RotateStepSpeedFast")] public float rotateStepSpeedFast = 5f;
        [FormerlySerializedAs("RotateSnapToGridSpeed")] public float rotateSnapToGridSpeed = 90f;
        [FormerlySerializedAs("ElevateSpeed")] public float elevateSpeed = 1f;
        [FormerlySerializedAs("ElevateStepSpeed")] public float elevateStepSpeed = 0.01f;
        [FormerlySerializedAs("ElevateSpeedFast")] public float elevateSpeedFast = 3f;
        [FormerlySerializedAs("ElevateStepSpeedFast")] public float elevateStepSpeedFast = 0.1f;
        [FormerlySerializedAs("ElevateSnapToGridSpeed")] public float elevateSnapToGridSpeed = 1f;
        [FormerlySerializedAs("MoveSpeed")] public float moveSpeed = 1f;
        [FormerlySerializedAs("MoveStepSpeed")] public float moveStepSpeed = 0.01f;
        [FormerlySerializedAs("MoveSpeedFast")] public float moveSpeedFast = 3f;
        [FormerlySerializedAs("MoveStepSpeedFast")] public float moveStepSpeedFast = 0.1f;
        [FormerlySerializedAs("MoveSnapToGridSpeed")] public float moveSnapToGridSpeed = 1f;
        [FormerlySerializedAs("ScaleSpeed")] public float scaleSpeed = 0.5f;
        [FormerlySerializedAs("ScaleStepSpeed")] public float scaleStepSpeed = 0.01f;
        [FormerlySerializedAs("ScaleSpeedFast")] public float scaleSpeedFast = 1f;
        [FormerlySerializedAs("ScaleStepSpeedFast")] public float scaleStepSpeedFast = 0.1f;

        [Header("Highlighter")]
        [FormerlySerializedAs("HoverHighlightProfile")] public HighlightProfile hoverHighlightProfile;
        [FormerlySerializedAs("SelectedHighlightProfile")] public HighlightProfile selectedHighlightProfile;
        [FormerlySerializedAs("DeniedHighlightProfile")] public HighlightProfile deniedHighlightProfile;

        [Header("Materials")]
        [FormerlySerializedAs("PolygonMaterial")] public Material polygonMaterial;

        [Header("Debug")]
        [FormerlySerializedAs("DebugRaycasts")] public bool debugRaycasts;
        [FormerlySerializedAs("CompressSaves")] public bool compressSaves = true;
        [FormerlySerializedAs("ShowItemIDs")] public bool showItemIDs;
        [FormerlySerializedAs("ShowFpsCounter")] public bool showFpsCounter;
    }
}