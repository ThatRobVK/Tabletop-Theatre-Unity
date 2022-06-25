using DuloGames.UI;
using TT.Data;
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
            Map.New(nameTextbox.text, descriptionTextbox.text);
            SceneManager.LoadScene("MapEditor", LoadSceneMode.Single);
        }
    }
}