using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))] // Ensures that this script was attached to a gameObject with a Box Collider (Just Remove This If You Would Like To Use Other Collider)
public class QuestStep_NarrateDialogue : QuestStep
{
    // --------------------------- VARIABLES -------------------------

    // Add Variables Here If Needed...

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

    // Method that should be called when a dialgoue is finish
    public void CompletesDialogue()
    {
        // Finish this quest 
        FinishQuestStep();
    }

    // Abstract method that includes any variables here to be saved from the Saving System for Quest Persistent
    protected override void SetQuestStepState(string state)
    {
        // No special state needed for inspect quests
        // We just use this to fulfill the requirement of the abstract method in the base class
    }
}