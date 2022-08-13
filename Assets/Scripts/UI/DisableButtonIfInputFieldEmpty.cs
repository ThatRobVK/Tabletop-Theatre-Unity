using UnityEngine;
using UnityEngine.UI;

namespace TT.UI
{
    /// <summary>
    /// When attached to a button, the button is disabled when the specified input field is empty, otherwise enabled.
    /// </summary>
    [RequireComponent(typeof(ToggledButton))]
    public class DisableButtonIfInputFieldEmpty : MonoBehaviour
    {
        
        [SerializeField][Tooltip("The input field to check.")] private InputField targetInputfield;
        [SerializeField][Tooltip("When ticked, the button is enabled whenever the input field isn't empty.")] private bool enableWhenNotEmpty;

        private ToggledButton _toggledButton;

        private void Start()
        {
            _toggledButton = GetComponent<ToggledButton>();
        }

        private void Update()
        {
            // If the button is enabled but the text is empty, disable the button
            if (_toggledButton.Enabled && string.IsNullOrEmpty(targetInputfield.text))
                _toggledButton.Enabled = false;

            if (enableWhenNotEmpty && !string.IsNullOrEmpty(targetInputfield.text))
                _toggledButton.Enabled = true;
        }
    }
}