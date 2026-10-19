using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.InputSystem;

public class QuestStep_TestCollect : Ain.QuestStep
{
    // ------------------------- VARIABLES -------------------------

    [SerializeField] int collectablesNeeded = 5;
    [SerializeField] int collectedCount;
    
    // ----------------------- FULFILL METHODS -------------------------
    #region FULFILL METHODS

    // ...
    public void UpdateCollections()
    {
        // ...
        collectedCount++;
        
        // ...
        if (collectedCount >= collectablesNeeded)
        {
            StepComplete();
        }
    }

    #endregion
    
    // ----------------------- PARENT CONTACTS -------------------------

    // Method to call for completing a Step
    protected virtual void StepComplete()
    {
        base.FinishQuestStep();
    }
}