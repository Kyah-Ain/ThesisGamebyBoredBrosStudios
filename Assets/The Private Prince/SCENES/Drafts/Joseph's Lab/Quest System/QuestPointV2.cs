using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class QuestPointV2 : MonoBehaviour
{
    // ------------------------- ENUMS -------------------------

    public enum TriggerMode
    {
        Manual,          // Player must press interact button
        AutoOnEnter,     // Automatically triggers when player enters
        AutoWithDelay    // Automatically triggers after a delay when player enters
    }

    public enum IconDisplayMode
    {
        Auto,           // Automatically show/hide based on quest state (default)
        AlwaysShow,     // Always show the icon (useful for debugging)
        AlwaysHide,     // Never show the icon (even if QuestIcon component exists)
        ShowOnlyOnManual // Only show when in Manual trigger mode
    }

    // ------------------------- SERIALIZED FIELDS -------------------------

    [Header("Quest")]
    [SerializeField] private QuestInfoSO questInfoForPoint; // Reference to the quest this point controls

    [Header("Quest Point Type")]
    [SerializeField] private bool startPoint = true;  // Whether this point can START the quest
    [SerializeField] private bool finishPoint = true; // Whether this point can FINISH the quest

    [Header("Trigger Settings")]
    [SerializeField] private TriggerMode triggerMode = TriggerMode.Manual; // How this quest point triggers
    [SerializeField] private float autoTriggerDelay = 1f; // Delay in seconds (only used in AutoWithDelay mode)
    [SerializeField] private bool canTriggerOnlyOnce = true; // Only trigger once per quest state change

    [Header("Visual Settings")]
    [SerializeField] private IconDisplayMode iconDisplayMode = IconDisplayMode.Auto; // How the quest icon behaves

    // ------------------------- PRIVATE VARIABLES -------------------------

    private bool playerIsNear = false;  // Track if the player is within trigger range
    private string questId;             // Cached quest ID from the ScriptableObject
    private QuestState currentQuestState; // The current state of the quest (e.g., CAN_START, FINISHED)
    private QuestIcon questIcon;        // UI or visual indicator above the quest point
    private bool hasAutoTriggered = false; // Track if auto-trigger has already fired

    // ------------------------- UNITY METHODS -------------------------

    // ...
    private void Awake()
    {
        CacheQuestId();
        GetQuestIcon();
        SetupCollider();
        UpdateIconVisibility();
    }

    // ...
    private void OnEnable()
    {
        SubscribeToEvents();
        ResetAutoTriggerFlag();
    }

    // ...
    private void OnDisable()
    {
        UnsubscribeFromEvents();
    }

    // ...
    private void OnValidate()
    {
        ValidateTriggerSettings();
        ValidateIconSettings();
    }

    // ------------------------- INITIALIZATION METHODS -------------------------

    // Method to cache the quest ID from the assigned ScriptableObject
    private void CacheQuestId()
    {
        if (questInfoForPoint != null)
        {
            questId = questInfoForPoint.id;
        }
        else
        {
            Debug.LogError($"QuestPointV2 on {gameObject.name} has no QuestInfoSO assigned!");
        }
    }

    // Method to get QuestIcon component from child GameObject
    private void GetQuestIcon()
    {
        questIcon = GetComponentInChildren<QuestIcon>();
    }

    // Method to setup the sphere collider for trigger detection
    private void SetupCollider()
    {
        SphereCollider col = GetComponent<SphereCollider>();
        col.isTrigger = true;   // Must be a trigger collider for OnTriggerEnter/Exit to fire
        col.radius = 2f;        // Default interaction range
    }

    // ------------------------- EVENT SUBSCRIPTIONS -------------------------

    // Method to subscribe to quest events when enabled
    private void SubscribeToEvents()
    {
        if (GameEventsManager.Instance == null) return;
        GameEventsManager.Instance.questEvents.onQuestStateChange += QuestStateChange;
    }

    // Method to unsubscribe from quest events when disabled
    private void UnsubscribeFromEvents()
    {
        if (GameEventsManager.Instance != null)
        {
            GameEventsManager.Instance.questEvents.onQuestStateChange -= QuestStateChange;
        }
    }

    // ------------------------- QUEST STATE HANDLING -------------------------

    // Event callback when quest state changes globally
    private void QuestStateChange(Quest quest)
    {
        // Only update if this quest point is linked to the quest that changed
        if (!quest.info.id.Equals(questId)) return;

        UpdateCurrentQuestState(quest.state);
        UpdateQuestIcon();
        ResetAutoTriggerIfAllowed();
    }

    // Method to update the cached quest state
    private void UpdateCurrentQuestState(QuestState newState)
    {
        currentQuestState = newState;
    }

    // Method to update the quest icon based on current state
    private void UpdateQuestIcon()
    {
        if (questIcon == null) return;

        // Update icon state if we're in a mode that shows state
        if (ShouldUpdateIconState())
        {
            questIcon.SetState(currentQuestState, startPoint, finishPoint);
        }

        // Update visibility based on display mode
        UpdateIconVisibility();
    }

    // Method to determine if icon state should be updated
    private bool ShouldUpdateIconState()
    {
        return iconDisplayMode == IconDisplayMode.Auto ||
               iconDisplayMode == IconDisplayMode.AlwaysShow;
    }

    // Method to reset auto-trigger flag if allowed by settings
    private void ResetAutoTriggerIfAllowed()
    {
        if (!canTriggerOnlyOnce)
        {
            hasAutoTriggered = false;
        }
    }

    // ------------------------- ICON VISUAL METHODS -------------------------

    // Method to update icon visibility based on display mode
    private void UpdateIconVisibility()
    {
        if (questIcon == null) return;

        bool shouldShow = DetermineIconVisibility();

        // Set icon active state
        questIcon.gameObject.SetActive(shouldShow);

        // If visible, update its state
        if (shouldShow)
        {
            questIcon.SetState(currentQuestState, startPoint, finishPoint);
        }
    }

    // Method to determine if icon should be visible based on display mode
    private bool DetermineIconVisibility()
    {
        switch (iconDisplayMode)
        {
            case IconDisplayMode.Auto:
                return IsQuestTriggerable();

            case IconDisplayMode.AlwaysShow:
                return true;

            case IconDisplayMode.AlwaysHide:
                return false;

            case IconDisplayMode.ShowOnlyOnManual:
                return (triggerMode == TriggerMode.Manual);

            default:
                return false;
        }
    }

    // Method to check if the quest is in a triggerable state
    private bool IsQuestTriggerable()
    {
        return (currentQuestState == QuestState.CAN_START && startPoint) ||
               (currentQuestState == QuestState.CAN_FINISH && finishPoint);
    }

    // ------------------------- TRIGGER METHODS -------------------------

    // This method is called when player presses the interact button
    public void Interact()
    {
        if (triggerMode == TriggerMode.Manual && playerIsNear)
        {
            AttemptQuestTrigger();
        }
    }

    // Public method to trigger the quest point from UnityEvents or other scripts
    public void TriggerQuestPoint()
    {
        AttemptQuestTrigger();
    }

    // Overload with optional force parameter
    public void TriggerQuestPoint(bool forceTrigger)
    {
        if (forceTrigger)
        {
            AttemptQuestTrigger();
        }
        else
        {
            AttemptQuestTrigger();
        }
    }

    // Core method to attempt starting or finishing the quest
    private void AttemptQuestTrigger()
    {
        if (currentQuestState == QuestState.CAN_START && startPoint)
        {
            StartQuest();
        }
        else if (currentQuestState == QuestState.CAN_FINISH && finishPoint)
        {
            FinishQuest();
        }
        else
        {
            LogCannotTrigger();
        }
    }

    // Method to start the quest
    private void StartQuest()
    {
        GameEventsManager.Instance.questEvents.StartQuest(questId);
        Debug.Log($"QuestPointV2: Started quest {questId} via {GetTriggerType()}");
    }

    // Method to finish the quest
    private void FinishQuest()
    {
        GameEventsManager.Instance.questEvents.FinishQuest(questId);
        Debug.Log($"QuestPointV2: Finished quest {questId} via {GetTriggerType()}");
    }

    // Helper method to get trigger type for debug logs
    private string GetTriggerType()
    {
        return triggerMode == TriggerMode.Manual ? "manual" : "auto";
    }

    // Method to log when quest cannot be triggered
    private void LogCannotTrigger()
    {
        Debug.Log($"QuestPointV2: Cannot trigger quest {questId}. " +
                 $"Current state: {currentQuestState}, " +
                 $"StartPoint: {startPoint}, " +
                 $"FinishPoint: {finishPoint}");
    }

    // ------------------------- AUTO TRIGGER METHODS -------------------------

    // Coroutine for delayed auto-trigger
    private System.Collections.IEnumerator DelayedAutoTrigger()
    {
        yield return new WaitForSeconds(autoTriggerDelay);

        // Only trigger if player is still nearby and we haven't triggered yet
        if (playerIsNear && !hasAutoTriggered)
        {
            hasAutoTriggered = true;
            AttemptQuestTrigger();
        }
    }

    // Method to reset the auto-trigger flag
    private void ResetAutoTriggerFlag()
    {
        hasAutoTriggered = false;
    }

    // Public method to manually reset auto-trigger (useful for debugging or special cases)
    public void ResetAutoTrigger()
    {
        hasAutoTriggered = false;
    }

    // ------------------------- COLLIDER METHODS -------------------------

    // Detect player entering trigger zone
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerIsNear = true;
        HandleAutoTriggerOnEnter();
    }

    // Handle auto-trigger logic when player enters
    private void HandleAutoTriggerOnEnter()
    {
        if (triggerMode == TriggerMode.Manual) return;
        if (hasAutoTriggered) return;
        if (!IsQuestTriggerable()) return;

        switch (triggerMode)
        {
            case TriggerMode.AutoOnEnter:
                TriggerImmediate();
                break;

            case TriggerMode.AutoWithDelay:
                TriggerWithDelay();
                break;
        }
    }

    // Trigger quest immediately
    private void TriggerImmediate()
    {
        hasAutoTriggered = true;
        AttemptQuestTrigger();
    }

    // Trigger quest with delay
    private void TriggerWithDelay()
    {
        StartCoroutine(DelayedAutoTrigger());
    }

    // Detect player exiting trigger zone
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerIsNear = false;
        }
    }

    // ------------------------- VALIDATION METHODS -------------------------

    // Validate trigger settings in the inspector
    private void ValidateTriggerSettings()
    {
        // Clamp delay to reasonable values
        autoTriggerDelay = Mathf.Max(0f, autoTriggerDelay);

        // Warn if manual mode has delay configured
        if (triggerMode == TriggerMode.Manual && autoTriggerDelay > 0f)
        {
            Debug.LogWarning($"QuestPointV2 on {gameObject.name}: autoTriggerDelay is only used in AutoWithDelay mode.", this);
        }

        // Warn if AutoWithDelay mode has zero delay
        if (triggerMode == TriggerMode.AutoWithDelay && autoTriggerDelay <= 0f)
        {
            Debug.LogWarning($"QuestPointV2 on {gameObject.name}: AutoWithDelay mode selected but delay is 0. Consider using AutoOnEnter mode.", this);
        }
    }

    // Validate icon settings in the inspector
    private void ValidateIconSettings()
    {
        if (iconDisplayMode == IconDisplayMode.ShowOnlyOnManual && triggerMode != TriggerMode.Manual)
        {
            Debug.LogWarning($"QuestPointV2 on {gameObject.name}: IconDisplayMode is 'ShowOnlyOnManual' but trigger mode is {triggerMode}. Icon will not appear!", this);
        }
    }
}