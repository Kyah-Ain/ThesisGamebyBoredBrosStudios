using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionLogger : MonoBehaviour
{
    // ------------------------- VARIABLES -------------------------

    [Header("REFERENCE")]
    [SerializeField] GameEventTrigger eventTrigger;
    [SerializeField] QuestInfoSO questInfoSO;
    
    // Retrieves the Quest's data using its Quest Id
    private Ain.Quest QuestData => Ain.QuestManager.Instance.GetQuestById(questInfoSO.id);
    
    // ------------------------- UNITY METHODS -------------------------
    
    // OnEnable is called when the object becomes enabled and active
    void OnEnable()
    {
        GameEventsManager.Instance.questEvents.onQuestStepProgress += OnQuestStepProgress;
    }

    // OnDisable is called when the object becomes disabled
    void OnDisable()
    {
        GameEventsManager.Instance.questEvents.onQuestStepProgress -= OnQuestStepProgress;
    }
    
    // ------------------------- LOG METHODS -------------------------

    // Method to broadcast the QuestLog
    public void UpdateQuestLog()
    {
        // ...
        Ain.Quest quest = QuestData;
        
        // ...
        if (quest == null) return;
        
        // ...
        string text = questInfoSO.questName;
        
        // // Retrieves the total questSteps to fulfill for a Quest
        // int totalSteps = questInfoSO.questStepPrefabs.Length;
        //
        // // Only proceeds if we atleast have 1 steps for a Quest
        // if (totalSteps > 0)
        // {
        //     // ...
        //     int displayStep = Mathf.Min(quest.currentQuestStepIndex + 1, totalSteps);
        //     
        //     // ...
        //     text += $"\nStep {displayStep}/{totalSteps}";
        // }
        
        // ...
        Ain.QuestStep step = quest.CurrentStep;
        
        // ...
        if (step != null && step.HasProgress) // != null (not ?.) so Unity's destroyed-object check applies
        {
            // ...
            text += $"\n{step.CurrentCount}/{step.TargetCount}";
        }
        
        // Broadcaster
        eventTrigger.ExecuteEvents(text);
    }
    
    // Method to broadcast the reset of the QuestLog
    public void ResetQuestLog()
    {
        // Broadcaster
        eventTrigger.ExecuteEvents("");
    }
    
    // --------------------------- HELPERS --------------------------
    
    // Method to only react to our own quest
    void OnQuestStepProgress(string id)
    {
        if (id == questInfoSO.id) UpdateQuestLog();
    }
}
