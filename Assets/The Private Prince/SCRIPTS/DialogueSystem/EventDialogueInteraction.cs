using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Events;

public class EventDialogueInteraction : MonoBehaviour, ITalkable
{
    // ------------------------- VARIABLES -------------------------

    [Header("IMPORTANT: DIALOGUE CANVAS")]
    [SerializeField] protected DialogueUI dialogueUI;

    [SerializeField] private DialogueObject dialogueObject;
    [SerializeField] private int dialogueIterationLimit = 1;
    //[SerializeField] private QuestStarter questStarter; // Reference to start quest after dialogue
    //[SerializeField] private GameObject ringtoneSFX; // Optional SFX to play when triggered

    private int currentIteration = 0;
    private bool isDialogueActive = false;

    [Header("OPTIONAL EVENT")]

    [Space]

    public UnityEvent onAfterDialogue;

    // ------------------------- UNITY METHODS -------------------------

    private void Update()
    {
        // Check if dialogue is no longer active and we were in a dialogue
        if (isDialogueActive && dialogueUI != null && !dialogueUI.IsOpen)
        {
            // Dialogue has been closed
            isDialogueActive = false;

            // Try to start the next dialogue iteration
            if (currentPlayer != null)
            {
                StartNarrating(currentPlayer);
            }
        }
    }

    private void OnTriggerEnter(Collider actor)
    {
        if (actor.CompareTag("Player") && actor.TryGetComponent(out PlayerDialogueInteraction player))
        {
            // Set this as the interactable object for the player
            player.Interactable = this;

            // Automatically trigger dialogue when player enters trigger
            StartNarrating(player);
        }
    }

    private void OnTriggerExit(Collider actor)
    {
        if (actor.CompareTag("Player") && actor.TryGetComponent(out PlayerDialogueInteraction player))
        {
            // Clear the interactable reference when player leaves
            if (player.Interactable is EventDialogueInteraction eventDialogue && eventDialogue == this)
            {
                player.Interactable = null;
            }
        }
    }

    // ------------------------- DIALOGUE METHODS -------------------------

    private PlayerDialogueInteraction currentPlayer; // Store player reference

    public void StartNarrating(PlayerDialogueInteraction player)
    {
        // Store player reference
        currentPlayer = player;

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
            GameplayInputManager.Instance.EnableMap("UserNavigation");
        }

        // Show the dialogue on THIS object's canvas, not the player's
        if (dialogueUI != null)
        {
            //// Handle response events if any
            //DialogueResponseEvent[] responseEvents = GetComponents<DialogueResponseEvent>();
            //if (responseEvents != null)
            //{
            //    foreach (DialogueResponseEvent responseEvent in responseEvents)
            //    {
            //        if (responseEvent != null && responseEvent.DialogueObject == dialogueObject)
            //        {
            //            dialogueUI.AddResponseEvents(responseEvent.Events);
            //            break;
            //        }
            //    }
            //}

            // Show the dialogue on THIS canvas
            dialogueUI.ShowDialogue(dialogueObject);

            // Increment iteration counter
            currentIteration++;
        }
    }

    private void HandleDialogueComplete(PlayerDialogueInteraction player)
    {
        Debug.Log($"EventDialogueInteraction: The HandleDialogue method was called!");

        // Re-enable player movement
        if (player.characterController != null)
        {
            player.characterController.inDialogue = false;
        }

        onAfterDialogue?.Invoke();

        //// Optionally destroy this trigger object
        //Destroy(this.gameObject);
    }

    // ------------------------- IINTERACTABLE IMPLEMENTATION -------------------------

    public void Interact(DialogueInteraction player)
    {
        // ...
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