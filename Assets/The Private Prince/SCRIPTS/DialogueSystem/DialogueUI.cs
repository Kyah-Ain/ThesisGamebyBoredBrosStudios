using System.Collections;
using UnityEngine;
using TMPro; //ADD WHEN USING TextMeshPro

public class DialogueUI : MonoBehaviour
{
    [SerializeField] private GameObject dialogueBox; // The main dialogue box UI element
    [SerializeField] private TMP_Text textLabel; // The text label where dialogue appears

    public bool IsOpen { get; private set; } // Public property to check if dialogue is currently open    

    private ResponseHandler responseHandler; // Reference to handle response buttons
    private TypeWriterEffect typeWriterEffect; // Reference to handle typewriter effect

    private void Start()
    {
        typeWriterEffect = GetComponent<TypeWriterEffect>(); // Get TypeWriterEffect component
        responseHandler = GetComponent<ResponseHandler>(); // Get ResponseHandler component   

        CloseDialogueBox(); // Ensure dialogue box is closed at start
    }

    public void ShowDialogue(DialogueObject dialogueObject)
    {
        IsOpen = true; // Set dialogue state to open
        dialogueBox.SetActive(true); // Activate the dialogue box UI
        StartCoroutine(StepThroughDialogue(dialogueObject)); // Start dialogue progression coroutine
    }

    public void AddResponseEvents(ResponseEvent[] responseEvents)
    {
        responseHandler.AddResponseEvents(responseEvents);
    }

    private IEnumerator StepThroughDialogue(DialogueObject dialogueObject)
    {
        // Loop through each line of dialogue in the dialogue object
        for (int i = 0; i < dialogueObject.Dialogue.Length; i++)
        {
            string dialogue = dialogueObject.Dialogue[i]; // Get current dialogue line

            yield return RunTypingEffect(dialogue);

            textLabel.text = dialogue;

            // If this is the last line and there are responses, break out of the loop
            if (i == dialogueObject.Dialogue.Length - 1 && dialogueObject.HasResponses) break;

            yield return null;

            // Wait for player to press Space before continuing to next line
            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
        }

        // Check if this dialogue has responses
        if (dialogueObject.HasResponses)
        {
            responseHandler.ShowResponses(dialogueObject.Responses); // Show response options
        }
        else
        {
            CloseDialogueBox(); // Close dialogue if no responses
        }
    }

    private IEnumerator RunTypingEffect(string dialogue)
    {
        typeWriterEffect.Run(dialogue, textLabel);

        while (typeWriterEffect.isRunning)
        {
            yield return null;

            if(Input.GetKeyDown(KeyCode.Space))
            {
                typeWriterEffect.Stop();
            }
        }
    }

    public void CloseDialogueBox()
    {
        IsOpen = false; // Set dialogue state to closed
        dialogueBox.SetActive(false); // Deactivate the dialogue box UI
        textLabel.text = string.Empty; // Clear the text label
    }
}