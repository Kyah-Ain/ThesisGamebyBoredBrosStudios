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
public class NPCNarrator : DialogueNarrator
{
    // ------------------------- VARIABLES -------------------------
    [Header("DIALOGUE EVENTS")]
    public UnityEvent onQuestionRaised;
    public UnityEvent onAcceptingRequest;
    public UnityEvent onFinishingRequest;
    
    [Header("QUEST")]
    [SerializeField] QuestInfoSO[] QuestInfo; // Container for the Scriptable Quest's Data
    private string _questId; // Container for the questId we want this script to correlates to
    private QuestState _currentQuestState; // Container for the quest State we want this script to correlates to
    private int _targetQuest = 0; // Basis for what questId in Quest the lines corresponds to
    
    [Header("DIALOGUE")]
    [SerializeField] DialogueInfoSO[] DialogueInfo; // Container for the Scriptable Dialogue's data
    private int currentDialogueWeek; // Basis for what library of dialogue lines the NPC should use

    [Header("STATUS")] 
    [SerializeField] DialogueState dialogueState = DialogueState.Idle; // Basis for what dialogue lines the NPC should use
    private bool _hasNarratedRandomly; // Flag for knowing if the next interaction should close or re-open the dialogue
    private bool _hasQuestionRaised; // Flag for stopping proceed input of a dialogue when there's a question
    private bool _isResponseDialogue; // Flag for prompting a message based on the user response before resetting the dialogue
    
    // ----------------------- UNITY METHODS -------------------------
    #region UNITY METHODS
    
    // Awake is called when this script was first initialized & loaded
    protected override void Awake()
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
    protected override void OnEnable()
    {
        Subscribe();
    }

    // OnDisable is called when the object becomes disabled
    protected override void OnDisable()
    {
        UnSubscribe();
    }
    
    // // OnStart is called once before the first frame update
    // protected override void Start()
    // {
    //     // Initialized the starting Quest to track with dialogue
    //     UpdateQuestAssign(_targetQuest); // _targetQuest = 0
    // }
    
    #endregion
    
    // ------------------------- SUBSCRIPTIONS ------------------------
    #region SUBSCRIPTIONS

    // Method to subscribe your local method to an event trigger
    protected override void Subscribe()
    {
        // Set subscriptions of these methods to an event
        // Left (Event Call) += Right (Method that would be called)
        base.Subscribe();
        GameEventsManager.Instance.questEvents.onQuestStateChange += QuestStateChange;
    }

    // Method to UnSubscribe your local method to an event trigger
    protected override void UnSubscribe()
    {
        // UnSubscribe them methods from an event
        // Left (Event Call) -= Right (Method that would be called)
        base.UnSubscribe(); 
        GameEventsManager.Instance.questEvents.onQuestStateChange -= QuestStateChange;
    }
    
    // ------------------------- EVENT LISTENERS -------------------------

    // Method to update the Quest State for this point 
    void QuestStateChange(Ain.Quest quest)
    {
        // Ignore Quest updates that aren't intended for this NPC
        if (!quest.info.id.Equals(_questId))
        {
            return;
        }
        
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
                // SwitchLines(DialogueState.WaitingForCompletion);
                dialogueState = DialogueState.WaitingForCompletion;
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
    
    #endregion
    
    // -------------------- DIALOGUE METHODS -------------------------
    #region DIALOGUE METHODS

    // Method to call Dialogue Narration from anywhere
    public override void StartDialogue()
    {
        // Don't proceed while waiting for player's response
        if (_hasQuestionRaised)
        {
            return;
        }

        // Temporary Accept / Decline response
        if (_isResponseDialogue)
        {
            base.NarrateByLines();
            return;
        }

        // Random Idle / Waiting lines
        if (dialogueState == DialogueState.Idle ||
            dialogueState == DialogueState.WaitingForCompletion)
        {
            NarrateRandomly();
            return;
        }

        // Detect the final line of the Quest offer
        if (dialogueState == DialogueState.HasRequest &&
            currentLines != null &&
            currentDialogueStep == currentLines.Length - 1)
        {
            onQuestionRaised?.Invoke();

            _hasQuestionRaised = true;
        }

        // Let the parent perform the actual narration
        base.NarrateByLines();
    }
    
    // Method to call for Narrating Dialogues Randomly
    void NarrateRandomly()
    {
        // Holds a reference to the current random responses and store it temporarily
        string[] randomResponses = DialogueInfo[currentDialogueWeek].dialogue.WaitingForCompletionLines;
        
        // Don't narrate if there's nothing to narrate
        if (randomResponses == null || randomResponses.Length <= 0)
        {
            return;
        }
    
        // Proceeds only if we're not in dialogue yet
        if (!_hasNarratedRandomly)
        {
            // Generates a random number but still bounds to the length of the current dialogue lines
            int randomNum = Random.Range(0, randomResponses.Length);
        
            // Updates the dialogue UI with the line retrieved from the Scriptable Dialogue
            base.DisplayLine(randomResponses[randomNum]);
            
            // Flips the dialogue to be closed on the next Interaction
            _hasNarratedRandomly = true;
        }
        // Proceeds if we're not any of the condition above
        else
        {
            // Flips the dialogue to be re-opened on the next Interaction
            _hasNarratedRandomly = false;
            
            base.FinishDialogue();
        }
    }
    
    // Method to call for accepting the Quest
    public void Accept()
    {
        // Accept the quest first, so the dialogue state can update
        onAcceptingRequest?.Invoke();

        // Start the acceptance response, or skip it if empty
        ThrowResponse(
            DialogueInfo[currentDialogueWeek].dialogue.AcceptRequestLines
        );
    }
    
    public void Decline()
    {
        // Start the decline response, or skip it if empty
        ThrowResponse(
            DialogueInfo[currentDialogueWeek].dialogue.DeclineRequestLines
        );
    }

    #endregion
    
    // ----------------------- HELPERS -------------------------
    #region HELPERS
    
    // Method to switch dialogue lines
    void SwitchLines(DialogueState state)
    {
        // Updates the dialogue state
        dialogueState = state;
        
        // Evaluates the value passed and assigns the current lines the dialogue gonna use
        switch (state)
        {
            // Dialogue for Idle State
            case DialogueState.Idle:
                
                base.SetDialogueLines(
                    DialogueInfo[currentDialogueWeek].dialogue.IdleLines
                );
                break;
            
            // Dialogue for HasRequest State
            case DialogueState.HasRequest:
                
                base.SetDialogueLines(
                    DialogueInfo[currentDialogueWeek].dialogue.HasRequestLines
                );
                break;
            
            // Dialogue for WaitingForCompletion State
            case DialogueState.WaitingForCompletion:
                
                base.SetDialogueLines(
                    DialogueInfo[currentDialogueWeek].dialogue.WaitingForCompletionLines
                );
                break;
            
            // Dialogue for CanFinishRequest State
            case DialogueState.CanFinishRequest:
                
                base.SetDialogueLines(
                    DialogueInfo[currentDialogueWeek].dialogue.CanFinishRequestLines
                ); 
                break;
        }
    }
    
    // Method to start a temporary response dialogue
    void ThrowResponse(string[] responseLines)
    {
        // Reset the previous conversation's temporary status
        _hasQuestionRaised = false;
        _hasNarratedRandomly = false;

        // If the response has no lines, skip it
        if (responseLines == null || responseLines.Length == 0)
        {
            _isResponseDialogue = false;

            // Restore the normal dialogue for the current quest state
            SwitchLines(dialogueState);

            // Close the dialogue UI through your existing event
            onDialogueDone?.Invoke();

            return;
        }

        // Otherwise, start narrating the temporary response
        _isResponseDialogue = true;

        base.SetDialogueLines(responseLines);
        
        base.NarrateByLines();
    }
    
    // Called by the parent whenever narration finishes
    protected override void OnDialogueFinished()
    {
        // Quest can now be finished
        if (_currentQuestState == QuestState.CAN_FINISH)
        {
            onFinishingRequest?.Invoke();
        }

        // Restore normal dialogue after temporary response
        if (_isResponseDialogue)
        {
            _isResponseDialogue = false;

            SwitchLines(dialogueState);
        }
    }
    
    // Method to update the Dialogue's Quest reference
    void UpdateQuestAssign(int questNumber)
    {
        // Checks if there are more quest to progress the dialogue with
        if (questNumber < QuestInfo.Length)
        {
            // Reset the previous conversation
            _hasQuestionRaised = false;
            _hasNarratedRandomly = false;
            _isResponseDialogue = false;
            
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
