using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Ain;

public class FinishDialogueQuestStep : Ain.QuestStep
{
    // ----------------------- PARENT CONTACTS -------------------------

    // Method to call for completing a Quest
    public void MissionComplete()
    {
        base.FinishQuestStep();
    }
}