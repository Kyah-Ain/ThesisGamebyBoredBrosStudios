using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestStep_Interact : Ain.QuestStep
{
    // ----------------------- TASK METHODS -------------------------

    // Method to execute when interaction has been fulfilled
    public void FulfillInteraction()
    {
        StepComplete();
    }

    // ----------------------- PARENT CONTACTS -------------------------

    // Method to call for completing a Step
    protected virtual void StepComplete()
    {
        base.FinishQuestStep();
    }
}
