using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestStep_WK1_Mission1 : Ain.QuestStep
{
    // ----------------------- TASK METHODS -------------------------

    // Method to execute when interaction has been fulfilled
    public void FulfillInteraction()
    {
        MissionComplete();
    }

    // ----------------------- PARENT CONTACTS -------------------------

    // Method to call for completing a Quest
    void MissionComplete()
    {
        base.FinishQuestStep();
    }
}
