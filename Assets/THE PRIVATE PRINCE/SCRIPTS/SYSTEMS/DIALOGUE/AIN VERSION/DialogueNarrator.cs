using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.InputSystem;
using UnityEngine.Events;
using TMPro;

// Required DebuggerNiAinPjls.cs for this to be able to monitor debugs, otherwise use the old one
[RequireComponent(typeof(DebuggerNiAinPjls))]
public class DialogueNarrator : MonoBehaviour
{
    // ------------------------- VARIABLES -------------------------
    [Header("DIALOGUE GATE EVENTS")]
    public UnityEvent onDialogueStarted; 
    public UnityEvent onDialogueDone;
    
    [Header("REFERENCES")]
    [SerializeField] protected DebuggerNiAinPjls debuggerNiAin; // Custom debugging script from your dev Ain

    [SerializeField] DialogueEffect dialogueEffect; // Custom script for adding effect to the Narration
    
    [Header("DIALOGUE")]
    [SerializeField] DialogueLines dialogueLines; // Dialogue data to narrate by lines
    
    [Header("UI")]
    [SerializeField] TextMeshProUGUI dialogueField; // Reference to the UI Text that would output the dialogue

    [Header("STATUS")] 
    protected string[] currentLines; // The temporary holder of the current active dialogue lines
    protected int currentDialogueStep; // Tracker for what line we currently are in the dialogue lines
    
    // ----------------------- UNITY METHODS -------------------------
    #region UNITY METHODS
    
    // Awake is called when this script was first initialized & loaded
    protected virtual void Awake()
    {
        // Checks if our reference for the script was not set
        if (debuggerNiAin == null)
        {
            // If it is not, then set it automatically by looking for the script class from this object
            debuggerNiAin = this.GetComponent<DebuggerNiAinPjls>();
        }
        
        // Uses the simple dialogue assigned from the Inspector
        SetDialogueLines(dialogueLines.speechLines);
    }
    
    // OnEnable is called when the object becomes enabled and active
    protected virtual void OnEnable()
    {
        Subscribe();
    }

    // OnDisable is called when the object becomes disabled
    protected virtual void OnDisable()
    {
        UnSubscribe();
    }

    // // OnStart is called once before the first frame update
    // protected virtual void Start()
    // {
    //     // Uses the simple dialogue assigned from the Inspector
    //     SetDialogueLines(dialogueLines.speechLines);
    // }

    #endregion
    
    // ------------------------- SUBSCRIPTIONS -------------------------
    #region SUBSCRIPTIONS

    // Method to subscribe your local method to an event trigger
    protected virtual void Subscribe()
    {
        // Set subscriptions of these methods to an event
        // Left (Event Call) += Right (Method that would be called)
        GameEventsManager.Instance.inputEvents.onProceed += StartDialogue;
    }

    // Method to UnSubscribe your local method to an event trigger
    protected virtual void UnSubscribe()
    {
        // UnSubscribe them methods from an event
        // Left (Event Call) -= Right (Method that would be called)
        GameEventsManager.Instance.inputEvents.onProceed -= StartDialogue;
    }
    
    #endregion
    
    // -------------------- DIALOGUE METHODS -------------------------
    #region DIALOGUE METHODS

    // Method to call Dialogue Narration from anywhere
    public virtual void StartDialogue()
    {
        NarrateByLines();
    }
    
    // Overload Method to call Dialogue Narration from the Unity New Input System
    public void StartDialogue(InputAction.CallbackContext context)
    {
        StartDialogue(); 
    }
    
    // Method to call for Narrating Dialogues by line
    protected void NarrateByLines()
    {
        debuggerNiAin.Warn("NarrateByLines has been pressed!");

        // Don't narrate if there's nothing to narrate
        if (currentLines == null || currentLines.Length <= 0)
        {
            return;
        }
        
        // If a line is currently being narrated, skip its effect (if there's any)
        if (TrySkipDialogueEffect())
        {
            return;
        }

        // Only broadcast at the initiation of the dialogue
        if (currentDialogueStep == 0)
        {
            // Broadcasts to the listeners that a dialogue has been triggered
            onDialogueStarted?.Invoke();
        }
        
        // Continue narrating the dialogue
        if (currentDialogueStep < currentLines.Length)
        {
            // Outputs the current dialogue line 
            DisplayLine(currentLines[currentDialogueStep]);

            // Advances to the next dialogue line
            currentDialogueStep++;
        }
        // Dialogue has reached the end
        else
        {
            FinishDialogue();
        }
    }
    
    // Finishes the current dialogue
    protected void FinishDialogue()
    {
        // Reset's the dialogue run to the first line
        currentDialogueStep = 0;

        // Parent's main responsibility when dialogue ends
        onDialogueDone?.Invoke();

        // Gives inheritors an opportunity to react afterward
        OnDialogueFinished();
    }

    // Optional hook for child classes
    protected virtual void OnDialogueFinished()
    {
    }

    #endregion
    
    // ----------------------- HELPERS -------------------------
    #region HELPERS
    
    // Changes what lines the narrator should currently narrate
    protected void SetDialogueLines(string[] lines)
    {
        // Sets the current line to read from the lines being passed
        currentLines = lines;
        
        // Reset's the dialogue run to the first line when changing lines 
        currentDialogueStep = 0; 
    }

    // Allows children to directly display a specific line
    protected void DisplayLine(string line)
    {
        // Uses the narration effect if one is available and enabled
        if (dialogueEffect != null && dialogueEffect.EnableNarrationEffect)
        {
            dialogueEffect.StartDialogueEffect(line);
            return;
        }

        // Otherwise output the dialogue immediately
        dialogueField.text = line;
    }
    
    // Checks if an effect is currently narrating and skips it
    protected bool TrySkipDialogueEffect()
    {
        // No usable narration effect
        if (dialogueEffect == null ||
            !dialogueEffect.EnableNarrationEffect ||
            !dialogueEffect.IsNarrating)
        {
            return false;
        }

        // Complete the currently narrating line
        dialogueEffect.SkipDialogue();

        return true;
    }

    #endregion
}