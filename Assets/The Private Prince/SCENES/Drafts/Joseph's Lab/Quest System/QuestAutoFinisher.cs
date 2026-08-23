using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Events;

/// <summary>
/// Automatically finishes a quest as soon as all QuestSteps are completed
/// (i.e., when the quest reaches CAN_FINISH state), without requiring
/// player interaction at a QuestPoint.
/// </summary>
public class QuestAutoFinisher : MonoBehaviour
{
    // ---------------------------- VARIABLES -------------------------

    [Header("QUEST")]
    [SerializeField] private QuestInfoSO questInfo; // Reference to the quest this point controls

    private string questId; // Placeholder for QuestId of a specific Quest

    [Header("OPTIONAL EVENTS")]
    public GameEvent onQuestFinishGlobal; // Event that was being sent out when a Quest Finishes (Scriptable Based)
    [Space]
    public UnityEvent onQuestFinishLocal; // Event that was being sent out when a Quest Finishes

    // -------------------------- UNITY METHODS -------------------------

    // Built-In Unity method that automatically called 1st 
    public void Awake() 
    {
        if (questInfo == null)
        {
            Debug.LogError($"QuestAutoFinisher on {gameObject.name}: No QuestInfoSO assigned!");
            return;
        }
        questId = questInfo.id;
    }

    // Built-In Unity method that automatically called 2nd (when Active) 
    private void OnEnable()
    {
        // Makes the OnQuestStateChange method to listens for the Quest State changes (for Auto Trigger Purposes)
        if (GameEventsManager.Instance != null)
        {
            GameEventsManager.Instance.questEvents.onQuestStateChange += OnQuestStateChange;
        }
    }

    // Built-In Unity method that automatically called 2nd (when Inactive) 
    private void OnDisable()
    {
        // Makes the OnQuestStateChange method to unlistens for the Quest State changes (for Saving Memory Pusposes)
        if (GameEventsManager.Instance != null)
        {
            GameEventsManager.Instance.questEvents.onQuestStateChange -= OnQuestStateChange;
        }
    }

    // -------------------------- QUEST METHODS -------------------------

    // Method that lets this script change a state of a Quest
    private void OnQuestStateChange(Ain.Quest quest) 
    {
        // Only proceeds if the Quest Update was meant for this Quest Id
        if (!quest.info.id.Equals(questId)) return;

        // Only proceeds if the Quest State was up to be Finished (All Quest Steps were fulfilled)
        if (quest.state == QuestState.CAN_FINISH) 
        {
            Debug.Log($"QuestAutoFinisher: Quest '{questId}' is CAN_FINISH � auto-finishing.");
            GameEventsManager.Instance.questEvents.FinishQuest(questId);

            // Invokes/Trigger an event for anyone who want to subscribe for when this quest ends
            onQuestFinishLocal?.Invoke();

            // Invokes/Trigger a custom Event that script based subscription
            onQuestFinishGlobal?.TriggerEvent();
        }
    }
}