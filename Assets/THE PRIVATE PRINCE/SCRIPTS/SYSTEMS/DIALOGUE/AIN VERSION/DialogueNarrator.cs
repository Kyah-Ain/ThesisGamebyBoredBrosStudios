using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.InputSystem;
using UnityEngine.Events;
using TMPro;

public enum DialogueState
{
    Idle,
    HasRequest,
    WaitingForCompletion,
    CanFinishRequest
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
    
    [Header("QUEST")]
    [SerializeField] QuestInfoSO[] QuestInfo; // Container for the Scriptable Quest's Data
    private string _questId; // Container for the questId we want this script to correlates to
    private QuestState _currentQuestState; // Container for the quest State we want this script to correlates to
    private int _targetQuest = 0; // Basis for what questId in Quest the lines corresponds to
    
    [Header("DIALOGUE")]
    [SerializeField] DialogueInfoSO[] DialogueInfo; // Container for the Scriptable Dialogue's data
    private string[] _currentLines; // Tracks the current dialogue lines the NPC is using
    private int currentDialogueWeek; // Basis for what library of dialogue lines the NPC should use
    private int _currentDialogueStep = 0; // Basis for what line in the dialogue lines the NPC should use
    
    [Header("UI")]
    [SerializeField] TextMeshProUGUI dialogueField; // Reference to the UI Text that would output the dialogue

    [Header("STATUS")] 
    [SerializeField] DialogueState dialogueState = DialogueState.Idle; // Basis for what dialogue lines the NPC should use
    private bool _hasNarratedRandomly; // Flag for knowing if the next interaction should close or re-open the dialogue
    
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
        
        // Initialized the starting Quest to track with dialogue
        UpdateQuestAssign(_targetQuest); // _targetQuest = 0
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
        // // Initialized the starting dialogue
        // SwitchLines(DialogueState.Idle);
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
        // UnSubscribe them methods from an event
        // Left (Event Call) -= Right (Method that would be called)
        GameEventsManager.Instance.inputEvents.onProceed -= StartDialogue;
        GameEventsManager.Instance.questEvents.onQuestStateChange -= QuestStateChange;
    }
    
    // ------------------------- EVENT LISTENERS -------------------------

    // Method to update the Quest State for this point 
    void QuestStateChange(Ain.Quest quest)
    {
        // Checks if the update receiving is meant for this quest
        if (quest.info.id.Equals(_questId))
        {
            // Update the state of this from the passed state value from the caller
            _currentQuestState = quest.state;

            // Chooses the suitable case base on the parameter passed
            switch (_currentQuestState)
            {
                // Dialogue logic for No Quest given
                case QuestState.REQUIREMENTS_NOT_MET:
                    SwitchLines(DialogueState.Idle);
                    break;
                
                // Dialogue logic for when there's a Quest wants to be given
                case QuestState.CAN_START:
                    SwitchLines(DialogueState.HasRequest);
                    break;
                
                // Dialogue logic for when there's a Quest waiting to be fulfilled
                case QuestState.IN_PROGRESS:
                    SwitchLines(DialogueState.WaitingForCompletion);
                    break;
                
                // Dialogue logic for when there's a Quest waiting to be finished
                case QuestState.CAN_FINISH:
                    SwitchLines(DialogueState.CanFinishRequest);
                    break;
                
                // Dialogue logic for when finished a Quest
                case QuestState.FINISHED:
                    UpdateQuestAssign(++_targetQuest);
                    break;
            }
        }
    }
    
    #endregion
    
    // -------------------- DIALOGUE METHODS -------------------------
    #region DIALOGUE METHODS

    // Method to call Dialogue Narration from anywhere
    public void StartDialogue()
    {
        NarrateDialogue();
    }
    
    // Overload Method to call Dialogue Narration from the Unity New Input System
    public void StartDialogue(InputAction.CallbackContext context)
    {
        NarrateDialogue();
    }
    
    // Method to call for executing the dialogue one by one
    void NarrateDialogue()
    {
        // Proceeds only if the dialogue state was in Idle
        if (dialogueState == DialogueState.Idle)
        {
            NarrateRandomly();
        }
        // Proceeds only if the ifs above hasn't fulfilled
        else
        {
            NarrateByLines();
        }
    }

    // Method to call for Narrating Dialogues by line
    void NarrateByLines()
    {
        // Don't narrate if there's nothing to narrate
        if (_currentLines == null || _currentLines.Length <= 0)
        {
            return;
        }
        
        // Calls the dialogue lines checker
        if (_currentDialogueStep < _currentLines.Length)
        {
            // Updates the dialogue UI with the line retrieved from the Scriptable Dialogue
            dialogueField.text = _currentLines[_currentDialogueStep];
            
            // Increments the dialogue to the next line 
            _currentDialogueStep++;
        }
        else
        {
            // Triggers all triggerable included under this Event Array in the Inspector
            onDialogueDone?.Invoke();
            
            // Decrements the dialogue back to the last line 
            // currentDialogueStep--;
            
            // Resets the conversation from the beginning
            _currentDialogueStep = 0;
            
            // debuggerNiAin.Log("Trying to End Convo...");
        }
    }
    
    // Method to call for Narrating Dialogues Randomly
    void NarrateRandomly()
    {
        // Don't narrate if there's nothing to narrate
        if (_currentLines == null || _currentLines.Length <= 0)
        {
            return;
        }
    
        // Proceeds only if we're already in dialogue
        if (_hasNarratedRandomly)
        {
            // Flips the dialogue to be re-opened on the next Interaction
            _hasNarratedRandomly = false;
            
            // Triggers all triggerable included under this Event Array in the Inspector
            onDialogueDone?.Invoke();
        }
        // Proceeds if we're not any of the condition above
        else
        {
            // Generates a random number but still bounds to the length of the current dialogue lines
            int randomNum = Random.Range(0, _currentLines.Length);
        
            // Updates the dialogue UI with the line retrieved from the Scriptable Dialogue
            dialogueField.text = _currentLines[randomNum];
            
            // Flips the dialogue to be closed on the next Interaction
            _hasNarratedRandomly = true;
        }
    }

    #endregion
    
    // ----------------------- HELPERS -------------------------
    #region HELPERS
    
    // Method to switch dialogue lines
    void SwitchLines(DialogueState state)
    {
        // Updates the dialogue state
        dialogueState = state;
        
        // Resets the dialogue lines from the start
        _currentDialogueStep = 0;
        
        // Evaluates the value passed and assigns the current lines the dialogue gonna use
        switch (state)
        {
            // Dialogue for Idle State
            case DialogueState.Idle:
                _currentLines = DialogueInfo[currentDialogueWeek].DialogueLines.IdleLines;
                break;
            
            // Dialogue for HasRequest State
            case DialogueState.HasRequest:
                _currentLines = DialogueInfo[currentDialogueWeek].DialogueLines.HasRequestLines; 
                break;
            
            // Dialogue for WaitingForCompletion State
            case DialogueState.WaitingForCompletion:
                _currentLines = DialogueInfo[currentDialogueWeek].DialogueLines.WaitingForCompletionLines; 
                break;
            
            // Dialogue for CanFinishRequest State
            case DialogueState.CanFinishRequest:
                _currentLines = DialogueInfo[currentDialogueWeek].DialogueLines.CanFinishRequestLines; 
                break;
        }
    }
    
    // Method to update the Dialogue's Quest reference
    void UpdateQuestAssign(int questNumber)
    {
        // Checks if there are more quest to progress the dialogue with
        if (questNumber < QuestInfo.Length)
        {
            // Sets the quest ID to track in this script
            _questId = QuestInfo[questNumber].id;

            // Updates the quest counter tracker
            _targetQuest = questNumber;

            // Checks if there are more dialogue to progress with
            if (questNumber < DialogueInfo.Length)
            {
                // Updates the dialogue reference along with the quest
                currentDialogueWeek = questNumber;
            }

            // Refresh Quest Updates
            Ain.QuestManager.Instance.InitializedQuestStates();
        }
        // Defaults back to the last Dialogue's Idle if there are no progress to initiate
        else
        {
            SwitchLines(DialogueState.Idle);
        }
    }

    #endregion
}