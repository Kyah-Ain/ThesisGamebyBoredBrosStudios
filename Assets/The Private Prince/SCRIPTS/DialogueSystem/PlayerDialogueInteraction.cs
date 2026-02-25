using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerDialogueInteraction : DialogueInteraction
{
    // ------------------------- VARIABLES -------------------------

    // Reference to the PlayerInput component for handling new input system actions and controls
    private PrivatePrinceControls ppControls;

    [Header("DIALOGUE")]
    public CharacterController2Point5D characterController;

    // Add a flag to track if dialogue is currently active
    private bool isDialogueActive = false;

    // ------------------------- UNITY METHODS -------------------------

    // ...
    public override void Awake()
    {
        // Find the character controller component on this GameObject
        characterController = this.GetComponent<CharacterController2Point5D>();

        if (PlayerInputManager.Instance != null)
        {
            // * Initialize the input system controls
            // * Subscribes to the Interact action's performed event
            ppControls = PlayerInputManager.Instance.Controls;
            ppControls.Player.Interact.performed += Interact;
        }
        else
        {
            Debug.LogError("PlayerInputManager instance not found. Ensure PlayerInputManager is present in the scene.");
        }
    }

    // ...
    public override void Update()
    {
        // Handle dialogue completion
        if (dialogueUI != null && dialogueUI.dialogueFinished && isDialogueActive)
        {
            characterController.inDialogue = false;
            isDialogueActive = false;
        }
    }

    // ...
    private void Interact(InputAction.CallbackContext context)
    {
        // Only allow interaction if:
        // n\ Dialogue UI exists
        // n\ Dialogue is NOT currently active (prevents interrupting current dialogue)
        // n\ There's an interactable object
        if (dialogueUI != null && !isDialogueActive && Interactable != null)
        {
            characterController.inDialogue = true;
            isDialogueActive = true;
            Interactable.Interact(this);
        }
    }

    // ...
    private void OnDestroy()
    {
        // Clean up the input event subscription
        if (ppControls != null)
        {
            ppControls.Player.Interact.performed -= Interact;
        }
    }
}