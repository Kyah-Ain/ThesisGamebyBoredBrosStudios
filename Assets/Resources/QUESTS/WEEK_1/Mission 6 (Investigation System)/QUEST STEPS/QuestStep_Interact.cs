using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestStepInteract : Ain.QuestStep
{
    // ------------------------- VARIABLES -------------------------

    [Header("SETTINGS")]
    [SerializeField] int interactionGoal; // The number of target interactable objects to find

    [Header("STATUS")]
    [SerializeField] [ReadOnly] private int interactedCount; // Tracks the number of interactables already found
    
    // ----------------------- TASK METHODS -------------------------
    
    // Method to process interacted objects
    public void ProcessInteraction()
    {
        // Increments the interacted objects count
        interactedCount++;
        
        // Checks if the interacted goal has been met
        if (interactedCount >= interactionGoal)
        {
            MissionComplete();
        }
    }

    // ----------------------- PARENT CONTACTS -------------------------

    // Method to call for completing a Quest
    void MissionComplete()
    {
        base.FinishQuestStep();
    }
}
