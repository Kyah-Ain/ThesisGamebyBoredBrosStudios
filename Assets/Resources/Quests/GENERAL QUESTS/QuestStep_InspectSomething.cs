using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestStep_InspectSomething : QuestStep
{
    // ------------------------- VARIABLES -------------------------

    public QuestStarter questStarter; // Reference to the QuestStarter.cs component for starting quests

    // ------------------------- UNITY METHODS -------------------------

    // Abstract method that requires implementation in derived classes (can have logic or not)
    protected override void SetQuestStepState(string state)
    {
        // No special state needed for inspect quests
        // We just use this to fulfill the requirement of the abstract method in the base class
    }

    // Needed for intera
    public void OnTriggerStay(Collider other)
    {
        
    }

    // ...
    public void AcceptQuest() 
    {
        // ...
        //questStarter.StartQuestById("Inspect_Something");
    }
}
