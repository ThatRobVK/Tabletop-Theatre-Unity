using System;
using System.Collections.Generic;
using DuloGames.UI;
using TMPro;
using TT.Shared;
using UnityEngine;
using UnityEngine.UI;

namespace TT.UI.Login
{
    [RequireComponent(typeof(UIWindow))]
    public class LoginWindow : MonoBehaviour
    {
        [SerializeField][Tooltip("The textbox containing the username.")] private Textbox username;
        [SerializeField][Tooltip("The textbox containing the password.")] private Textbox password;
        [SerializeField][Tooltip("The panel overlay to tell people to wait during login.")] private GameObject waitPanel;
        [SerializeField][Tooltip("The area where error messages are shown.")] private GameObject errorArea;
        [SerializeField][Tooltip("The text field where error messages are shown.")] private TMP_Text errorText;
        [SerializeField] [Tooltip("The cancel button that hides this window.")] private Button cancelButton;
        

        private UIWindow _window;
        
        private Dictionary<LoginFailureReason, string> failureReasonToTextMap = new Dictionary<LoginFailureReason, string>()
        {
            {LoginFailureReason.AuthenticationFailed, "Login failed. Check your credentials and try again."},
            {
                LoginFailureReason.ConnectionError,
                "Could not connect to server. Please try again later or check the website for server status."
            },
            {
                LoginFailureReason.EmailNotVerified,
                "E-mail address not verified. Check your e-mail or go to https://www.tabletop-theatre.com/verify-email/"
            }
        };


        private void OnEnable()
        {
            _window = GetComponent<UIWindow>();
            _window.onTransitionBegin.AddListener(OnWindowTransitionBegin);
            if (cancelButton) cancelButton.onClick.AddListener(HideWindow);
            
            // Listen for auth events
            Helpers.Comms.User.OnLoginSuccess += HandleLoginSuccess;
            Helpers.Comms.User.OnLoginFailed += HandleLoginFailed;
        }

        private void HideWindow()
        {
            _window.Hide();
        }

        private void OnDisable()
        {
            if (_window)
                _window.onTransitionBegin.RemoveListener(OnWindowTransitionBegin);

            if (Helpers.Comms != null && Helpers.Comms.User != null)
            {
                Helpers.Comms.User.OnLoginSuccess -= HandleLoginSuccess;
                Helpers.Comms.User.OnLoginFailed -= HandleLoginFailed;
            }
        }

        private void OnWindowTransitionBegin(UIWindow window, UIWindow.VisualState newState, bool instant)
        {
            if (newState == UIWindow.VisualState.Shown)
            {
                // Reset the panel
                username.text = string.Empty;
                password.text = string.Empty;
                waitPanel.SetActive(false);
                errorArea.SetActive(false);
                errorText.text = string.Empty;
            }
        }

        private void HandleLoginFailed(LoginFailureReason reason)
        {
            // Show the error
            errorText.text = failureReasonToTextMap[reason];
            errorArea.SetActive(true);
            
            // Hide wait panel
            waitPanel.SetActive(false);
        }

        private void HandleLoginSuccess()
        {
            HideWindow();
        }

        public void ToggleWaitPanel(bool show)
        {
            // Show the wait panel
            waitPanel.SetActive(show);
        }
    }
}