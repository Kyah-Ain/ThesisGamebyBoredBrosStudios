using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestStep_TalkToNPC : QuestStep
{
    protected override void SetQuestStepState(string state)
    {
        // No special state needed for talk quests
        // We just use this to fulfill the requirement of the abstract method in the base class
    }
}