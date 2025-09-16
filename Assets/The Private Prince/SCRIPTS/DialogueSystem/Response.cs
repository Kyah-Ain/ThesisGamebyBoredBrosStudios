using UnityEngine;

// Makes this class serializable so it can be displayed in the Unity Inspector
[System.Serializable]
public class Response
{
    [SerializeField] private string responseText; // The text that will appear on the response button
    [SerializeField] private DialogueObject dialogueObject; // The dialogue object that this response leads to

    public string ResponseText => responseText; // Public read-only property to access the response text

    public DialogueObject DialogueObject => dialogueObject; // Public read-only property to access the next dialogue object
}