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
using TMPro;
using TT.Shared.UserContent;
using UnityEngine;

namespace TT.UI.Load
{
    public class MapListDetails : MonoBehaviour
    {
        [SerializeField] private TMP_Text nameLabel;
        [SerializeField] private TMP_Text authorLabel;
        [SerializeField] private TMP_Text descriptionLabel;
        [SerializeField] private TMP_Text createDateLabel;
        [SerializeField] private TMP_Text saveDateLabel;

        public void ShowMapDetails(MapMetadata mapMetadata)
        {
            nameLabel.text = mapMetadata.name;
            authorLabel.text = mapMetadata.authorName;
            descriptionLabel.text = mapMetadata.description;
            createDateLabel.text = DateTime.FromFileTimeUtc(mapMetadata.dateCreated).ToString("g");
            saveDateLabel.text = DateTime.FromFileTimeUtc(mapMetadata.dateSaved).ToString("g");
        }
    }
}