using System;
using UnityEngine;

public class QuestEvents
{
    public event Action<string> onStartQuest;

    public void StartQuest(string id)
    {
        if (onStartQuest != null)
        {
            onStartQuest(id);
        }
    }

    public event Action<string> onAdvanceQuest;

    public void AdvanceQuest(string id)
    {
        Debug.Log($"=== QUEST EVENTS DEBUG ===");
        Debug.Log($"QuestEvents: Advancing quest {id}");

        if (onAdvanceQuest != null)
        {
            Debug.Log($"Invoking onAdvanceQuest event for {id}");
            onAdvanceQuest(id);
            Debug.Log($"Event invoked successfully");
        }
        else
        {
            Debug.LogWarning($"No subscribers to onAdvanceQuest event for {id}");
        }
        Debug.Log($"=== END QUEST EVENTS DEBUG ===");
    }

    public event Action<string> onFinishQuest;

    public void FinishQuest(string id)
    {
        if (onFinishQuest != null)
        {
            onFinishQuest(id);
        }
    }

    public event Action<Quest> onQuestStateChange;

    public void QuestStateChange(Quest quest)
    {
        if (onQuestStateChange != null)
        {
            onQuestStateChange(quest);
        }
    }

    public event Action<string, int, QuestStepState > onQuestStepStateChange;

    public void QuestStepStateChange(string id, int stepIndex, QuestStepState questStateStep)
    {
        if (onQuestStepStateChange != null)
        {
            onQuestStepStateChange(id, stepIndex, questStateStep);
        }
    }



}
