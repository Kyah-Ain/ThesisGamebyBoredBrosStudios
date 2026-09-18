using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.InputSystem;

public class QuestStepTestCollect : Ain.QuestStep
{
    // ------------------------- VARIABLES -------------------------

    [SerializeField] int collectablesNeeded = 5;
    [SerializeField] int collectedCount;
    
    // ----------------------- FULFILL METHODS -------------------------
    #region UNITY METHODS

    // ...
    public void UpdateCollections()
    {
        // ...
        collectedCount++;
        
        // ...
        if (collectedCount >= collectablesNeeded)
        {
            MissionComplete();
        }
    }

    #endregion
    
    // ----------------------- PARENT CONTACTS -------------------------

    // Method to call for completing a Quest
    public void MissionComplete()
    {
        base.FinishQuestStep();
    }
}