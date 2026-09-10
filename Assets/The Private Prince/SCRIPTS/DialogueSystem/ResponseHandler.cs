using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class ResponseHandler : MonoBehaviour
{
    [Header("Input Navigation")]
    [SerializeField] private bool enableInputNavigation = true; // Flag to enable/disable input navigation for responses
    [SerializeField] private Color normalColor = Color.white; // Color for unselected response buttons
    [SerializeField] private Color selectedColor = Color.yellow; // Color for the selected response button

    [Header("UI References")]
    [SerializeField] private RectTransform responseBox; // The container for the response buttons, used to adjust size based on number of responses
    [SerializeField] private RectTransform responseButtonTemplate; // Template for response buttons, used to instantiate new buttons for each response
    [SerializeField] private RectTransform responseContainer; // The parent container where response buttons will be instantiated

    private DialogueUI dialogueUI; // Reference to the DialogueUI component for showing dialogues and closing the dialogue box
    private ResponseEvent[] responseEvents; // Array of response events that can be assigned in the inspector, used to trigger specific actions when a response is picked
    private List<GameObject> tempResponseButton = new List<GameObject>(); // List to keep track of the instantiated response buttons so they can be easily destroyed when a new set of responses is shown or when a response is picked
    private List<Button> responseButtons = new List<Button>(); // List to keep track of the Button components of the instantiated response buttons for handling input navigation and selection


    private PrivatePrinceControls ppControls; // Reference to the PrivatePrinceControls script for handling input actions
    private int currentSelectedIndex = 0; // Index to track the currently selected response button for input navigation
    private bool isResponseActive = false; // Flag to track if the response options are currently active, used to enable/disable input navigation
    private bool navigationInputProcessed = false; // Flag to ensure that navigation input is only processed once per button press, preventing rapid changes in selection when holding down the navigation input

    private void Start()
    {
        dialogueUI = GetComponent<DialogueUI>();

        if (GameplayInputManager.Instance != null) 
        {
            ppControls = GameplayInputManager.Instance.Controls;

            SubscribeToInputEvents();
        }
    }

    // ------------------------- EVENT METHODS ------------------------- 

    // ...
    private void SubscribeToInputEvents() 
    {
        if (ppControls == null) return;

        // Unsubscribe first to prevent double or multiple subscriptions 
        UnsubscribeFromInputEvents();

        ppControls.UI.NavigateUI.performed += OnNavigatePerformed;
        ppControls.UI.Proceed.performed += OnSubmitPerformed;
        ppControls.UI.Cancel.performed += OnCancelPerformed;

        Debug.Log("ResponseHandler: Subscribed to input events");
    }

    // ...
    private void UnsubscribeFromInputEvents() 
    {
        if (ppControls == null) return;

        ppControls.UI.NavigateUI.performed -= OnNavigatePerformed;
        ppControls.UI.Proceed.performed -= OnSubmitPerformed;
        ppControls.UI.Cancel.performed -= OnCancelPerformed;

        Debug.Log("ResponseHandler: Unsbscribed to input events");
    }

    // ...
    public void AddResponseEvents(ResponseEvent[] responseEvents)
    {
        this.responseEvents = responseEvents;
    }

    // ------------------------- NAVIGATION METHODS ------------------------- 

    // Method to handle navigation input
    private void OnNavigatePerformed(InputAction.CallbackContext context) 
    {
        if (!isResponseActive || !enableInputNavigation) return;

        Vector2 navigationInput = context.ReadValue<Vector2>();

        if (navigationInput.y > 0.5f) // Up Input
        {
            NavigateToPreviousResponse();
        }
        else if (navigationInput.y < -0.5f) // Bottom Input
        {
            NavigateToNextResponse();
        }
        else if (navigationInput.x < -0.5f) // Left Input
        {
            NavigateToPreviousResponse(); 
        }
        else if (navigationInput.x > 0.5f) // Right Input
        {
            NavigateToNextResponse();
        }
    }

    // ...
    private void NavigateToPreviousResponse() 
    {
        if (responseButtons.Count == 0) return;

        UpdateButtonHighlight(currentSelectedIndex, false);

        currentSelectedIndex--;

        if (currentSelectedIndex < 0)
        {
            currentSelectedIndex = responseButtons.Count - 1; 
        }

        UpdateButtonHighlight(currentSelectedIndex, true);

        Debug.Log($"ResponseHandler: you selected option {currentSelectedIndex + 1}");
    }

    // ...
    private void NavigateToNextResponse()
    {
        if (responseButtons.Count == 0) return;

        UpdateButtonHighlight(currentSelectedIndex, false);

        currentSelectedIndex++;

        if (currentSelectedIndex >= responseButtons.Count)
        {
            currentSelectedIndex = 0; // Wrap to first option
        }

        UpdateButtonHighlight(currentSelectedIndex, true);

        Debug.Log($"ResponseHandler: you selected option {currentSelectedIndex + 1}");
    }

    // ...
    private void UpdateButtonHighlight(int index, bool selected) 
    {
        if (index < 0 || index >= responseButtons.Count) return;

        GameObject buttonObj = tempResponseButton[index];
        TMP_Text buttonTMP = responseButtons[index].GetComponent<TMP_Text>();

        if (buttonTMP != null)
        {
            // Change the text color directly
            buttonTMP.color = selected ? selectedColor : normalColor;

            //// Optional: Add some visual feedback like bold or font size change
            //if (selected)
            //{
            //    buttonTMP.fontStyle = FontStyles.Bold;
            //    // Optional: slightly increase font size when selected
            //    // buttonText.fontSize = buttonText.fontSize + 2;
            //}
            //else
            //{
            //    buttonTMP.fontStyle = FontStyles.Normal;
            //    // Optional: revert font size
            //    // buttonText.fontSize = buttonText.fontSize - 2;
            //}
        }

        // Optional: Also highlight the button background if you have an Image component
        Image buttonImage = buttonObj.GetComponent<Image>();
        if (buttonImage != null)
        {
            buttonImage.color = selected ? selectedColor : normalColor;
        }
    }


    // ------------------------- INTERACTION METHODS ------------------------- 

    // ...
    private void OnSubmitPerformed(InputAction.CallbackContext context)
    {
        if (!isResponseActive || !enableInputNavigation) return;

        // Checks if:
        // - There are response buttons available
        // - The selected index is not negative 
        // - The current selected index is within the valid range of response buttons
        if (responseButtons.Count > 0 && 
            currentSelectedIndex >= 0 && 
            currentSelectedIndex < responseButtons.Count) 
        {
            // Simulate a click on the currently selected response button
            responseButtons[currentSelectedIndex].onClick.Invoke();

            // Provides feedback
            Debug.Log($"Selected response {currentSelectedIndex} via input");
        }
    }

    // ...
    private void OnCancelPerformed(InputAction.CallbackContext context)
    {
        if (!isResponseActive || !enableInputNavigation) return;

        Debug.Log("Cancel pressed while responses active");

        SelectDeclineOption();
    }

    // ...
    private void SelectDeclineOption() 
    {
        for (int i = 0; i < tempResponseButton.Count; i++)
        {
            string responseText = tempResponseButton[i].GetComponent<TMP_Text>().text.ToLower();
            
            if (responseText.Contains("not now") ||
                responseText.Contains("not right now") ||
                responseText.Contains("maybe later") ||
                responseText.Contains("decline") ||
                responseText.Contains("cancel") ||
                responseText.Contains("no"))
            {
                UpdateButtonHighlight(currentSelectedIndex, false);

                currentSelectedIndex = i;

                UpdateButtonHighlight(currentSelectedIndex, true);
            }
        }
    }

    // ---------------------------- DIALOGUE METHODS ------------------------- 

    // ...
    public void ShowResponses(Response[] responses)
    {
        ClearResponseButtons();

        float responseBoxHeight = 0;
        currentSelectedIndex = 0;
        isResponseActive = true;

        for (int i = 0; i < responses.Length; i++)
        {
            Response response = responses[i];
            int responseIndex = i;

            GameObject responseButton = Instantiate(responseButtonTemplate.gameObject, responseContainer);
            responseButton.gameObject.SetActive(true);

            TMP_Text buttonText = responseButton.GetComponent<TMP_Text>();
            Button button = responseButton.GetComponent<Button>();

            buttonText.text = response.ResponseText;
            buttonText.color = normalColor;

            button.onClick.AddListener(() => OnPickedResponse(response, responseIndex));

            tempResponseButton.Add(responseButton);
            responseButtons.Add(button);

            responseBoxHeight += responseButtonTemplate.sizeDelta.y;
        }

        responseBox.sizeDelta = new Vector2(responseBox.sizeDelta.x, responseBoxHeight);
        responseBox.gameObject.SetActive(true);

        if (enableInputNavigation && responseButtons.Count > 0) 
        {
            UpdateButtonHighlight(0, true);
        }
    }

    // ...
    private void OnPickedResponse(Response response, int responseIndex)
    {
        isResponseActive = false;
        responseBox.gameObject.SetActive(false);

        ClearResponseButtons();

        // Store whether this response should reset dialogue
        bool shouldResetDialogue = false;

        if (responseEvents != null && responseIndex <= responseEvents.Length)
        {
            responseEvents[responseIndex].OnPickedResponse?.Invoke();

            // Check if this is a "decline" response that should reset
            if (response.ResponseText.ToLower().Contains("not now") ||
                response.ResponseText.ToLower().Contains("not right now") ||
                response.ResponseText.ToLower().Contains("maybe later") ||
                response.ResponseText.ToLower().Contains("decline") ||
                response.ResponseText.ToLower().Contains("cancel") ||
                response.ResponseText.ToLower().Contains("no"))
            {
                shouldResetDialogue = true;
                Debug.Log($"Detected decline response: {response.ResponseText}");
            }
        }

        responseEvents = null;

        if (response.DialogueObject)
        {
            dialogueUI.ShowDialogue(response.DialogueObject);
        }
        else
        {
            // Reset dialogue for decline responses
            if (shouldResetDialogue)
            {
                ResetDialogueToDefault();
            }
            dialogueUI.CloseDialogueBox();
        }
    }

    // -------------------------- RESET METHODS -------------------------

    // ...
    private void ResetDialogueToDefault()
    {
        DialogueActivator activator = FindObjectOfType<DialogueActivator>();
        if (activator != null)
        {
            NPCDialogueController dialogueController = activator.GetComponent<NPCDialogueController>();
            if (dialogueController != null)
            {
                dialogueController.ResetToDefault();
            }
        }
    }

    // ...
    private void ClearResponseButtons()
    {
        foreach (GameObject button in tempResponseButton)
        {
            Destroy(button);
        }

        tempResponseButton.Clear();
        responseButtons.Clear();
    }

    // -------------------------- CLEANUP METHODS -------------------------

    // Unity Built-in method called when this component is disabled
    private void OnDisable()
    {
        // Clean up subscriptions & Reset responsive state
        UnsubscribeFromInputEvents();
        isResponseActive = false;
    }

    // Unity Built-in method called when this component is destroyed
    private void OnDestroy()
    {
        // Clean up subscriptions
        UnsubscribeFromInputEvents();
    }
}