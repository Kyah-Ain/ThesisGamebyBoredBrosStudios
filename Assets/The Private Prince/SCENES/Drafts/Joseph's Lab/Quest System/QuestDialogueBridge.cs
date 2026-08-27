using UnityEngine;

public class QuestDialogueBridge : MonoBehaviour
{
    // -------------------- STRING BASED METHODS -------------------------

    public void StartQuestByString(string questId)
    {
        Debug.Log($"QuestDialogueBridge: Starting quest {questId}");
        // Debug.Log($"QuestManager.Instance = {QuestManager.Instance}");
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

    public void AdvanceQuestByString(string questId)
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

    public void FinishQuestByString(string questId)
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

    // -------------------- SCRIPTABLE OBJ BASED METHODS -------------------------
    // NOTE - QuestInfo Parameter Methods (Method Overloading)
    // Overload Method - are for cases where you want the code extract the questId for you (No Typing Needed)

    public void StartQuestByIdSO(QuestInfoSO questId)
    {
        Debug.Log($"QuestDialogueBridge: Starting quest {questId}");
        // Debug.Log($"QuestManager.Instance = {QuestManager.Instance}");
        Debug.Log($"GameEventsManager.Instance = {GameEventsManager.Instance}");

        if (GameEventsManager.Instance != null)
        {
            GameEventsManager.Instance.questEvents.StartQuest(questId.id);
            Debug.Log($"Dialogue triggered: Started quest {questId}");
        }
        else
        {
            Debug.LogError("GameEventsManager instance not found!");
        }
    }

    public void AdvanceQuestByIdSO(QuestInfoSO questId)
    {
        Debug.Log($"=== QUEST DIALOGUE BRIDGE DEBUG ===");
        Debug.Log($"QuestDialogueBridge: Advancing quest {questId.id}");

        if (GameEventsManager.Instance != null)
        {
            Debug.Log($"GameEventsManager found, calling questEvents.AdvanceQuest");
            GameEventsManager.Instance.questEvents.AdvanceQuest(questId.id);
            Debug.Log($"Successfully called AdvanceQuest for {questId.id}");
        }
        else
        {
            Debug.LogError("GameEventsManager instance not found!");
        }
        Debug.Log($"=== END DIALOGUE BRIDGE DEBUG ===");
    }

    public void FinishQuestByIdSO(QuestInfoSO questId)
    {
        if (GameEventsManager.Instance != null)
        {
            GameEventsManager.Instance.questEvents.FinishQuest(questId.id);
            Debug.Log($"Dialogue triggered: Finished quest {questId.id}");
        }
        else
        {
            Debug.LogError("GameEventsManager instance not found!");
        }
    }

    // ------------------------- OBJECT METHODS -------------------------

    // AIN'S CUSTOM METHODS HERE =========================================================

    // Method that destroys the specified trigger object when aquired or activated
    public void DestroyTriggeredObject(GameObject triggerObjects)
    {
        Destroy(triggerObjects); // Destroy the trigger object that was interacted with
    }
}