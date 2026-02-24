using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InteractableDialogue : MonoBehaviour
{
    // ------------------------- VARIABLES -------------------------

    [Header("DIALOGUES")]
    [SerializeField] private DialogueObject defaultPromptDialogue;
    [SerializeField] private DialogueObject interactionPromptDialogue;

    public DialogueObject DefaultDialogueObject => defaultPromptDialogue;
    public DialogueObject InteractionDialogueObject => interactionPromptDialogue;

    [Header("UI REFERENCES")]
    public GameObject interactablePanel; // Reference to the Interactable Panel In-Game Canvas
    public TextMeshProUGUI interactPromptText; // Reference to the TextMeshProUGUI component for displaying the interaction prompt text

    [Header("LIMITERS")]
    public float interactionRange = 2f; // Sets the range at which the player can interact with this object

    // ------------------------- UNITY METHODS -------------------------

    // Built-In Unity method that called when this script's gameObject is first initialized
    public void Awake()
    {
        // This Method is being use to make sure that the Interact Prompt
        // n/ is always hidden the first they are spawned

        // Ensures the interactable panel reference is assigned in the inspector
        if (interactablePanel == null)
            Debug.LogWarning($"Interactable Panel reference is missing on " +
                             $"{gameObject.name} - please assign it in the inspector");

        // Sets the interaction prompt text to the default dialogue message
        interactPromptText.text = defaultPromptDialogue.Dialogue[0];

        // Hides the interactable panel when the player exits the trigger area
        interactablePanel.SetActive(false);
    }

    // NOTE:
    // Collider - refers to the data type of the parameter "actor" that enters 
    // actor - refers to the gameObject that entered this trigger collider 

    // Built-In Unity method that called when a gameObject with a Collider enters
    private void OnTriggerEnter(Collider actor)
    {
        // Filters the trigger event to only respond to a 'Player' tagged gameObject
        if (actor.CompareTag("Player"))
        {
            // Show the interactable panel when the player enters the trigger area
            interactablePanel.SetActive(true);
        }
    }

    // Built-In Unity method that called as long as a gameObject with a Collider stays within the trigger area
    private void OnTriggerStay(Collider actor)
    {
        // Filters the trigger event to only respond to a 'Player' tagged gameObject
        if (actor.CompareTag("Player"))
        {
            // Calculate the distance between this object and the player 
            float distanceToPlayer = Vector3.Distance(this.transform.position, actor.transform.position);

            // Checks if the player is within the interaction range to determine if interaction is possible
            if (distanceToPlayer <= interactionRange)
            {
                // Sets the interaction prompt text to the interaction dialogue message 
                interactPromptText.text = interactionPromptDialogue.Dialogue[0];

                // Visualize line between this object and the player in the Scene view for debugging purposes
                Debug.DrawLine(this.transform.position, actor.transform.position, Color.red);
            }
            else
            {
                // Sets the interaction prompt text to the default dialogue message
                interactPromptText.text = defaultPromptDialogue.Dialogue[0];

                // Same as before but with a different color to indicate player is in the interaction range
                Debug.DrawLine(this.transform.position, actor.transform.position, Color.green);
            }
        }
    }

    // Built-In Unity method that called when a gameObject with a Collider exits
    private void OnTriggerExit(Collider actor)
    {
        // Filters the trigger event to only respond to a 'Player' tagged gameObject
        if (actor.CompareTag("Player"))
        {
            // Hides the interactable panel when the player exits the trigger area
            interactablePanel.SetActive(false);
        }
    }
}