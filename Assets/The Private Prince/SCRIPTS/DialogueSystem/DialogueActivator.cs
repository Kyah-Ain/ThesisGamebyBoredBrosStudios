using UnityEngine;

public class DialogueActivator : MonoBehaviour, IInteractable
{
    [SerializeField] private DialogueObject dialogueObject; // The dialogue object to be displayed when interacted with

    public void UpdateDialogueObject(DialogueObject dialogueObject)
    {
        this.dialogueObject = dialogueObject; 
    }

    // Called when another collider enters this object's trigger zone
    private void OnTriggerEnter(Collider other)
    {
        // Check if the entering object is the player and get Player2Point5D component
        if (other.CompareTag("Player") && other.TryGetComponent(out Player2Point5D player))
        {
            player.Interactable = this; // Set this object as the player's current interactable
        }
    }

    // Called when another collider exits this object's trigger zone
    private void OnTriggerExit(Collider other)
    {
        // Check if the exiting object is the player and get Player2Point5D component
        if (other.CompareTag("Player") && other.TryGetComponent(out Player2Point5D player))
        {
            // Check if the player's current interactable is this specific DialogueActivator
            if (player.Interactable is DialogueActivator dialogueActivator && dialogueActivator == this)
            {
                player.Interactable = null; // Clear the player's interactable reference
            }
        }
    }

    // Implementation of the IInteractable interface method
    public void Interact(Player2Point5D player)
    {
        if(TryGetComponent(out DialogueResponseEvent responseEvents) && responseEvents.DialogueObject == dialogueObject)
        {
            player.DialogueUI.AddResponseEvents(responseEvents.Events); 
        }

        // Tell the player's dialogue UI to show this object's dialogue
        player.DialogueUI.ShowDialogue(dialogueObject);
    }
}