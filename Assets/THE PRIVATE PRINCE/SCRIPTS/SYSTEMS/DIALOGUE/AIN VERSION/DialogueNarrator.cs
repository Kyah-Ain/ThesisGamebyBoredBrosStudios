using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.InputSystem;
using UnityEngine.Events;
using TMPro;

public enum NpcState
{
    BeforeQuest,
    OnQuest
}

// Required DebuggerNiAinPjls.cs for this to be able to monitor debugs, otherwise use the old one
[RequireComponent(typeof(DebuggerNiAinPjls))]
public class DialogueNarrator : MonoBehaviour
{
    // ------------------------- VARIABLES -------------------------
    [Header("EVENTS")]
    public UnityEvent onDialogueDone;
    
    [Header("REFERENCES")]
    [SerializeField] DebuggerNiAinPjls debuggerNiAin; // Custom debugging script from your dev Ain
    [SerializeField] DialogueInfoSO dialogueInfo; // Container for the Scriptable Dialogue's data
    
    [Header("QUEST")]
    [SerializeField] QuestInfoSO questInfoForPoint; // Container for the Scriptable Quest's Data
    string questId;
    QuestState currentQuestState;
    
    [Header("UI")]
    [SerializeField] TextMeshProUGUI dialogueField; // Reference to the UI Text that would output the dialogue

    [Header("STATUS")] 
    [SerializeField] NpcState currentNpcState = NpcState.BeforeQuest; // Basis for what dialogue lines the NPC should use
    [SerializeField] Dialogues currentDialogueClass; // Tracks the current dialogue library the NPC is using
    [SerializeField] string[] currentLines; // Tracks the current dialogue lines the NPC is using
    [SerializeField] int currentDialogueWeek; // Basis for what library of dialogue lines the NPC should use
    [SerializeField] int currentDialogueStep; // Basis for what line in the dialogue lines the NPC should use
    [SerializeField] bool hasNarratedRandomly;
    
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
        
        // Assigns a quest id from the QuestInfoSO
        questId = questInfoForPoint.id;
    }
    
    // OnEnable is called when the object becomes enabled and active
    void OnEnable()
    {
        Subscribe();
    }

    // OnDisable is called when the object becomes disabled
    void OnDisable()
    {
        UnSubscribe();
    }
    
    // Start is called before the first frame update
    void Start()
    {
        // Initialized the starting dialogue
        UpdateCurrentDialogue(currentNpcState, currentDialogueWeek);
    }

    // Update is called once per frame
    void Update()
    {
        // Updates the NPC dialogues by switching the enum (for Debugging Purposes)
        // ChangeNpcState(currentNpcState);
    }
    
    #endregion
    
    // ------------------------- SUBSCRIPTIONS -------------------------
    #region SUBSCRIPTIONS

    // Method to subscribe your local method to an event trigger
    void Subscribe()
    {
        // Set subscriptions of these methods to an event
        // Left (Event Call) += Right (Method that would be called)
        GameEventsManager.Instance.inputEvents.onProceed += StartDialogue;
        GameEventsManager.Instance.questEvents.onQuestStateChange += QuestStateChange;
    }

    // Method to UnSubscribe your local method to an event trigger
    void UnSubscribe()
    {
        // UnSubscribe them methods to an event
        // Left (Event Call) -= Right (Method that would be called)
        GameEventsManager.Instance.inputEvents.onProceed -= StartDialogue;
        GameEventsManager.Instance.questEvents.onQuestStateChange -= QuestStateChange;
    }
    
    // ------------------------- EVENT LISTENERS -------------------------

    // Method to update the Quest State for this point 
    void QuestStateChange(Ain.Quest quest)
    {
        // Checks if the update receiving is meant for this quest
        if(quest.info.id.Equals(questId))
        {
            // Update the state of this from the passed state value from the caller
            currentQuestState = quest.state;
            
            // Evaluates if the quest can be start 
            if (currentQuestState.Equals(QuestState.CAN_START))
            {
                ChangeNpcState(NpcState.BeforeQuest);
            }
            // Evaluates if the quest is in progress
            else if (currentQuestState.Equals(QuestState.IN_PROGRESS))
            {
                ChangeNpcState(NpcState.OnQuest);
            }
        }
    }
    
    #endregion
    
    // ---------------------- NPC METHODS -------------------------
    #region NPC METHODS
    
    // Method to switch the State of the NPC's dialogue set
    public void ChangeNpcState(NpcState newNpcState)
    {
        // Overwrites the stored npc state with the updated one
        currentNpcState = newNpcState;
        
        // Overwrites the stored npc state with the updated one
        UpdateCurrentDialogue(newNpcState, currentDialogueWeek);
    }
    
    #endregion
    
    // -------------------- DIALOGUE METHODS -------------------------
    #region DIALOGUE METHODS

    // Method to call Dialogue Narration from anywhere
    public void StartDialogue()
    {
        NarrateDialogue();
        
        // Iterates through all Dialogues assign to this script (for Debugging Purposes)
        // StartCoroutine(DialoguesShowcase());
    }
    
    // Overload Method to call Dialogue Narration from the Unity New Input System
    public void StartDialogue(InputAction.CallbackContext context)
    {
        NarrateDialogue();
    }
    
    // Method to call for executing the dialogue one by one
    void NarrateDialogue()
    {
        // Proceeds only if this NPC was talked before acquiring its Quest
        if (currentNpcState == NpcState.BeforeQuest)
        {
            NarrateByLines();
        }
        // Proceeds only if this NPC was talked after acquiring its Quest
        else if (currentNpcState == NpcState.OnQuest)
        {
            NarrateRandomly();
        }
    }

    // Method to call for Narrating Dialogues by line
    void NarrateByLines()
    {
        // Calls the dialogue lines checker
        if (IsThereMoreLine(currentLines))
        {
            // Updates the dialogue UI with the line retrieved from the Scriptable Dialogue
            dialogueField.text = currentLines[currentDialogueStep];
            
            // Increments the dialogue to the next line 
            currentDialogueStep++;
        }
        else
        {
            // Triggers all triggerable included under this Event Array in the Inspector
            onDialogueDone?.Invoke();
            
            // Decrements the dialogue back to the last line 
            currentDialogueStep--;
            
            debuggerNiAin.Log("Trying to End Convo...");
        }
    }
    
    // Method to call for Narrating Dialogues Randomly
    void NarrateRandomly()
    {
        // Don't narrate if there's nothing to narrate
        if (currentLines == null || currentLines.Length == 0)
        {
            return;
        }
    
        // Proceeds only if we're already in dialogue
        if (hasNarratedRandomly)
        {
            // Flips the dialogue to be re-opened on the next Interaction
            hasNarratedRandomly = false;
            
            // Triggers all triggerable included under this Event Array in the Inspector
            onDialogueDone?.Invoke();
        }
        // Proceeds if we're not any of the condition above
        else
        {
            // Generates a random number but still bounds to the length of the current dialogue lines
            int randomNum = Random.Range(0, currentLines.Length);
        
            // Updates the dialogue UI with the line retrieved from the Scriptable Dialogue
            dialogueField.text = currentLines[randomNum];
            
            // Flips the dialogue to be closed on the next Interaction
            hasNarratedRandomly = true;
        }
    }

    #endregion
    
    // ----------------------- COROUTINES -------------------------
    #region COROUTINES

    

    #endregion
    
    // ----------------------- HELPERS -------------------------
    #region HELPERS
    
    // Method to ask if there are lines left to iterate
    bool IsThereMoreLine(string[] arrayOfLinesToCheck)
    {
        // Checks if we still have lines to iterate
        return arrayOfLinesToCheck != null &&
               currentDialogueStep < arrayOfLinesToCheck.Length;
    }
    
    // Method to ask if there are dialogues left to iterate
    bool IsThereMoreWeek(int weekToCheck)
    {
        // Checks if we still have dialogues to iterate
        return weekToCheck >= 0 &&
               weekToCheck < dialogueInfo.Dialogues.Length;
    }
    
    // Method to call for Updating NPC's dialogue lines
    public void UpdateDialogueWeek()
    {
        // Gets the next dialogue week
        int nextDialogueWeek = currentDialogueWeek + 1;
        
        // Checks if there's still another dialogue week to iterate
        if (!IsThereMoreWeek(nextDialogueWeek))
        {
            return;
        }
        
        // Increments the dialogue to the next class
        currentDialogueWeek = nextDialogueWeek;

        // Increments the dialogue to the next class
        UpdateCurrentDialogue(currentNpcState, currentDialogueWeek++);
    }
    
    // Method to call for Updating NPC's dialogue lines specifically
    public void JumpDialogueWeek(int weekToJump)
    {
        // Checks if the requested dialogue week exists
        if (!IsThereMoreWeek(weekToJump))
        {
            return;
        }
        
        // Increments the dialogue to the next class
        UpdateCurrentDialogue(currentNpcState, weekToJump);
    }
        
    // Method to make the NPCs dialogue up to date
    void UpdateCurrentDialogue(NpcState newNpcState, int newDialogueWeek)
    {
        // Overwrites the stored dialogue library in used to the newly update one 
        currentDialogueClass = dialogueInfo.Dialogues[newDialogueWeek];
        
        // Switch the current lines in used to a new one
        currentLines = SwitchLines(newNpcState);
        
        // Also updates the NPC state flag
        currentNpcState = newNpcState;
        
        // // Resets dialogue steps 
        // currentDialogueStep = 0;
    }
    
    // Method to switch from one NPC state to another
    string[] SwitchLines(NpcState dialogueType)
    {
        // Evaluates the value passed and corresponds it to the matched case
        switch (dialogueType)
        {
            // Case to match if the NPC state was BeforeQuest
            case NpcState.BeforeQuest:
                return currentDialogueClass.DialogueLines;
            
            // Case to match if the NPC state was OnQuest
            case NpcState.OnQuest:
                return currentDialogueClass.IdleLines;
            
            // Default Case if there's no match
            default:
                return currentDialogueClass.DialogueLines;
        }
    }

    #endregion
    
    // ------------------------ DEBUGGERS -------------------------
    #region DEBUGGERS
    
    // Coroutine Method to call for executing the dialogues
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
