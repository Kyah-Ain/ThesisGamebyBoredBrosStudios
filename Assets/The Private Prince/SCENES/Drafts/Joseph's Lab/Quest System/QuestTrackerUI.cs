using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class QuestTrackerUI : MonoBehaviour
{
    [SerializeField] private TMP_Text questText;
    private Dictionary<string, string> activeQuests = new Dictionary<string, string>();
    private bool isSubscribed = false;

    private void Start()
    {
        StartCoroutine(InitializeAfterFrame());
    }

    private System.Collections.IEnumerator InitializeAfterFrame()
    {
        yield return null;

        // Wait for GameEventsManager
        int maxWait = 10;
        while (GameEventsManager.Instance == null && maxWait > 0)
        {
            yield return new WaitForSeconds(0.1f);
            maxWait--;
        }

        if (GameEventsManager.Instance != null)
        {
            GameEventsManager.Instance.questEvents.onStartQuest += OnQuestStarted;
            GameEventsManager.Instance.questEvents.onFinishQuest += OnQuestFinished;
            isSubscribed = true;
            Debug.Log("QuestTrackerUI: Successfully subscribed to events");
        }
        else
        {
            Debug.LogError("QuestTrackerUI: GameEventsManager not found after waiting");
        }

        UpdateText();
    }

    private void OnDisable()
    {
        if (isSubscribed && GameEventsManager.Instance != null)
        {
            GameEventsManager.Instance.questEvents.onStartQuest -= OnQuestStarted;
            GameEventsManager.Instance.questEvents.onFinishQuest -= OnQuestFinished;
            isSubscribed = false;
        }
    }

    private void OnQuestStarted(string questId)
    {
        Debug.Log($"QuestTrackerUI: Quest started - {questId}");

        if (QuestManager.Instance == null)
        {
            Debug.LogError("QuestTrackerUI: QuestManager.Instance is null");
            return;
        }

        Quest quest = QuestManager.Instance.GetQuestById(questId);
        if (quest != null)
        {
            activeQuests[questId] = quest.info.displayName;
            UpdateText();
        }
        else
        {
            Debug.LogError($"QuestTrackerUI: Could not find quest with id {questId}");
        }
    }

    private void OnQuestFinished(string questId)
    {
        Debug.Log($"QuestTrackerUI: Quest finished - {questId}");

        if (activeQuests.ContainsKey(questId))
        {
            activeQuests.Remove(questId);
            UpdateText();
        }
    }

    private void UpdateText()
    {
        if (questText == null)
        {
            Debug.LogError("QuestTrackerUI: questText is not assigned!");
            return;
        }

        if (activeQuests.Count == 0)
        {
            questText.text = "No active quests";
            Debug.Log("QuestTrackerUI: Displaying 'No active quests'");
        }
        else
        {
            string outText = "Active Quests:\n";
            foreach (var quest in activeQuests.Values)
            {
                outText += "- " + quest + "\n";
            }
            questText.text = outText;
            Debug.Log($"QuestTrackerUI: Displaying {activeQuests.Count} quest(s)");
        }
    }
}