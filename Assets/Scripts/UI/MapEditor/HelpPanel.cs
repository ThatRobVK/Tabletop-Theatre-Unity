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

using DuloGames.UI;
using TT.Data;
using TT.State;
using TT.World;
using UnityEngine;

namespace TT.UI.MapEditor
{
    public class HelpPanel : MonoBehaviour
    {
        [Header("Key bindings panels")]
        [SerializeField] private GameObject controlsIdle;
        [SerializeField] private GameObject controlsPaint;
        [SerializeField] private GameObject controlsDraggableSelected;
        [SerializeField] private GameObject controlsDraggablePlacement;
        [SerializeField] private GameObject controlsRamSelected;
        [SerializeField] private GameObject controlsRamPlacement;

        [Header("Tips panels")]
        [SerializeField] private GameObject tipsIdle;
        [SerializeField] private GameObject tipsTerrain;
        [SerializeField] private GameObject tipsVegetation;
        [SerializeField] private GameObject tipsConstruction;
        [SerializeField] private GameObject tipsItems;
        [SerializeField] private GameObject tipsLights;

        [Header("Windows")]
        [SerializeField] private UIWindow terrainWindow;
        [SerializeField] private UIWindow vegetationWindow;
        [SerializeField] private UIWindow constructionWindow;
        [SerializeField] private UIWindow itemsWindow;
        [SerializeField] private UIWindow lightsWindow;

        void Update()
        {
            var painting = StateController.CurrentStateType == StateType.TerrainPaint;
            var placement = StateController.CurrentState.IsPlacementState;
            var draggableSelected = WorldObjectBase.Current && WorldObjectBase.Current.IsDraggable;
            var ramSelected = WorldObjectBase.Current && (WorldObjectBase.Current.Type == WorldObjectType.River || WorldObjectBase.Current.Type == WorldObjectType.Road || WorldObjectBase.Current.Type == WorldObjectType.ScatterArea);

            controlsIdle.SetActive(!painting && !placement && !draggableSelected && !ramSelected);
            controlsPaint.SetActive(painting);
            controlsDraggablePlacement.SetActive(placement && draggableSelected);
            controlsDraggableSelected.SetActive(!placement && draggableSelected);
            controlsRamPlacement.SetActive(placement && ramSelected);
            controlsRamSelected.SetActive(!placement && ramSelected);

            tipsIdle.SetActive(!terrainWindow.IsOpen && !vegetationWindow.IsOpen && !constructionWindow.IsOpen && !itemsWindow.IsOpen && !lightsWindow.IsOpen);
            tipsTerrain.SetActive(terrainWindow.IsOpen);
            tipsVegetation.SetActive(vegetationWindow.IsOpen);
            tipsConstruction.SetActive(constructionWindow.IsOpen);
            tipsItems.SetActive(itemsWindow.IsOpen);
            tipsLights.SetActive(lightsWindow.IsOpen);
        }
    }
}
