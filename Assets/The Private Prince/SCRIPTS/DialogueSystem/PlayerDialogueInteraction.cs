using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDialogueInteraction : DialogueInteraction
{
    // ------------------------- VARIABLES -------------------------

    [Header("DIALOGUE")]
    public CharacterController2Point5D characterController;

    // ------------------------- UNITY METHODS -------------------------

    // ...
    public override void Awake()
    {
        // Find the character controller component on this GameObject
        characterController = this.GetComponent<CharacterController2Point5D>(); 
    }

    public override void Update()
    {
        // Handle dialogue closing
        if (dialogueUI != null && dialogueUI.dialogueFinished)
        {
            characterController.inDialogue = false;

            // Handle input instead
            if (Input.GetKeyDown(KeyCode.E) && Interactable != null)
            {
                characterController.inDialogue = true;
                Interactable.Interact(this); // Player initiates interaction
            }
        }
    }
}