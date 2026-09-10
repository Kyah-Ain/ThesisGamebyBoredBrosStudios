using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;
using UnityEngine.Events;

// Required DebuggerNiAinPjls.cs for this to be able to monitor debugs, otherwise use the old one
[RequireComponent(typeof(DebuggerNiAinPjls))]
public class InteractionPrompt : MonoBehaviour
{
    // ------------------------- VARIABLES -------------------------

    [Header("REFERENCES")]
    [SerializeField] DebuggerNiAinPjls debuggerNiAin; // Custom debugging script from your dev Ain

    [Header("EVENTS")]
    [SerializeField] UnityEvent onTriggerEnter;
    [SerializeField] UnityEvent onTriggerExit;

    [field: Header("PROMPTS")]
    // Accessable but not Modifieable containers for the Prompt's lines
    [field: SerializeField] public DialogueObject defaultLine { get; private set; }
    [field: SerializeField] public DialogueObject interactedLine { get; private set; }

    [Header("UI")]
    // [SerializeField] GameObject promptPanel; // Refers to the Prompt's Visuals
    [SerializeField] TextMeshProUGUI promptTextField; // Refers to the Prompt's text field

    // ------------------------- UNITY METHODS -------------------------

    // Built-In Unity method that called when this script's gameObject is first initialized
    public void Awake()
    {
        // Checks if our reference for the script was not set
        if (debuggerNiAin == null)
        {
            // If it is not, then set it automatically by looking for the script class from this object
            debuggerNiAin = this.GetComponent<DebuggerNiAinPjls>();
        }

        // Sets the prompt to default when first loaded
        SwapPromptToDefault();
    }

    // Built-In Unity method that called when a gameObject with a Collider enters
    private void OnTriggerEnter(Collider actor)
    {
        // Filters the trigger to only responds to 'Player' tagged requests
        if (!actor.CompareTag("Player")) return;

        // Executes logics under this event set in the Inspector
        onTriggerEnter?.Invoke();

        SwapPromptLine(interactedLine.Dialogue[0]);
    }

    // Built-In Unity method that called when a gameObject with a Collider exits
    private void OnTriggerExit(Collider actor)
    {
        // Filters the trigger to only responds to 'Player' tagged requests
        if (!actor.CompareTag("Player")) return;
        
        // Executes logics under this event set in the Inspector 
        onTriggerExit?.Invoke();

        SwapPromptToDefault();
    }

    // ------------------------- PROMPT METHODS -------------------------

    // Callable Method for swapping prompt lines
    public void SwapPromptLine(string processedText)
    {
        // Overwrites the prompt text with the new one
        promptTextField.text = processedText;
    }

    // Callable Method for restoring default prompt line
    public void SwapPromptToDefault()
    {
        SwapPromptLine(defaultLine.Dialogue[0]);
    }
}