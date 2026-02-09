using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class QuestStep_TalkToNPC : QuestStep
{
    protected override void SetQuestStepState(string state)
    {
        // No special state needed for talk quests
    }

    // Ain's wondering why doesnt we call "base.FinishQuestStep()" here...?
}