using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventDialogueInteraction : PlayerDialogueInteraction
{
    // ------------------------- VARIABLES -------------------------

    public QuestStarter questStarter; // Reference to the QuestStarter.cs component for starting quests
    //public GameObject ringtoneSFX; // Reference to the ringtone SFX GameObject to play when dialogue starts

    // ------------------------- UNITY METHODS -------------------------

    // ...
    public override void Awake()
    {
        // Find the character controller component on this GameObject
        characterController = FindAnyObjectByType<CharacterController2Point5D>();

        //// Find the ringtone SFX GameObject in the scene
        //if (ringtoneSFX == null)
        //    ringtoneSFX = GameObject.FindWithTag("RingtoneSFX");

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

            // WARNING: Make sure the object this script is attached to, is a player
            Interactable?.Interact(this); // Used null propagation for less lines
            dialogueIterationLimit--;
        }
        else
        {
            // When dialogue iteration limit is reached, start the quest and destroy this component
            questStarter.StartQuestById("Follow_The_Sound");

            //ringtoneSFX.SetActive(true); // Play the ringtone SFX when dialogue starts

            Destroy(this.gameObject); // Destroy this component when dialogue iteration limit is reached
        }
    }
}