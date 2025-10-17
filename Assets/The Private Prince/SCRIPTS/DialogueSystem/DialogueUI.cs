using System.Collections;
using UnityEngine;
using TMPro;

public class DialogueUI : MonoBehaviour
{
    [SerializeField] private GameObject dialogueBox; // The dialogue box UI element
    [SerializeField] private TMP_Text textLabel; // The text field for displaying dialogue

    public bool IsOpen { get; private set; } // Public flag to check if dialogue is open

    private ResponseHandler responseHandler; // Handles showing and reacting to responses
    private TypeWriterEffect typeWriterEffect; // Handles text typing animation
    private Coroutine currentDialogueCoroutine; // Keeps track of the active dialogue coroutine

    private void Start()
    {
        typeWriterEffect = GetComponent<TypeWriterEffect>(); // Find the typing effect component
        responseHandler = GetComponent<ResponseHandler>(); // Find the response handler component
        CloseDialogueBox(); // Make sure the dialogue box starts closed
    }

    public void ShowDialogue(DialogueObject dialogueObject)
    {
        // If dialogue is already open, stop the previous coroutine first
        if (currentDialogueCoroutine != null)
        {
            StopCoroutine(currentDialogueCoroutine);
        }

        IsOpen = true; // Mark dialogue as open
        dialogueBox.SetActive(true); // Show the dialogue box
        currentDialogueCoroutine = StartCoroutine(StepThroughDialogue(dialogueObject)); // Start showing the dialogue
    }

    public void AddResponseEvents(ResponseEvent[] responseEvents)
    {
        // Pass the response events to the response handler
        responseHandler.AddResponseEvents(responseEvents);
    }

    private IEnumerator StepThroughDialogue(DialogueObject dialogueObject)
    {
        // Loop through each line of dialogue
        for (int i = 0; i < dialogueObject.Dialogue.Length; i++)
        {
            string dialogue = dialogueObject.Dialogue[i]; // Get current line

            // Run typing effect for this line
            yield return RunTypingEffect(dialogue);

            // Display full line text once typing is done
            textLabel.text = dialogue;

            // If last line and there are responses, break to show them
            if (i == dialogueObject.Dialogue.Length - 1 && dialogueObject.HasResponses)
                break;

            // Wait until player presses Space or Escape before continuing
            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Escape));

            // If Escape pressed, close immediately
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CloseDialogueBox();
                yield break;
            }
        }

        // After finishing all lines
        if (dialogueObject.HasResponses)
        {
            // Show responses if any exist
            responseHandler.ShowResponses(dialogueObject.Responses);
        }
        else
        {
            // Otherwise, wait for one more input to close
            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Escape));
            CloseDialogueBox();
        }
    }

    private IEnumerator RunTypingEffect(string dialogue)
    {
        // Start the typewriter effect
        typeWriterEffect.Run(dialogue, textLabel);

        // Wait while it’s typing
        while (typeWriterEffect.isRunning)
        {
            yield return null;

            // Allow player to skip typing with Space
            if (Input.GetKeyDown(KeyCode.Space))
            {
                typeWriterEffect.Stop();
            }
        }
    }

    public void CloseDialogueBox()
    {
        // Stop any ongoing dialogue coroutine
        if (currentDialogueCoroutine != null)
        {
            StopCoroutine(currentDialogueCoroutine);
            currentDialogueCoroutine = null;
        }

        // Reset all dialogue UI state
        IsOpen = false;
        dialogueBox.SetActive(false);
        textLabel.text = string.Empty;
    }
}
