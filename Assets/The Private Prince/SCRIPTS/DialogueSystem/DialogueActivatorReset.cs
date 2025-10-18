using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueActivatorReset : MonoBehaviour
{
    [SerializeField] private DialogueObject defaultDialogueObject;
    private DialogueActivator dialogueActivator;

    private void Start()
    {
        dialogueActivator = GetComponent<DialogueActivator>();
    }

    public void ResetToDefaultDialogue()
    {
        if (dialogueActivator != null && defaultDialogueObject != null)
        {
            dialogueActivator.UpdateDialogueObject(defaultDialogueObject);
        }
    }
}
