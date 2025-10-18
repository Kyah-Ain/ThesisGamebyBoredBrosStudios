using UnityEngine;

public class DialogueActivator : MonoBehaviour, IInteractable
{
    [SerializeField] private DialogueObject dialogueObject;

    public void UpdateDialogueObject(DialogueObject dialogueObject)
    {
        this.dialogueObject = dialogueObject;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && other.TryGetComponent(out Player2Point5D player))
        {
            player.Interactable = this;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && other.TryGetComponent(out Player2Point5D player))
        {
            if (player.Interactable is DialogueActivator dialogueActivator && dialogueActivator == this)
            {
                player.Interactable = null;
            }
        }
    }

    public void Interact(Player2Point5D player)
    {
        foreach (DialogueResponseEvent responseEvent in GetComponents<DialogueResponseEvent>())
        {
            if (responseEvent.DialogueObject == dialogueObject)
            {
                player.DialogueUI.AddResponseEvents(responseEvent.Events);
                break;
            }
        }

        player.DialogueUI.ShowDialogue(dialogueObject);
    }

    public void ResetToDefaultDialogue()
    {
        NPCDialogueController dialogueController = GetComponent<NPCDialogueController>();
        if (dialogueController != null && dialogueController.DefaultDialogue != null)
        {
            UpdateDialogueObject(dialogueController.DefaultDialogue);
            Debug.Log($"Dialogue reset to: {dialogueController.DefaultDialogue.name}");
        }
        else
        {
            Debug.LogWarning("Could not reset dialogue - no NPCDialogueController or default dialogue found");
        }
    }
}