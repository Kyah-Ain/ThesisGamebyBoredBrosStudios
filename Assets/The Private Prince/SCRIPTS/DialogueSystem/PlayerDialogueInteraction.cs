using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDialogueInteraction : DialogueInteraction
{
    // ------------------------- VARIABLES -------------------------

    [Header("DIALOGUE")]
    [SerializeField] protected CharacterController2Point5D characterController;

    // ------------------------- UNITY METHODS -------------------------

    // ...
    public override void Awake()
    {
        // Find the character controller component on this GameObject
        characterController = this.GetComponent<CharacterController2Point5D>(); 
    }

    // ...
    public override void Update()
    {
        if (base.dialogueUI != null && base.dialogueUI.IsOpen) return;

        if (base.dialogueUI.dialogueFinished) 
        { 
            characterController.inDialogue = false; // Re-enable movement when dialogue finishes
        }

        // Button prompt for Dialogue Interaction
        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("Interact key pressed, attempting to interact with: " + Interactable);

            characterController.inDialogue = true; // Disable movement when dialogue starts

            Interactable?.Interact(this); // Used null propagation for less lines
        }
    }
}