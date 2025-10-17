using UnityEngine;

public class QuestDialogueBridge : MonoBehaviour
{
    public void StartQuest(string questId)
    {
        Debug.Log($"QuestDialogueBridge: Starting quest {questId}");
        Debug.Log($"QuestManager.Instance = {QuestManager.Instance}");
        Debug.Log($"GameEventsManager.Instance = {GameEventsManager.Instance}");

        if (GameEventsManager.Instance != null)
        {
            GameEventsManager.Instance.questEvents.StartQuest(questId);
            Debug.Log($"Dialogue triggered: Started quest {questId}");
        }
        else
        {
            Debug.LogError("GameEventsManager instance not found!");
        }
    }

    public void AdvanceQuest(string questId)
    {
        Debug.Log($"=== QUEST DIALOGUE BRIDGE DEBUG ===");
        Debug.Log($"QuestDialogueBridge: Advancing quest {questId}");

        if (GameEventsManager.Instance != null)
        {
            Debug.Log($"GameEventsManager found, calling questEvents.AdvanceQuest");
            GameEventsManager.Instance.questEvents.AdvanceQuest(questId);
            Debug.Log($"Successfully called AdvanceQuest for {questId}");
        }
        else
        {
            Debug.LogError("GameEventsManager instance not found!");
        }
        Debug.Log($"=== END DIALOGUE BRIDGE DEBUG ===");
    }

    public void FinishQuest(string questId)
    {
        if (GameEventsManager.Instance != null)
        {
            GameEventsManager.Instance.questEvents.FinishQuest(questId);
            Debug.Log($"Dialogue triggered: Finished quest {questId}");
        }
        else
        {
            Debug.LogError("GameEventsManager instance not found!");
        }
    }
}
