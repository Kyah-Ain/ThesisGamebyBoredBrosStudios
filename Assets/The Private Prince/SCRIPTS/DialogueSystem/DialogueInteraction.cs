using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueInteraction : MonoBehaviour
{
    // ------------------------- VARIABLES -------------------------

    [Header("DIALOGUE")]
    [SerializeField] private DialogueUI dialogueUI;
    [SerializeField] private CharacterController2Point5D characterController;

    public DialogueUI DialogueUI => dialogueUI;
    public IInteractable Interactable { get; set; }

    // ------------------------- UNITY METHODS -------------------------

    // ...
    public void Awake()
    {
        // Find the character controller component on this GameObject
        characterController = this.GetComponent<CharacterController2Point5D>(); 
    }

    // ...
    public void Update()
    {
        if (dialogueUI != null && dialogueUI.IsOpen) return;

        if (dialogueUI.dialogueFinished) 
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