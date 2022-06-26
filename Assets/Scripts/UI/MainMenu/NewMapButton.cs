using System.Collections.Generic;
using System.Linq;
using DuloGames.UI;
using TT.CommsLib;
using TT.Data;
using TT.Shared.UserContent;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TT.UI.MainMenu
{
    [RequireComponent(typeof(Button))]
    public class NewMapButton : MonoBehaviour
    {
        [SerializeField] private Textbox nameTextbox;
        [SerializeField] private Textbox descriptionTextbox;
        [SerializeField] private UISelectField terrainDropdown;
        
        private Button _button;

        private void Start()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(HandleButtonClicked);
        }

        /// <summary>
        /// Called when the attached button is clicked. Create a new map and load the map editor scene.
        /// </summary>
        private void HandleButtonClicked()
        {
            var selectedTerrain = terrainDropdown.options[terrainDropdown.selectedOptionIndex];
            var terrainLayer = Content.Current.Combined.TerrainLayers.FirstOrDefault(x => x.Name == selectedTerrain);
            var terrainId = terrainLayer != null ? terrainLayer.ID : "0";
            
            Map.New(nameTextbox.text, descriptionTextbox.text, terrainId);

            SceneManager.LoadScene("MapEditor", LoadSceneMode.Single);
        }
    }
}