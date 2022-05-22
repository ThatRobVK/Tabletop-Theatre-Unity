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

using TT.World;
using UnityEngine;
using UnityEngine.UI;

namespace TT.UI.MapEditor.ObjectProperties
{
    [RequireComponent(typeof(Button))]
    [RequireComponent(typeof(Image))]
    public class StarButton : MonoBehaviour
    {
        [SerializeField][Tooltip("The image to show when the current item has been starred.")] private Sprite starredSprite;
        [SerializeField][Tooltip("The image to show when the current item is not starred.")] private Sprite notStarredSprite;


        private Image _image;

        // Start is called before the first frame update
        void Start()
        {
            GetComponent<Button>().onClick.AddListener(HandleButtonClicked);
            _image = GetComponent<Image>();
        }

        // Update is called once per frame
        void Update()
        {
            if (WorldObjectBase.Current)
            {
                _image.sprite = WorldObjectBase.Current.Starred ? starredSprite : notStarredSprite;
            }
        }

        private void HandleButtonClicked()
        {
            if (WorldObjectBase.Current) WorldObjectBase.Current.Starred = !WorldObjectBase.Current.Starred;
        }
    }
}