using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestStep_PickupThePhone : QuestStep
{
    // --------------------------- VARIABLES -------------------------

    // Add Variables Here If Needed...
    private bool _questReadyToComplete = false; // Flag that gets set when the player picks the correct response

    // ------------------------- UNITY METHODS -------------------------

    //// Built-In Unity method that called when a gameObject with a Collider enters
    //private void OnTriggerEnter(Collider actor)
    //{
    //    // Filters the trigger event to only respond to a 'Player' tagged gameObject
    //    if (actor.CompareTag("Player"))
    //    {
    //        // Finish this quest 
    //        FinishQuestStep();
    //    }
    //}

    // Called the moment the player picks a response
    public void PreparesQuestCompletion()
    {
        _questReadyToComplete = true;
    }

    // Method that should be called when a dialgoue is finish
    public void CompleteQuest()
    {
        if (_questReadyToComplete)
        {
            // Finish this quest 
            FinishQuestStep();
        }
    }

    // Abstract method that includes any variables here to be saved from the Saving System for Quest Persistent
    protected override void SetQuestStepState(string state)
    {
        // No special state needed for inspect quests
        // We just use this to fulfill the requirement of the abstract method in the base class
    }
}