using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ResponseHandler : MonoBehaviour
{
    [SerializeField] private RectTransform responseBox;
    [SerializeField] private RectTransform responseButtonTemplate;
    [SerializeField] private RectTransform responseContainer;

    private DialogueUI dialogueUI;
    private ResponseEvent[] responseEvents;
    private List<GameObject> tempResponseButton = new List<GameObject>();

    private void Start()
    {
        dialogueUI = GetComponent<DialogueUI>();
    }

    public void AddResponseEvents(ResponseEvent[] responseEvents)
    {
        this.responseEvents = responseEvents;
    }

    public void ShowResponses(Response[] responses)
    {
        float responseBoxHeight = 0;

        for (int i = 0; i < responses.Length; i++)
        {
            Response response = responses[i];
            int responseIndex = i;

            GameObject responseButton = Instantiate(responseButtonTemplate.gameObject, responseContainer);
            responseButton.gameObject.SetActive(true);
            responseButton.GetComponent<TMP_Text>().text = response.ResponseText;
            responseButton.GetComponent<Button>().onClick.AddListener(() => OnPickedResponse(response, responseIndex));

            tempResponseButton.Add(responseButton);

            responseBoxHeight += responseButtonTemplate.sizeDelta.y;
        }

        responseBox.sizeDelta = new Vector2(responseBox.sizeDelta.x, responseBoxHeight);
        responseBox.gameObject.SetActive(true);
    }

    private void OnPickedResponse(Response response, int responseIndex)
    {
        responseBox.gameObject.SetActive(false);

        foreach (GameObject button in tempResponseButton)
        {
            Destroy(button);
        }
        tempResponseButton.Clear();

        // Store whether this response should reset dialogue
        bool shouldResetDialogue = false;

        if (responseEvents != null && responseIndex <= responseEvents.Length)
        {
            responseEvents[responseIndex].OnPickedResponse?.Invoke();

            // Check if this is a "decline" response that should reset
            if (response.ResponseText.ToLower().Contains("not now") ||
                response.ResponseText.ToLower().Contains("not right now") ||
                response.ResponseText.ToLower().Contains("maybe later"))
            {
                shouldResetDialogue = true;
                Debug.Log($"Detected decline response: {response.ResponseText}");
            }
        }

        responseEvents = null;

        if (response.DialogueObject)
        {
            dialogueUI.ShowDialogue(response.DialogueObject);
        }
        else
        {
            // Reset dialogue for decline responses
            if (shouldResetDialogue)
            {
                ResetDialogueToDefault();
            }
            dialogueUI.CloseDialogueBox();
        }
    }

    private void ResetDialogueToDefault()
    {
        DialogueActivator activator = FindObjectOfType<DialogueActivator>();
        if (activator != null)
        {
            NPCDialogueController dialogueController = activator.GetComponent<NPCDialogueController>();
            if (dialogueController != null)
            {
                dialogueController.ResetToDefault();
            }
        }
    }
}   