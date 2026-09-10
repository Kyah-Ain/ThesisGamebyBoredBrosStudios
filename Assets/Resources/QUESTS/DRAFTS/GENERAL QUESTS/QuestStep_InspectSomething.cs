using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestStep_InspectSomething : QuestStep
{
    // ------------------------ VARIABLES -------------------------

    // Add values here if needed

    // ------------------------- METHODS -------------------------

    // Abstract method that requires implementation in derived classes (can have logic or not)
    protected override void SetQuestStepState(string state)
    {
        // No special state needed for inspect quests
        // We just use this to fulfill the requirement of the abstract method in the base class
    }

    // Method to convert a data into string 
    private void UpdateState() 
    {
        // Use this when you have values in here you need to store the progress
        //ChangeState();
    }
}