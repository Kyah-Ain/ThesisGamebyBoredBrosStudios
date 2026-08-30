using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;
using UnityEngine.Events;

// Required DebuggerNiAinPjls.cs for this to be able to monitor debugs, otherwise use the old one
[RequireComponent(typeof(DebuggerNiAinPjls))]
public class DialogueNarrator : MonoBehaviour
{
    // ------------------------- VARIABLES -------------------------
    [Header("EVENTS")]
    public UnityEvent onDialogueDone;
    
    [Header("REFERENCES")]
    [SerializeField] DebuggerNiAinPjls debuggerNiAin; // Custom debugging script from your dev Ain
    [SerializeField] private DialogueInfoSO dialogueInfo;
    
    [Header("UI")]
    [SerializeField] TextMeshProUGUI dialogueField;
    
    // ----------------------- UNITY METHODS -------------------------
    #region UNITY METHODS
    
    // Awake is called when this script was first initialized & loaded
    private void Awake()
    {
        // Checks if our reference for the script was not set
        if (debuggerNiAin == null)
        {
            // If it is not, then set it automatically by looking for the script class from this object
            debuggerNiAin = this.GetComponent<DebuggerNiAinPjls>();
        }
    }
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    #endregion
    
    // -------------------- DIALOGUE METHODS -------------------------
    #region DIALOGUE METHODS

    // Method to call for executing the dialogue
    public void StartDialogue()
    {
        StartCoroutine(DialoguesShowcase());
    }

    #endregion
    
    // ----------------------- COROUTINES -------------------------
    #region COROUTINES

    

    #endregion
    
    // ------------------------ DEBUGGERS -------------------------
    #region DEBUGGERS
    
    IEnumerator DialoguesShowcase()
    {
        // Prints all the dialogues in the console
        foreach (var dialogueClass in dialogueInfo.Dialogues)
        {
            foreach (string dialogueLines in dialogueClass.DialogueLines)
            {
                dialogueField.text = dialogueLines;
                
                yield return new WaitForSeconds(3f);
                
                debuggerNiAin.Log(dialogueLines);
            }
        }
        
        // Triggers all triggerable included under this Event Array in the Inspector
        onDialogueDone?.Invoke();
    }
    
    #endregion
}
