using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ResponseHandler : MonoBehaviour
{
    [SerializeField] private RectTransform responseBox; // The container for all response buttons
    [SerializeField] private RectTransform responseButtonTemplate; // The template button for responses
    [SerializeField] private RectTransform responseContainer; // The parent object for response buttons

    private DialogueUI dialogueUI; // Reference to the DialogueUI component
    private ResponseEvent[] responseEvents;

    private List<GameObject> tempResponseButton = new List<GameObject>(); // Temporary list to track created response buttons

    private void Start()
    {
        dialogueUI = GetComponent<DialogueUI>(); // Get the DialogueUI component on the same GameObject
    }

    public void AddResponseEvents(ResponseEvent[] responseEvents)
    {
        this.responseEvents = responseEvents;
    }

    public void ShowResponses(Response[] responses)
    {
        float responseBoxHeight = 0; // Variable to calculate total height needed for response box

        for(int i = 0; i < responses.Length; i++)
        {
            Response response = responses[i];
            int responseIndex = i;

            GameObject responseButton = Instantiate(responseButtonTemplate.gameObject, responseContainer); // Create button from template
            responseButton.gameObject.SetActive(true); // Enable the button (template is usually hidden)
            responseButton.GetComponent<TMP_Text>().text = response.ResponseText; // Set button text to response text
            responseButton.GetComponent<Button>().onClick.AddListener(() => OnPickedResponse(response, responseIndex)); // Add click event listener

            tempResponseButton.Add(responseButton); // Add button to temporary list for cleanup

            responseBoxHeight += responseButtonTemplate.sizeDelta.y; // Add button height to total height calculation
        }

        responseBox.sizeDelta = new Vector2(responseBox.sizeDelta.x, responseBoxHeight); // Resize response box to fit all buttons
        responseBox.gameObject.SetActive(true); // Show the response box
    }

    private void OnPickedResponse(Response response, int responseIndex)
    {
        responseBox.gameObject.SetActive(false); // Hide the response box

        foreach (GameObject button in tempResponseButton)
        {
            Destroy(button); // Destroy all temporary response buttons
        }
        tempResponseButton.Clear(); // Clear the list of temporary buttons

        if(responseEvents != null && responseIndex <= responseEvents.Length)
        {
            responseEvents[responseIndex].OnPickedResponse?.Invoke();
        }

        responseEvents = null;

        if(response.DialogueObject)
        {
            dialogueUI.ShowDialogue(response.DialogueObject); 
        }
        else
        {
            dialogueUI.CloseDialogueBox();
        }

        dialogueUI.ShowDialogue(response.DialogueObject); // Show the next dialogue based on the chosen response
    }
}