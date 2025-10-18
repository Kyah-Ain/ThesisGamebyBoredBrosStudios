using UnityEngine;

[RequireComponent(typeof(SphereCollider))] // Ensures a 3D collider is always attached
public class QuestPoint : MonoBehaviour
{
    [Header("Quest")]
    [SerializeField] private QuestInfoSO questInfoForPoint; // Reference to the quest this point controls

    [Header("Config")]
    [SerializeField] private bool startPoint = true;  // Whether this point starts the quest
    [SerializeField] private bool finishPoint = true; // Whether this point can finish the quest

    private bool playerIsNear = false;  // Track if the player is within trigger range
    private string questId;             // Cached quest ID
    private QuestState currentQuestState; // The current state of the quest (e.g., CAN_START, FINISHED)
    private QuestIcon questIcon;        // UI or visual indicator above the quest point

    private void Awake()
    {
        // Cache quest ID from the assigned ScriptableObject
        questId = questInfoForPoint.id;

        // Get QuestIcon component from child GameObject (e.g., floating icon above NPC)
        questIcon = GetComponentInChildren<QuestIcon>();

        // Setup collider automatically for 3D detection
        SphereCollider col = GetComponent<SphereCollider>();
        col.isTrigger = true;   // Must be a trigger collider for OnTriggerEnter/Exit to fire
        col.radius = 2f;        // Default interaction range
    }

    private void OnEnable()
    {
        // Null check to prevent errors during initialization
        if (GameEventsManager.Instance == null) return;

        // Listen for quest state changes globally
        GameEventsManager.Instance.questEvents.onQuestStateChange += QuestStateChange;
    }

    private void OnDisable()
    {
        // Unsubscribe to prevent memory leaks
        GameEventsManager.Instance.questEvents.onQuestStateChange -= QuestStateChange;
    }

    private void QuestStateChange(Quest quest)
    {
        // Only update the quest state if this point is linked to the same quest
        if (quest.info.id.Equals(questId))
        {
            currentQuestState = quest.state;
            questIcon.SetState(currentQuestState, startPoint, finishPoint);

            if (questIcon != null)
            {
                questIcon.SetState(currentQuestState, startPoint, finishPoint);
            }
            else
            {
                Debug.LogWarning($"QuestIcon not found on QuestPoint for quest: {questId}");
            }
        }
    }

    // This method is called when player presses a key (e.g., "E") to interact
    public void Interact()
    {
        if (!playerIsNear) return;

        // Start or finish the quest depending on state and point type
        if (currentQuestState == QuestState.CAN_START && startPoint)
        {
            GameEventsManager.Instance.questEvents.StartQuest(questId);
        }
        else if (currentQuestState == QuestState.CAN_FINISH && finishPoint)
        {
            GameEventsManager.Instance.questEvents.FinishQuest(questId);
        }
    }

    // Detect player entering trigger zone
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerIsNear = true;
        }
    }

    // Detect player exiting trigger zone
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerIsNear = false;
        }
    }
}
