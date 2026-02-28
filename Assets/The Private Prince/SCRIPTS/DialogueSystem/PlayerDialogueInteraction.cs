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

        if (GameplayInputManager.Instance != null)
        {
            // * Initialize the input system controls
            // * Subscribes to the Interact action's performed event
            ppControls = GameplayInputManager.Instance.Controls;
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
        if (Interactable == null || Interactable.Equals(null))
        {
            Debug.Log("Interactable object no longer exists");
            Interactable = null; // Clears the reference
            return;
        }

        // Only allow interaction if:
        // * Dialogue UI exists
        // * Dialogue is NOT currently active (prevents interrupting current dialogue)
        // * There's an interactable object
        if (dialogueUI != null && !isDialogueActive && Interactable != null)
        {
            // Switch to the UserNavigation action map when dialogue starts
            GameplayInputManager.Instance.SwitchActionMap("UserNavigation");

            // Disable player movement when dialogue starts by setting the inDialogue flag on the character controller
            characterController.inDialogue = true;

            // Set the dialogue active flag to true to prevent starting another dialogue until this one finishes
            isDialogueActive = true;

            try
            {
                // Invoke the Interact method on the interactable object for the Dialogue System
                // * passing this PlayerDialogueInteraction as a parameter
                Interactable.Interact(this);
            }
            catch (MissingReferenceException e)
            {
                Debug.LogError($"Interactable was destroyed before interaction could complete: {e}");
                
                Interactable = null;
                isDialogueActive = false;
                characterController.inDialogue = false;
            }
        }
    }

    // Automated Unity Built-In method being called when this object is destroyed
    private void OnDestroy()
    {
        // Clear the Interactable reference
        Interactable = null;

        // Unsubscribe from input events
        if (ppControls != null)
        {
            ppControls.Player.Interact.performed -= ExecuteInteract;
        }
    }

    // Automated Unity Built-In method being called when this component is disabled
    private void OnDisable()
    {
        // Reset dialogue state if component is disabled
        if (isDialogueActive)
        {
            isDialogueActive = false;
            characterController.inDialogue = false;
        }
    }
}