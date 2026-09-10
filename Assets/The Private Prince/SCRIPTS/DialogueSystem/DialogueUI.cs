using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class DialogueUI : MonoBehaviour
{
    // ------------------------- VARIABLES -------------------------

    // Reference to the PrivatePrinceControls script that handles the new input system controls
    private PrivatePrinceControls ppControls;

    [SerializeField] private GameObject dialogueBox; // The dialogue box UI element
    [SerializeField] private TMP_Text textLabel; // The text field for displaying dialogue

    public bool IsOpen { get; private set; } // Public flag to check if dialogue is open
    public bool dialogueFinished; // Flag to indicate if dialogue has finished (used for response handling)

    private ResponseHandler responseHandler; // Handles showing and reacting to responses
    private TypeWriterEffect typeWriterEffect; // Handles text typing animation
    private Coroutine currentDialogueCoroutine; // Keeps track of the active dialogue coroutine

    private bool interactPressed = false; // Flag to track if the interact button was pressed (for Traversing through dialogue)
    private bool cancelPressed = false; // Flag to track if the cancel button was pressed (for Cancelling the dialogue)

    [Header("OPTIONAL EVENTS")]
    public GameEvent onDialogueBoxClosedGlobal;

    // ------------------------- UNITY METHODS -------------------------

    public void Awake()
    {
        if (GameplayInputManager.Instance != null) 
        {
            ppControls = GameplayInputManager.Instance.Controls;

            // Subscribe to the Interact and Cancel actions' performed events
            ppControls.UI.Proceed.performed += OnInteractPerformed;
            ppControls.UI.Cancel.performed += OnCancelPerformed;
        }
        else
        {
            Debug.LogError("PlayerInputManager instance not found. Ensure PlayerInputManager is present in the scene.");
        }
    }

    private void Start()
    {
        typeWriterEffect = GetComponent<TypeWriterEffect>(); // Find the typing effect component
        responseHandler = GetComponent<ResponseHandler>(); // Find the response handler component
        ResetDialogueStats(); // Ensure dialogue stats are reset at the start of the game
    }

    // ---------------------- INTERACTION METHODS -------------------------

    // Method to handle traversing through dialogue (Input Action callback for Interact)
    private void OnInteractPerformed(InputAction.CallbackContext context) 
    {
        Debug.Log($"OnInteractPerformed called - IsOpen: {IsOpen}");
        // Only register inputs if dialogue is open
        if (IsOpen) 
        {
            interactPressed = true; // Set the flag to indicate the interact button was pressed
            Debug.Log("Interact flag set to true");
        }
    }

    // Method to handle cancelling the dialogue (Input Action callback for Cancel)
    private void OnCancelPerformed(InputAction.CallbackContext context) 
    {
        Debug.Log($"OnCancelPerformed called - IsOpen: {IsOpen}");
        // Only register inputs if dialogue is open
        if (IsOpen)
        {
            cancelPressed = true; // Set the flag to indicate the interact button was pressed
            Debug.Log("Cancel flag set to true");
        }
    }

    // Method to reset input flags after they have been processed in the dialogue coroutine
    private void ResetInputFlags()
    {
        // Reset all flags to false after processing to ensure they only trigger once per trigger
        interactPressed = false;
        cancelPressed = false;
    }

    // Method to reset all dialogue stats 
    private void ResetDialogueStats() 
    {
        // Resets all dialogue stats to default values (used when starting a new dialogue)
        IsOpen = false;
        dialogueBox.SetActive(false);
        textLabel.text = string.Empty;
        dialogueFinished = true;
        ResetInputFlags();
    }

    // ------------------------- DEV METHODS -------------------------

    // Method to start the Dialogue
    public void ShowDialogue(DialogueObject dialogueObject)
    {
        // If dialogue is already open, stop the previous coroutine first
        if (currentDialogueCoroutine != null)
        {
            StopCoroutine(currentDialogueCoroutine);
        }

        IsOpen = true; // Mark dialogue as open
        dialogueBox.SetActive(true); // Show the dialogue box
        ResetInputFlags(); // Reset input flags at the start of new dialogue
        GameplayInputManager.Instance.EnableMap("UserNavigation"); // Switch to the UserNavigation action map for dialogue interaction
        currentDialogueCoroutine = StartCoroutine(StepThroughDialogue(dialogueObject)); // Start showing the dialogue
        dialogueFinished = false; // Reset finished flag for new dialogue
    }

    // Method to add response events from the DialogueObject to the ResponseHandler
    public void AddResponseEvents(ResponseEvent[] responseEvents)
    {
        // Pass the response events to the response handler
        responseHandler.AddResponseEvents(responseEvents);
    }

    // Coroutine Method to traverse through each line of dialogue
    private IEnumerator StepThroughDialogue(DialogueObject dialogueObject)
    {
        // Loop through each line of dialogue
        for (int i = 0; i < dialogueObject.Dialogue.Length; i++)
        {
            string dialogue = dialogueObject.Dialogue[i]; // Get current line

            // Run typing effect for this line
            yield return RunTypingEffect(dialogue);

            // Display full line text once typing is done
            textLabel.text = dialogue;

            // If last line and there are responses, break to show them
            if (i == dialogueObject.Dialogue.Length - 1 && dialogueObject.HasResponses)
                break;

            // Wait for player input to proceed to next line
            yield return WaitForInput();

            // Immediately exits the loop dialogue if cancel button was pressed
            if (cancelPressed)
            {
                // Calls the method to close the dialogue box
                CloseDialogueBox();
                yield break;
            }

            // Calls the method to reset input flags 
            ResetInputFlags();
        }

        // After finishing all lines
        if (dialogueObject.HasResponses)
        {
            // Show responses if any exist
            responseHandler.ShowResponses(dialogueObject.Responses);
        }
        else
        {
            // Wait for player input to close dialogue if no responses
            yield return WaitForInput();

            // Checks if cancel button was pressed 
            if (cancelPressed)
            {
                // Immediately exits the dialogue if cancel button was pressed, otherwise just closes it
                CloseDialogueBox();
                yield break;
            }

            // Calls the method to close the dialogue box 
            CloseDialogueBox();
        }
    }

    // Coroutine Method to wait for player input before proceeding in the dialogue
    private IEnumerator WaitForInput() 
    {
        // Wait until either the interact or cancel button is pressed
        while (!interactPressed && !cancelPressed)
        {
            // Return null when a button hasn't been pressed yet
            // * waits for user input to skip the dialogue
            yield return null;
        }
    }

    // Coroutine Method to run the typewriter effect for a given line of dialogue
    private IEnumerator RunTypingEffect(string dialogue)
    {
        // Start the typewriter effect
        typeWriterEffect.Run(dialogue, textLabel);

        // Wait while it�s typing
        while (typeWriterEffect.isRunning)
        {
            // Return null each frame while the effect is running to allow it to animate properly
            yield return null;

            // Allow player to skip typing with Space
            if (interactPressed)
            {
                // Immediately finish the typewriter effect and display the full line of dialogue
                typeWriterEffect.Stop();
                ResetInputFlags(); // Reset after skipping
            }
        }
    }

    // Method to close the dialogue box and reset all relevant state
    public void CloseDialogueBox()
    {
        // Stop any ongoing dialogue coroutine
        if (currentDialogueCoroutine != null)
        {
            // Stops the current dialogue coroutine if it's still running 
            StopCoroutine(currentDialogueCoroutine);
            currentDialogueCoroutine = null;
        }

        ResetDialogueStats(); // Reset all dialogue stats to default values

        // Checks if PlayerInputManager instance exists 
        if (GameplayInputManager.Instance != null)
        {
            // Resets back to Player action map when dialogue intteraction closes
            GameplayInputManager.Instance.EnableMap("Player");
            Debug.Log("Dialogue closed - Switched back to Player map");
        }

        Debug.Log($"onDialogueBoxClosedGlobal was triggered through: {this.gameObject.name} " +
                  $"from {this.gameObject.transform.parent.name}");

        onDialogueBoxClosedGlobal?.TriggerEvent();
    }

    // Automated Unity Built-In method being called when this object is destroyed
    private void OnDisable()
    {
        // Check if PrivatePrinceControls is not null before trying to unsubscribe to prevent null reference errors
        if (ppControls != null)
        {
            // Unsubscribe from the Interact and Cancel actions' performed events to prevent memory leaks and unintended behavior when this object is destroyed
            ppControls.UI.Proceed.performed -= OnInteractPerformed;
            ppControls.UI.Cancel.performed -= OnCancelPerformed;
            Debug.Log("DialogueUI: Unsubscribed from input events");
        }
    }
}