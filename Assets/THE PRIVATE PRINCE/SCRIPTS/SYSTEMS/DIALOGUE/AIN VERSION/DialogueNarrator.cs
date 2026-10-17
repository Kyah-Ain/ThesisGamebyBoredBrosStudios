using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.InputSystem;
using UnityEngine.Events;
using TMPro;

// A whole conversation made of multiple dialogue sets (each set can have a different speaker)
[System.Serializable]
public class DialogueConversation
{
    public string conversationName; // Label to identify this conversation in the Inspector
    public DialogueLines[] sets; // The dialogue sets (speakers) that play in order for this conversation
}

// Required DebuggerNiAinPjls.cs for this to be able to monitor debugs, otherwise use the old one
[RequireComponent(typeof(DebuggerNiAinPjls))]
public class DialogueNarrator : MonoBehaviour
{
    // ------------------------- VARIABLES -------------------------
    [Header("DIALOGUE GATE EVENTS")]
    public UnityEvent onDialogueStarted; 
    // public UnityEvent onProceedDialogue;
    public UnityEvent onNarrationDone;
    public UnityEvent onDialogueDone;
    
    [Header("REFERENCES")]
    [SerializeField] protected DebuggerNiAinPjls debuggerNiAin; // Custom debugging script from your dev Ain
    [SerializeField] GameEventTrigger profileTrigger;
    [SerializeField] GameEventTrigger dialogueTrigger;
    [SerializeField] DialogueEffect dialogueEffect; // Custom script for adding effect to the Narration
    
    [Header("DIALOGUE")]
    [SerializeField] protected DialogueLines dialogueID; // Holds the UniqueID for the uniqueness of the Dialogue
    [SerializeField] DialogueLines[] dialogueLines; // Dialogue data to narrate by lines (one entry per dialogue set)
    [SerializeField] bool playRandomSet; // If true, picks only one random conversation and completes right after it
    [SerializeField] DialogueConversation[] conversations; // Conversations to randomly pick from (used when playRandomSet is true)
    
    [Header("UI")]
    [SerializeField] TextMeshProUGUI dialogueField; // Reference to the UI Text that would output the dialogue

    [Header("STATUS")] 
    public static string currentDialogueIdActive; // The ID of whichever narrator currently owns the dialogue (null if nobody)
    [SerializeField][ReadOnly] protected string[] currentLines; // The temporary holder of the current active dialogue lines
    [SerializeField][ReadOnly] protected int currentDialogueStep; // Tracker for what line we currently are in the dialogue lines
    [SerializeField][ReadOnly] protected int currentDialogueSet; // Tracker for what dialogue set (from dialogueLines) we currently are in
    [SerializeField][ReadOnly] protected int currentConversation; // Tracker for which conversation (from conversations) was picked
    
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
        
        // Children that supply their own lines skip this
        if (!UsesDialogueSets)
        {
            return;
        }

        // Uses the first dialogue set assigned from the Inspector
        if (HasDialogueSets)
        {
            SetDialogueLines(ActiveSets[0].speechLines);
        }
        else
        {
            debuggerNiAin.Warn($"[{name}] has no Dialogue Lines assigned in the Inspector.");
        }
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
        
        // Release ownership if we were mid-dialogue
        if (IsMine(currentDialogueIdActive))
        {
            currentDialogueIdActive = null;
            ResetDialogueProgress();
        }
    }

    // OnStart is called once before the first frame update
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
        GameEventsManager.Instance.inputEvents.onSubmit += OnSubmitPressed;
    }

    // Method to UnSubscribe your local method to an event trigger
    protected virtual void UnSubscribe()
    {
        // UnSubscribe them methods from an event
        // Left (Event Call) -= Right (Method that would be called)
        GameEventsManager.Instance.inputEvents.onSubmit -= OnSubmitPressed;
    }
    
    #endregion
    
    // -------------------- DIALOGUE METHODS -------------------------
    #region DIALOGUE METHODS

    // Method to call Dialogue Narration from anywhere
    public virtual void StartDialogue()
    {
        StartDialogue(OwnerId);
    }
    
    // Overload Method to call Dialogue Narration through an event
    public virtual void StartDialogue(string id)
    {
        // Not meant for this dialogue instance
        if (!IsMine(id)) return;

        // Claims ownership, or stops if another NPC already owns the dialogue
        if (!TryClaimDialogue()) return;

        // Lets this narrator (or its child) decide how the dialogue proceeds
        AdvanceDialogue();
    }
    
    // Overload Method to call Dialogue Narration from the Unity New Input System
    public void OnSubmitPressed(InputAction.CallbackContext context)
    {
        // Only continue if this NPC already owns the active dialogue
        if (!IsMine(currentDialogueIdActive)) return;

        // Lets this narrator (or its child) decide how the dialogue proceeds
        AdvanceDialogue();
        // StartDialogue(dialogueID.speechLines[0]); 
        // onProceedDialogue?.Invoke();
    }
    
    // Method to try owning the shared dialogue, so only one narrator talks at a time
    protected bool TryClaimDialogue()
    {
        // Another NPC already owns the dialogue, so block
        if (currentDialogueIdActive != null && !IsMine(currentDialogueIdActive))
        {
            debuggerNiAin.Warn($"[{name}] BLOCKED. '{currentDialogueIdActive}' owns the dialogue.");
            return false;
        }

        // Claim ownership so OnSubmitPressed lets us continue
        currentDialogueIdActive = OwnerId;
        return true;
    }

    // Method to give up ownership without finishing (for when there's nothing to narrate)
    protected void ReleaseDialogue()
    {
        if (IsMine(currentDialogueIdActive))
        {
            currentDialogueIdActive = null;
        }
    }
    
    // Method to call for Narrating Dialogues by line
    protected void NarrateByLines()
    {
        debuggerNiAin.Warn("NarrateByLines has been pressed!");

        // Don't narrate if there's nothing to narrate
        if (currentLines == null || currentLines.Length <= 0)
        {
            debuggerNiAin.Warn("Current Lines has been null or below 1");

            // Nothing to narrate, so don't leave the dialogue locked
            ReleaseDialogue();
            
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
            // Picks which conversation to narrate for this run (if random is enabled)
            if (playRandomSet)
            {
                PickRandomConversation();
            }

            // Broadcasts to the listeners that a dialogue has been triggered
            onDialogueStarted?.Invoke();
        }
        
        // Proceeds only if reached the last line in a dialogue set
        if (currentDialogueStep >= currentLines.Length)
        {
            // Moves to the next set, or completes the dialogue if there's none left
            if (!TryMoveToNextSet())
            {
                FinishDialogue();
                return;
            }
        }
        
        // Retrieves the current line based on the tracker
        string lineToNarrate = currentLines[currentDialogueStep];

        // Updates the tracker to a new dialogue line to be displayed
        currentDialogueStep++;

        // Outputs the current dialogue line    
        DisplayLine(lineToNarrate, GetSpeakerName());
    }
    
    // Method to move to the next dialogue set that has lines (returns false if there's none left)
    bool TryMoveToNextSet()
    {
        DialogueLines[] sets = ActiveSets;

        // Children that supply their own lines never move through the Inspector sets
        if (!UsesDialogueSets || sets == null)
        {
            return false;
        }

        // Keeps moving forward until a set that actually has lines is found
        while (currentDialogueSet < sets.Length - 1)
        {
            // Updates the tracker to a new dialogue set to be narrated
            currentDialogueSet++;

            DialogueLines nextSet = sets[currentDialogueSet];

            if (nextSet != null && nextSet.speechLines != null && nextSet.speechLines.Length > 0)
            {
                // Set the dialogue to the new dialogue set (also resets the step)
                SetDialogueLines(nextSet.speechLines);
                return true;
            }
        }

        return false;
    }
    
    // Method to pick one random conversation, so only that conversation gets narrated
    void PickRandomConversation()
    {
        // Children that supply their own lines never use the Inspector sets
        if (!UsesDialogueSets || conversations == null)
        {
            return;
        }

        // Collects only the conversations whose first set actually has lines
        List<int> validConversations = new List<int>();

        for (int i = 0; i < conversations.Length; i++)
        {
            DialogueConversation convo = conversations[i];

            if (convo != null &&
                convo.sets != null &&
                convo.sets.Length > 0 &&
                convo.sets[0] != null &&
                convo.sets[0].speechLines != null &&
                convo.sets[0].speechLines.Length > 0)
            {
                validConversations.Add(i);
            }
        }

        // Nothing usable to pick from, so keep whatever is currently loaded
        if (validConversations.Count == 0)
        {
            return;
        }

        // Updates the tracker to the randomly picked conversation
        currentConversation = validConversations[Random.Range(0, validConversations.Count)];

        // Starts from the first set of that conversation (also resets the step)
        currentDialogueSet = 0;
        SetDialogueLines(ActiveSets[0].speechLines);
    }
    
    // Finishes the current dialogue
    protected void FinishDialogue()
    {
        // Reset's the dialogue run back to the first set and first line
        ResetDialogueProgress();

        // Releases the ownership of the dialogue
        currentDialogueIdActive = null;

        // Gives inheritors an opportunity to react afterward
        OnDialogueFinished();
        
        // Parent's main responsibility when dialogue ends
        onDialogueDone?.Invoke();
    }
    
    // Method to reset the dialogue run back to its beginning
    protected void ResetDialogueProgress()
    {
        currentDialogueSet = 0;

        if (UsesDialogueSets && HasDialogueSets)
        {
            // Back to the first set (also resets the step)
            SetDialogueLines(ActiveSets[0].speechLines);
        }
        else
        {
            // Children that supply their own lines keep their current lines
            currentDialogueStep = 0;
        }
    }
    
    #endregion
    
    // -------------------- CHILD HOOKS -------------------------
    #region CHILD HOOKS
    
    // Optional Methods for child scripts to receive updates from its parents
    
    // Toggles if this narrator moves through the Inspector's dialogue sets
    // Children that supply their own lines through SetDialogueLines() should turn this off
    protected virtual bool UsesDialogueSets => true;
    
    // Method that called when the player proceeds the dialogue (Submit)
    // Children can override this to run their own proceed logic
    protected virtual void AdvanceDialogue()
    {
        NarrateByLines();
    }
    
    // Method that returns the name of whoever is currently speaking
    protected virtual string GetSpeakerName()
    {
        DialogueLines[] sets = ActiveSets;

        if (sets == null || sets.Length == 0)
        {
            return "";
        }

        int index = Mathf.Clamp(currentDialogueSet, 0, sets.Length - 1);

        return sets[index] != null ? sets[index].speakerName : "";
    }
    
    // Method that called when a line was finished iterating
    protected virtual void OnNarratingDone()
    {
        
    }
    
    // Method that called when the dialogue were to be finished
    protected virtual void OnDialogueFinished()
    {
    }

    #endregion
    
    // ----------------------- HELPERS -------------------------
    #region HELPERS
    
    // The ID that represents this narrator in the shared dialogue ownership
    protected string OwnerId
    {
        get
        {
            if (dialogueID != null && dialogueID.speechLines != null && dialogueID.speechLines.Length > 0)
            {
                return dialogueID.speechLines[0];
            }

            // Falls back to a per-object ID if no Dialogue ID was assigned in the Inspector
            return GetInstanceID().ToString();
        }
    }

    // The dialogue sets currently being narrated (the picked conversation in random mode, otherwise the Inspector's dialogueLines)
    protected DialogueLines[] ActiveSets
    {
        get
        {
            if (playRandomSet &&
                conversations != null &&
                currentConversation >= 0 &&
                currentConversation < conversations.Length &&
                conversations[currentConversation] != null)
            {
                return conversations[currentConversation].sets;
            }

            return dialogueLines;
        }
    }

    // Checks if there's at least one usable dialogue set assigned in the Inspector
    protected bool HasDialogueSets =>
        ActiveSets != null && ActiveSets.Length > 0 && ActiveSets[0] != null;
    
    protected bool IsMine(string id) =>
        string.Equals(id, OwnerId, System.StringComparison.OrdinalIgnoreCase);

    
    // Changes what lines the narrator should currently narrate
    protected void SetDialogueLines(string[] lines)
    {
        // Sets the current line to read from the lines being passed
        currentLines = lines;
        
        // Reset's the dialogue run to the first line when changing lines 
        currentDialogueStep = 0; 
    }

    // Allows children to directly display a specific line
    protected void DisplayLine(string line, string speaker = "")
    {
        // Uses the narration effect if one is available and enabled
        if (dialogueEffect != null && dialogueEffect.EnableNarrationEffect)
        {
            dialogueEffect.StartDialogueEffect(line, speaker);
            return;
        }

        // Temporary: Might be removed later
        if (dialogueField != null)
        {
            dialogueField.text = line;  
        }
        
        // Otherwise output the dialogue immediately
        dialogueTrigger.ExecuteEvents(line);
        profileTrigger.ExecuteEvents(speaker);

        // Without an effect, the line is already completely displayed
        NarrationDone();
    }
    
    // Called whenever a narration line has completely finished displaying
    public void NarrationDone()
    {
        // Broadcasts to external listeners
        onNarrationDone?.Invoke();

        // Gives inheritors an opportunity to react
        OnNarratingDone();
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