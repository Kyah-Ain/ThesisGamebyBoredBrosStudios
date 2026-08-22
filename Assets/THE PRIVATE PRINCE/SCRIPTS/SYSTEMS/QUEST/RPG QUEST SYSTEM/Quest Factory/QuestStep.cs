using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class QuestStep : MonoBehaviour
{
    // ------------------------- VARIABLES -------------------------

    private bool isFinished = false; // Indicates if the quest step is finished/completed

    private string questId; // The unique identifier for the quest this step belongs to

    private int stepIndex; // The index of this step within the quest

    // --------------------------- SETTER -------------------------

    public void InitializeQuestStep(string questId, int stepIndex, string questStepState)
    {
        this.questId = questId;
        this.stepIndex = stepIndex;
        if (questStepState != null && questStepState != "")
        {
            SetQuestStepState(questStepState);
        }
    }

    // ---------------------- INHERITABLE METHODS -------------------------

    protected void FinishQuestStep()
    {
        isFinished = true;

        // Script Based Event call (Optional)
        // GameEventsManager.Instance.questEvents.AdvanceQuest(questId);

        Destroy(this.gameObject);
    }

    protected void ChangeState(string newState)
    {
        // Script Based Event call (Optional)
        GameEventsManager.Instance.questEvents.QuestStepStateChange(questId, stepIndex, new QuestStepState(newState));
    }

    protected abstract void SetQuestStepState(string state);
}
