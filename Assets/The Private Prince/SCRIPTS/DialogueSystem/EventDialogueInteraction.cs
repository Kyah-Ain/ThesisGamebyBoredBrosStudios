using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventDialogueInteraction : MonoBehaviour, IInteractable
{
    // ------------------------- VARIABLES -------------------------

    [SerializeField] private DialogueObject dialogueObject;
    [SerializeField] private int dialogueIterationLimit = 1;
    [SerializeField] private QuestStarter questStarter; // Reference to start quest after dialogue
    //[SerializeField] private GameObject ringtoneSFX; // Optional SFX to play when triggered

    private int currentIteration = 0;
    private bool isDialogueActive = false;

    // ------------------------- UNITY METHODS -------------------------

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && other.TryGetComponent(out PlayerDialogueInteraction player))
        {
            // Set this as the interactable object for the player
            player.Interactable = this;

            // Automatically trigger dialogue when player enters trigger
            StartNarrating(player);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && other.TryGetComponent(out PlayerDialogueInteraction player))
        {
            // Clear the interactable reference when player leaves
            if (player.Interactable is EventDialogueInteraction eventDialogue && eventDialogue == this)
            {
                player.Interactable = null;
            }
        }
    }

    // ------------------------- DIALOGUE METHODS -------------------------

    public void StartNarrating(PlayerDialogueInteraction player)
    {
        // Don't start if dialogue is already active
        if (isDialogueActive) return;

        // Check if we've reached the iteration limit
        if (currentIteration >= dialogueIterationLimit)
        {
            HandleDialogueComplete(player);
            return;
        }

        //// Play ringtone SFX if assigned
        //if (ringtoneSFX != null)
        //{
        //    ringtoneSFX.SetActive(true);
        //}

        // Set dialogue active flag
        isDialogueActive = true;

        // Disable player movement
        if (player.characterController != null)
        {
            player.characterController.inDialogue = true;
        }

        // Switch to navigation action map
        if (GameplayInputManager.Instance != null)
        {
            GameplayInputManager.Instance.SwitchActionMap("UserNavigation");
        }

        // Show the dialogue
        if (player.DialogueUI != null)
        {
            // Handle response events if any
            DialogueResponseEvent[] responseEvents = GetComponents<DialogueResponseEvent>();
            if (responseEvents != null)
            {
                foreach (DialogueResponseEvent responseEvent in responseEvents)
                {
                    if (responseEvent != null && responseEvent.DialogueObject == dialogueObject)
                    {
                        player.DialogueUI.AddResponseEvents(responseEvent.Events);
                        break;
                    }
                }
            }

            // Show the dialogue
            player.DialogueUI.ShowDialogue(dialogueObject);

            // Increment iteration counter
            currentIteration++;
        }
    }

    private void HandleDialogueComplete(PlayerDialogueInteraction player)
    {
        // Re-enable player movement
        if (player.characterController != null)
        {
            player.characterController.inDialogue = false;
        }

        // Start quest if assigned
        if (questStarter != null)
        {
            questStarter.StartQuestById("Follow_The_Sound");
        }

        // Optionally destroy this trigger object
        Destroy(this.gameObject);
    }

    // ------------------------- IINTERACTABLE IMPLEMENTATION -------------------------

    public void Interact(DialogueInteraction player)
    {
        // Cast to PlayerDialogueInteraction since that's what we're working with
        if (player is PlayerDialogueInteraction playerDialogue)
        {
            StartNarrating(playerDialogue);
        }
    }

    // ------------------------- PUBLIC METHODS -------------------------

    public void UpdateDialogueObject(DialogueObject newDialogueObject)
    {
        dialogueObject = newDialogueObject;
    }
}