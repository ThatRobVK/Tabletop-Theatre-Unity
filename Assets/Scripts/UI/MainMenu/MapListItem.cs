using System;
using TMPro;
using UnityEngine;

namespace TT.UI.MainMenu
{
    public class MapListItem : MonoBehaviour
    {
        [SerializeField] private TMP_Text mapNameText;
        [SerializeField] private TMP_Text saveDateText;

        public void Initialise(string mapName, DateTime saveDate)
        {
            mapNameText.text = mapName;
            saveDateText.text = saveDate.ToString("hh:mm on dd/MM/yyyy");
        }
    }
}