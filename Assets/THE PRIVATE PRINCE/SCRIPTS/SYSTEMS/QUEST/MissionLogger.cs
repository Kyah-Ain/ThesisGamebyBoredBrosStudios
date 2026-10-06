using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionLogger : MonoBehaviour
{
    // ------------------------- VARIABLES -------------------------

    [Header("REFERENCE")]
    [SerializeField] GameEventTrigger eventTrigger;
    [SerializeField] QuestInfoSO questInfoSO; 
    
    // ------------------------- LOG METHODS -------------------------

    // Method to broadcast the QuestLog
    public void UpdateQuestLog()
    {
        // Broadcaster
        eventTrigger.ExecuteEvents(questInfoSO.questName);
    }
}
