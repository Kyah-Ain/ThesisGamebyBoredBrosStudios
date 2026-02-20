using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventDialogueInteraction : PlayerDialogueInteraction
{
    // ------------------------- VARIABLES -------------------------

    public QuestStarter questStarter; // Reference to the QuestStarter.cs component for starting quests

    // ------------------------- UNITY METHODS -------------------------

    // ...
    public override void Awake()
    {
        // Find the character controller component on this GameObject
        characterController = FindAnyObjectByType<CharacterController2Point5D>();

        // Find the QuestStarter component in the scene (assuming there's only one)
        questStarter = this.GetComponent<QuestStarter>();
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
            // When dialogue iteration limit is reached, start the quest and destroy this component
            questStarter.StartQuestById("Check_Jonas_Phone");
            Destroy(this.gameObject); // Destroy this component when dialogue iteration limit is reached
        }
    }
}