using UnityEngine;
using UnityEngine.UI;

namespace TT.UI.MainMenu
{
    [RequireComponent(typeof(Button))]
    [RequireComponent(typeof(CanvasGroup))]
    public class LogoutButton : MonoBehaviour
    {
        private Button _button;
        private CanvasGroup _canvasGroup;

        private void OnEnable()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(HandleButtonClick);

            bool userIsLoggedIn = Helpers.Comms.User.IsLoggedIn;
            _canvasGroup = GetComponent<CanvasGroup>();
            ToggleButton(userIsLoggedIn);

            // Add handlers
            Helpers.Comms.User.OnLoginSuccess += OnLoginSuccess;
            Helpers.Comms.User.OnLogout += OnLogout;
        }
        
        private void OnDisable()
        {
            if (Helpers.Comms != null && Helpers.Comms.User != null)
            {
                // Remove handlers
                Helpers.Comms.User.OnLoginSuccess -= OnLoginSuccess;
                Helpers.Comms.User.OnLogout -= OnLogout;
            }
        }

        private void OnLogout()
        {
            ToggleButton(false);
        }

        private void OnLoginSuccess()
        {
            ToggleButton(true);
        }

        private void ToggleButton(bool userIsLoggedIn)
        {
            _canvasGroup.alpha = userIsLoggedIn ? 1 : 0;
            _canvasGroup.interactable = userIsLoggedIn;
            _canvasGroup.blocksRaycasts = userIsLoggedIn;
        }

        private void HandleButtonClick()
        {
            if (Helpers.Comms.User.IsLoggedIn)
            {
                Helpers.Comms.User.Logout();
            }
        }

    }
}