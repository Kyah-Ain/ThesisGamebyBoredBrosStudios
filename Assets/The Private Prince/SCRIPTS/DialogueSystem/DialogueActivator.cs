using System.Collections;
using System.Collections.Generic;
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
        if (other.CompareTag("Player") && other.TryGetComponent(out DialogueInteraction player))
        {
            player.Interactable = this;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && other.TryGetComponent(out DialogueInteraction player))
        {
            if (player.Interactable is DialogueActivator dialogueActivator && dialogueActivator == this)
            {
                player.Interactable = null;
            }
        }
    }

    public void Interact(DialogueInteraction player)
    {
        if (this == null || gameObject == null)
        {
            Debug.LogWarning("DialogueActivator has been destroyed");
            return;
        }

        DialogueResponseEvent[] responseEvents = GetComponents<DialogueResponseEvent>();
        
        if (responseEvents == null) return;

        foreach (DialogueResponseEvent responseEvent in responseEvents)
        {
            if (responseEvent != null && responseEvent.DialogueObject == dialogueObject)
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