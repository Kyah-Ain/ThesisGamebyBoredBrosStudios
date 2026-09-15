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
    public Dialogue dialogue;
    
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
    // Private Data (can only be seen & set in the Inspector of this Instance)
    [SerializeField] private DialogueLines idleLines;
    [SerializeField] private DialogueLines hasRequestLines;
    [SerializeField] private DialogueLines acceptRequestLines;
    [SerializeField] private DialogueLines declineRequestLines;
    [SerializeField] private DialogueLines waitingForCompletionLines;
    [SerializeField] private DialogueLines canFinishRequestLines;
    
    // Readable Copies of "Private Data" (accessible Xerox for other scripts)
    public string[] IdleLines => idleLines.speechLines;
    public string[] HasRequestLines => hasRequestLines.speechLines;
    public string[] AcceptRequestLines => acceptRequestLines.speechLines;
    public string[] DeclineRequestLines => declineRequestLines.speechLines;
    public string[] WaitingForCompletionLines => waitingForCompletionLines.speechLines;
    public string[] CanFinishRequestLines => canFinishRequestLines.speechLines;
}

#endregion