using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    // ------------------------- VARIABLES -------------------------

    // Reference to the PrivatePrinceControls script that handles the new input system controls
    private PrivatePrinceControls ppControls;

    private int currentSelectedIndex = 0; // Index to track the currently selected button for input navigation
    private bool isNavigationActive = true; // Flag to track if navigation is currently active

    [Header("MENU COMPONENTS")]
    public Animator animatorUI;

    [Header("Navigation Settings")]
    [SerializeField] private bool enableInputNavigation = true;
    [SerializeField] private bool wrapAround = true;

    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color selectedColor = Color.yellow;

    [Header("Button References")]
    [SerializeField] private Button[] menuButtons; // Drag ALL your menu buttons here

    // ------------------------- UNITY METHODS -------------------------

    // ...
    private void Start()
    {
        // Find the first active button to start selection
        FindFirstActiveButton();
    }

    // ...
    void OnEnable()
    {
        SubscribeToControls();
    }

    // ...
    void OnDisable()
    {
        UnSubscribeToControls();
        isNavigationActive = false;
    }

    // ------------------------- EVENT METHODS -------------------------

    // ...
    private void SubscribeToControls()
    {
        if (GameplayInputManager.Instance != null)
        {
            ppControls = GameplayInputManager.Instance.Controls;

            // Unsubscribe first to prevent double or multiple subscriptions 
            UnSubscribeToControls();

            ppControls.UserNavigation.NavigateUI.performed += OnNavigatePerformed;
            ppControls.UserNavigation.Interact.performed += OnSubmitPerformed;
            ppControls.UserNavigation.Interact.performed += TriggerSplashScreens;
            //ppControls.UserNavigation.Cancel.performed += OnCancelPerformed;
        }
        else
        {
            Debug.LogError("PlayerInputManager instance not found. Ensure PlayerInputManager is present in the scene.");
        }
    }

    // ...
    private void UnSubscribeToControls()
    {
        if (ppControls != null)
        {
            ppControls.UserNavigation.NavigateUI.performed -= OnNavigatePerformed;
            ppControls.UserNavigation.Interact.performed -= OnSubmitPerformed;
            ppControls.UserNavigation.Interact.performed -= TriggerSplashScreens;
            //ppControls.UserNavigation.Cancel.performed -= OnCancelPerformed;
        }
    }

    // ------------------------- CHECKER METHODS ------------------------- 

    // ...
    private void FindFirstActiveButton()
    {
        if (menuButtons.Length == 0) return;

        // Find the first active and interactable button
        for (int i = 0; i < menuButtons.Length; i++)
        {
            if (menuButtons[i] != null && menuButtons[i].isActiveAndEnabled && menuButtons[i].interactable)
            {
                currentSelectedIndex = i;
                UpdateButtonHighlight(currentSelectedIndex, true);
                break;
            }
        }
    }

    private bool IsButtonValid(Button button)
    {
        return button != null && button.isActiveAndEnabled && button.interactable;
    }

    // ------------------------- NAVIGATION METHODS ------------------------- 

    // Method to handle navigation input
    private void OnNavigatePerformed(InputAction.CallbackContext context)
    {
        if (!enableInputNavigation || !isNavigationActive || menuButtons.Length == 0) return;

        Vector2 navigationInput = context.ReadValue<Vector2>();

        if (navigationInput.y > 0.5f) // Up Input
        {
            NavigateToPreviousBTN();
        }
        else if (navigationInput.y < -0.5f) // Bottom Input
        {
            NavigateToNextBTN();
        }
        else if (navigationInput.x < -0.5f) // Left Input
        {
            NavigateToPreviousBTN();
        }
        else if (navigationInput.x > 0.5f) // Right Input
        {
            NavigateToNextBTN();
        }
    }

    // ...
    private void NavigateToPreviousBTN()
    {
        if (menuButtons.Length == 0) return;

        // Unhighlight current button
        UpdateButtonHighlight(currentSelectedIndex, false);

        int startIndex = currentSelectedIndex;

        // Find previous active button
        do
        {
            currentSelectedIndex--;

            if (currentSelectedIndex < 0)
            {
                currentSelectedIndex = wrapAround ? menuButtons.Length - 1 : 0;
            }

            // Check if we've looped all the way around
            if (currentSelectedIndex == startIndex)
            {
                Debug.Log("No other active buttons found");
                UpdateButtonHighlight(currentSelectedIndex, true);
                return;
            }

        } while (!IsButtonValid(menuButtons[currentSelectedIndex]));

        // Highlight new button
        UpdateButtonHighlight(currentSelectedIndex, true);
        Debug.Log($"MenuManager: Selected option {currentSelectedIndex + 1}: {menuButtons[currentSelectedIndex].name}");
    }

    // ...
    private void NavigateToNextBTN()
    {
        if (menuButtons.Length == 0) return;

        // Unhighlight current button
        UpdateButtonHighlight(currentSelectedIndex, false);

        int startIndex = currentSelectedIndex;

        // Find next active button
        do
        {
            currentSelectedIndex++;

            if (currentSelectedIndex >= menuButtons.Length)
            {
                currentSelectedIndex = wrapAround ? 0 : menuButtons.Length - 1;
            }

            // Check if we've looped all the way around
            if (currentSelectedIndex == startIndex)
            {
                Debug.Log("No other active buttons found");
                UpdateButtonHighlight(currentSelectedIndex, true);
                return;
            }

        } while (!IsButtonValid(menuButtons[currentSelectedIndex]));

        // Highlight new button
        UpdateButtonHighlight(currentSelectedIndex, true);
        Debug.Log($"MenuManager: Selected option {currentSelectedIndex + 1}: {menuButtons[currentSelectedIndex].name}");
    }

    // ...
    private void UpdateButtonHighlight(int index, bool selected)
    {
        if (index < 0 || index >= menuButtons.Length) return;
        if (menuButtons[index] == null) return;

        Button button = menuButtons[index];

        // Create a color block to modify colors if needed
        ColorBlock colors = button.colors;

        if (selected)
        {
            // Method 1: Use Select() - this triggers the button's selected state
            button.Select();

            // Optional: If you want to ensure it stays highlighted
            // button.OnSelect(null); // This manually triggers the selection highlight

            Debug.Log($"Button {button.name} highlighted");
        }
        else
        {
            // Method 2: If you want to manually control the transition
            if (EventSystem.current != null)
            {
                // Only deselect if this button is currently selected
                if (EventSystem.current.currentSelectedGameObject == button.gameObject)
                {
                    EventSystem.current.SetSelectedGameObject(null);
                }
            }

            Debug.Log($"Button {button.name} unhighlighted");
        }
    }

    // ------------------------- INTERACTION METHODS ------------------------- 

    // ...
    private void OnSubmitPerformed(InputAction.CallbackContext context)
    {
        if (!enableInputNavigation || !isNavigationActive) return;

        if (currentSelectedIndex >= 0 && currentSelectedIndex < menuButtons.Length)
        {
            Button selectedButton = menuButtons[currentSelectedIndex];

            // Make sure the button is valid before invoking
            if (IsButtonValid(selectedButton))
            {
                selectedButton.onClick.Invoke();
                Debug.Log($"MenuManager: Submitted button: {selectedButton.name}");
            }
        }
    }

    // ...
    private void TriggerSplashScreens(InputAction.CallbackContext context) 
    {
        animatorUI.SetTrigger("isInteracted");
    }

    //// ...
    //private void StopAnim() 
    //{
    //    animatorUI.StopPlayback();
    //}
}