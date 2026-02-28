using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerDialogueInteraction : DialogueInteraction
{
    // ------------------------- VARIABLES -------------------------

    // Reference to the PrivatePrinceControls script that handles the new input system controls
    private PrivatePrinceControls ppControls;

    [Header("DIALOGUE")]
    public CharacterController2Point5D characterController;

    // Add a flag to track if dialogue is currently active
    private bool isDialogueActive = false;

    // ------------------------- UNITY METHODS -------------------------

    // Built-in Unity method called when this script was first loaded
    public override void Awake()
    {
        // Find the character controller component on this GameObject
        characterController = this.GetComponent<CharacterController2Point5D>();

        if (PlayerInputManager.Instance != null)
        {
            // * Initialize the input system controls
            // * Subscribes to the Interact action's performed event
            ppControls = PlayerInputManager.Instance.Controls;
            ppControls.Player.Interact.performed += ExecuteInteract;
        }
        else
        {
            Debug.LogError("PlayerInputManager instance not found. Ensure PlayerInputManager is present in the scene.");
        }
    }

    // Built-in Unity method called once per frame
    public override void Update()
    {
        // Handle dialogue completion
        if (dialogueUI != null && dialogueUI.dialogueFinished && isDialogueActive)
        {
            characterController.inDialogue = false;
            isDialogueActive = false;
        }
    }

    // Method to handle interaction input (Input Action callback for Interact)
    private void ExecuteInteract(InputAction.CallbackContext context)
    {
        // Only allow interaction if:
        // * Dialogue UI exists
        // * Dialogue is NOT currently active (prevents interrupting current dialogue)
        // * There's an interactable object
        if (dialogueUI != null && !isDialogueActive && Interactable != null)
        {
            // Switch to the UserNavigation action map when dialogue starts
            PlayerInputManager.Instance.SwitchActionMap("UserNavigation");

            // Disable player movement when dialogue starts by setting the inDialogue flag on the character controller
            characterController.inDialogue = true;

            // Set the dialogue active flag to true to prevent starting another dialogue until this one finishes
            isDialogueActive = true;

            // Invoke the Interact method on the interactable object for the Dialogue System
            // * passing this PlayerDialogueInteraction as a parameter
            Interactable.Interact(this);
        }
    }

    // Automated Unity Built-In method being called when this object is destroyed
    private void OnDestroy()
    {
        // Checks if the ppControls reference is not null before trying to unsubscribe
        if (ppControls != null)
        {
            // Clean up the input event subscription by unsubscribing from the 'ExecuteInteract' method 
            ppControls.Player.Interact.performed -= ExecuteInteract;
        }
    }
}