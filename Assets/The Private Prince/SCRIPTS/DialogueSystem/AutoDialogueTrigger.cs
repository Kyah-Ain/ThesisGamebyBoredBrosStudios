using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Auto-triggering dialogue that doesn't require trigger colliders
/// </summary>
public class AutoDialogueTrigger : DialogueInteraction, ITalkable
{
    [Header("AUTO DIALOGUE SETTINGS")]
    [SerializeField] private DialogueObject dialogueObject;
    [SerializeField] private float delayBeforeStart = 0f;
    [SerializeField] private bool startOnEnable = true;
    [SerializeField] private bool destroyAfterDialogue = false;
    [SerializeField] private UnityEvent onDialogueComplete;

    private bool hasTriggered = false;
    private PlayerDialogueInteraction currentPlayer;

    private void OnEnable()
    {
        if (startOnEnable && !hasTriggered)
        {
            if (delayBeforeStart > 0f)
            {
                StartCoroutine(DelayedStart());
            }
            else
            {
                FindAndStartDialogue();
            }
        }
    }

    private IEnumerator DelayedStart()
    {
        yield return new WaitForSeconds(delayBeforeStart);
        FindAndStartDialogue();
    }

    public void FindAndStartDialogue()
    {
        if (hasTriggered) return;

        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null && player.TryGetComponent(out PlayerDialogueInteraction playerInteraction))
        {
            currentPlayer = playerInteraction;
            hasTriggered = true;

            // Set this as the interactable object
            currentPlayer.Interactable = this;

            // Start dialogue
            StartCoroutine(StartDialogueSequence());
        }
        else
        {
            Debug.LogError($"AutoDialogueTrigger: Player not found on {gameObject.name}");
        }
    }

    private IEnumerator StartDialogueSequence()
    {
        // Wait a frame to ensure everything is set up
        yield return null;

        // Trigger the dialogue
        if (currentPlayer != null && dialogueUI != null)
        {
            // Disable player movement
            if (currentPlayer.characterController != null)
            {
                currentPlayer.characterController.inDialogue = true;
            }

            // Switch action map
            if (GameplayInputManager.Instance != null)
            {
                GameplayInputManager.Instance.EnableMap("UserNavigation");
            }

            // Show dialogue
            dialogueUI.ShowDialogue(dialogueObject);

            // Wait for dialogue to complete
            yield return new WaitUntil(() => !dialogueUI.IsOpen);

            // Handle completion
            HandleDialogueComplete();
        }
    }

    private void HandleDialogueComplete()
    {
        // Re-enable player movement
        if (currentPlayer != null && currentPlayer.characterController != null)
        {
            currentPlayer.characterController.inDialogue = false;
        }

        // Invoke completion event
        onDialogueComplete?.Invoke();

        // Clear interactable reference
        if (currentPlayer != null && Object.Equals(currentPlayer.Interactable, this))
        {
            currentPlayer.Interactable = null;
        }

        // Destroy if needed
        if (destroyAfterDialogue)
        {
            Destroy(gameObject);
        }
    }

    // IInteractable implementation
    public void Interact(DialogueInteraction player)
    {
        // Already handled automatically, but this allows manual triggering if needed
        if (!hasTriggered && player is PlayerDialogueInteraction playerDialogue)
        {
            currentPlayer = playerDialogue;
            hasTriggered = true;
            StartCoroutine(StartDialogueSequence());
        }
    }
}