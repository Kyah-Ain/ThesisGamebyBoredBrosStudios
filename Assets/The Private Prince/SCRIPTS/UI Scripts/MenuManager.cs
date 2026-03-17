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

    enum DefaultState 
    {
        AutoSelectFirstButton,
        ManualSelectFirstButton
    }

    [SerializeField] private DefaultState state = DefaultState.AutoSelectFirstButton;

    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color selectedColor = Color.yellow;

    [Header("Button References")]
    [SerializeField] private Button startingButton; // Drag your default selected menu buttons here
    [SerializeField] private Button[] menuButtons; // Drag ALL your menu buttons here

    // ------------------------- UNITY METHODS -------------------------

    // ...
    private void Start()
    {
        if (state == DefaultState.AutoSelectFirstButton)
        {
            // Automatically find and select the first active button
            FindFirstActiveButton();
        }
        else
        {
            // Manually set the default selected button
            if (startingButton != null)
            {
                SetDefaultButton(startingButton);
            }
            else
            {
                Debug.LogWarning("Starting button not assigned! Falling back to auto selection.");
                FindFirstActiveButton();
            }
        }
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
            ppControls.UserNavigation.Interact.performed += TriggerSkip;
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
            ppControls.UserNavigation.Interact.performed -= TriggerSkip;
            //ppControls.UserNavigation.Cancel.performed -= OnCancelPerformed;
        }
    }

    // ------------------------- STARTER METHODS ------------------------- 

    // ...
    private void SetDefaultButton(Button startingButton)
    {
        if (menuButtons.Length == 0) return;

        // Find the index of the starting button in the menuButtons array
        for (int i = 0; i < menuButtons.Length; i++)
        {
            if (menuButtons[i] == startingButton)
            {
                currentSelectedIndex = i;
                break;
            }
        }

        // Make sure the button is valid before selecting
        if (IsButtonValid(menuButtons[currentSelectedIndex]))
        {
            UpdateButtonHighlight(currentSelectedIndex, true);
            Debug.Log($"MenuManager: Manually set default button: {menuButtons[currentSelectedIndex].name}");
        }
        else
        {
            Debug.LogWarning("Starting button is not valid! Finding first active button instead.");
            FindFirstActiveButton();
        }
    }


    // ...
    private void FindFirstActiveButton()
    {
        if (menuButtons.Length == 0) return;

        // Find the first active and interactable button
        for (int i = 0; i < menuButtons.Length; i++)
        {
            if (IsButtonValid(menuButtons[i]))
            {
                currentSelectedIndex = i;
                UpdateButtonHighlight(currentSelectedIndex, true);
                Debug.Log($"MenuManager: Auto-selected button: {menuButtons[i].name}");
                return;
            }
        }
        
        Debug.LogWarning("No active buttons found in menu!");
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
            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(button.gameObject);
            }

            // Method 1: Use Select() - this triggers the button's selected state
            button.Select();

            // Optional: Force the button to be in highlighted state
            button.OnSelect(null);

            // Optional: If you want to ensure it stays highlighted
            // button.OnSelect(null); // This manually triggers the selection highlight

            Debug.Log($"Button {button.name} highlighted");
        }
        //else
        //{
        //    // Method 2: If you want to manually control the transition
        //    if (EventSystem.current != null)
        //    {
        //        // Only deselect if this button is currently selected
        //        if (EventSystem.current.currentSelectedGameObject == button.gameObject)
        //        {
        //            EventSystem.current.SetSelectedGameObject(null);
        //        }
        //    }

        //    Debug.Log($"Button {button.name} unhighlighted");
        //}
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

    // Method for triggering 
    public void SkipAnimation()
    {
        animatorUI.SetTrigger("isInteracted");
    }

    // ...
    public void TriggerSkip(InputAction.CallbackContext context) 
    {
        SkipAnimation();
    }

    //// ...
    //private void StopAnim() 
    //{
    //    animatorUI.StopPlayback();
    //}
}