using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestStep_Combat : QuestStep
{
    // --------------------------- VARIABLES -------------------------

    // Add Variables Here If Needed...
    private int enemiesToDefeat = 5;

    // ------------------------- UNITY METHODS -------------------------

    public void EnemyDied() 
    {
        enemiesToDefeat--;

        if (enemiesToDefeat <= 0)
        {
            CompleteQuest();
        }
    }

    // Method that should be called when a dialgoue is finish
    public void CompleteQuest()
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
