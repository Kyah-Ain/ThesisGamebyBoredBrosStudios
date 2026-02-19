using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventDialogueInteraction : PlayerDialogueInteraction
{
    // ------------------------- UNITY METHODS -------------------------

    // ...
    public override void Awake()
    {
        // Find the character controller component on this GameObject
        characterController = FindAnyObjectByType<CharacterController2Point5D>();
    }

    // ...
    public override void Update()
    {
        if (dialogueUI != null && dialogueUI.IsOpen) return;

        if (dialogueUI.dialogueFinished)
        {
            characterController.inDialogue = false; // Re-enable movement when dialogue finishes
        }

        if (dialogueIterationLimit > 0 && !dialogueUI.IsOpen)
        {
            characterController.inDialogue = true; // Disable movement when dialogue starts

            Interactable?.Interact(this); // Used null propagation for less lines
            dialogueIterationLimit--;
        }
        else
        {
            Destroy(this.gameObject); // Destroy this component when dialogue iteration limit is reached
        }
    }
}