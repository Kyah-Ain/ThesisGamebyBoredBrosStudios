using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Small helper to call the global quest start from an inspector event
public class QuestStarter : MonoBehaviour
{
    // Public method so UnityEvent can call it with a string parameter
    public void StartQuestById(string questId)
    {
        // Defensive check to avoid null references
        if (GameEventsManager.Instance == null)
        {
            Debug.LogError("GameEventsManager.Instance is null. Ensure GameEventsManager exists in the scene.");
            return;
        }

        // Fire the global quest start event
        GameEventsManager.Instance.questEvents.StartQuest(questId);
    }
}

