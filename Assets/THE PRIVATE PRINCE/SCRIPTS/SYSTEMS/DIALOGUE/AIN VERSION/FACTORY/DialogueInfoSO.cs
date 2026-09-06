using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DialogueInfoSO", menuName = "Dialogue/DialogueInfoSO", order = 1)]
public class DialogueInfoSO : ScriptableObject
{
    // ------------------------- VARIABLES -------------------------

    [field: SerializeField] public string id { get; private set; }
    
    [Header("Info")]
    public string dialogueName;

    [Header("Dialogue")]
    // [TextArea] public string[] IdleLines;
    // [TextArea] public string[] HasRequestLines;
    // [TextArea] public string[] WaitingForCompletionLines;
    // [TextArea] public string[] CanFinishRequestLines;
    
    public Dialogue DialogueLines;
    // public Responses[] Responses;
    // ------------------------- METHODS -------------------------
    
    // Built-In Uity Method to ensure the 'id' field is always set to the name of the ScriptableObject asset
    private void OnValidate()
    {
        // This method is called in the Unity Editor when the script is loaded or a value is changed in the inspector
        // - it ensures that the 'id' field is always set to the name of the ScriptableObject asset
        // - which can be useful for identifying quests by their asset name

        // If we're in the Unity Editor, set the 'id' field to the name of the asset
        #if UNITY_EDITOR
            id = this.name; // Set the 'id' field to the name of the ScriptableObject asset
            UnityEditor.EditorUtility.SetDirty(this); // Mark the ScriptableObject as dirty to ensure the change is saved
        #endif // End of Unity Editor check
    }
}

#region EXTENSION CLASSES

// Makes this class serializable so it can be displayed in the Unity Inspector
[System.Serializable]
public class Dialogue
{
    // Private Data
    [SerializeField][TextArea] private string[] idleLines;
    [SerializeField][TextArea] private string[] hasRequestLines;
    [SerializeField][TextArea] private string[] waitingForCompletionLines;
    [SerializeField][TextArea] private string[] canFinishRequestLines;
    
    // Readable Only Copies
    public string[] IdleLines => idleLines;
    public string[] HasRequestLines => hasRequestLines;
    public string[] WaitingForCompletionLines => waitingForCompletionLines;
    public string[] CanFinishRequestLines => canFinishRequestLines;
}

// Makes this class serializable so it can be displayed in the Unity Inspector
[System.Serializable]
public class Responses
{
    [SerializeField][TextArea] private string responseText; // The text that will appear on the response button
    // [SerializeField] private 
}

#endregion