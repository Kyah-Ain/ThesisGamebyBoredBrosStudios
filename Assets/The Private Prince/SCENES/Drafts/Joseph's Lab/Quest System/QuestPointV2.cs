using UnityEngine;

//[RequireComponent(typeof(SphereCollider))]
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

    private void Awake()
    {
        Debug.Log($"QuestPointV2: Awake called on {gameObject.name}");
        CacheQuestId();
        GetQuestIcon();
        SetupCollider();
        UpdateIconVisibility();
        Debug.Log($"QuestPointV2: Initialization complete. Quest ID: {questId}, StartPoint: {startPoint}, FinishPoint: {finishPoint}, TriggerMode: {triggerMode}");
    }

    private void OnEnable()
    {
        Debug.Log($"QuestPointV2: OnEnable called on {gameObject.name}");
        SubscribeToEvents();
        ResetAutoTriggerFlag();
    }

    private void OnDisable()
    {
        Debug.Log($"QuestPointV2: OnDisable called on {gameObject.name}");
        UnsubscribeFromEvents();
    }

    private void OnValidate()
    {
        ValidateTriggerSettings();
        ValidateIconSettings();
    }

    // ------------------------- INITIALIZATION METHODS -------------------------

    private void CacheQuestId()
    {
        if (questInfoForPoint != null)
        {
            questId = questInfoForPoint.id;
            Debug.Log($"QuestPointV2: Cached quest ID: {questId} from {questInfoForPoint.name}");
        }
        else
        {
            Debug.LogError($"QuestPointV2: ERROR: QuestPointV2 on {gameObject.name} has no QuestInfoSO assigned!");
        }
    }

    private void GetQuestIcon()
    {
        questIcon = GetComponentInChildren<QuestIcon>();
        if (questIcon != null)
        {
            Debug.Log($"QuestPointV2: Found QuestIcon component in children of {gameObject.name}");
        }
        else
        {
            Debug.Log($"QuestPointV2: No QuestIcon component found in children of {gameObject.name} (this is optional)");
        }
    }

    private void SetupCollider()
    {
        SphereCollider col = GetComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius = 2f;
        Debug.Log($"QuestPointV2: Setup SphereCollider on {gameObject.name}: isTrigger={col.isTrigger}, radius={col.radius}");
    }

    // ------------------------- EVENT SUBSCRIPTIONS -------------------------

    private void SubscribeToEvents()
    {
        if (GameEventsManager.Instance == null)
        {
            Debug.LogWarning($"QuestPointV2: GameEventsManager.Instance is null on {gameObject.name}. Make sure GameEventsManager exists in the scene!");
            return;
        }

        GameEventsManager.Instance.questEvents.onQuestStateChange += QuestStateChange;
        Debug.Log($"QuestPointV2: Subscribed to quest state change events for quest {questId}");
    }

    private void UnsubscribeFromEvents()
    {
        if (GameEventsManager.Instance != null)
        {
            GameEventsManager.Instance.questEvents.onQuestStateChange -= QuestStateChange;
            Debug.Log($"QuestPointV2: Unsubscribed from quest state change events for quest {questId}");
        }
    }

    // ------------------------- QUEST STATE HANDLING -------------------------

    private void QuestStateChange(Quest quest)
    {
        Debug.Log($"QuestPointV2: QuestStateChange event received. Quest ID: {quest.info.id}, My Quest ID: {questId}");

        if (!quest.info.id.Equals(questId))
        {
            Debug.Log($"QuestPointV2: Quest ID mismatch, ignoring event");
            return;
        }

        Debug.Log($"QuestPointV2: Updating quest state for {questId}: {quest.state}");
        UpdateCurrentQuestState(quest.state);
        UpdateQuestIcon();
        ResetAutoTriggerIfAllowed();

        Debug.Log($"QuestPointV2: Current state after update: {currentQuestState}, CanTrigger: {IsQuestTriggerable()}");
    }

    private void UpdateCurrentQuestState(QuestState newState)
    {
        Debug.Log($"QuestPointV2: Updating currentQuestState from {currentQuestState} to {newState}");
        currentQuestState = newState;
    }

    private void UpdateQuestIcon()
    {
        if (questIcon == null)
        {
            Debug.Log($"QuestPointV2: No QuestIcon to update");
            return;
        }

        if (ShouldUpdateIconState())
        {
            Debug.Log($"QuestPointV2: Updating QuestIcon state to {currentQuestState}");
            questIcon.SetState(currentQuestState, startPoint, finishPoint);
        }

        UpdateIconVisibility();
    }

    private bool ShouldUpdateIconState()
    {
        bool shouldUpdate = iconDisplayMode == IconDisplayMode.Auto || iconDisplayMode == IconDisplayMode.AlwaysShow;
        Debug.Log($"QuestPointV2: ShouldUpdateIconState: {shouldUpdate} (Mode: {iconDisplayMode})");
        return shouldUpdate;
    }

    private void ResetAutoTriggerIfAllowed()
    {
        if (!canTriggerOnlyOnce)
        {
            hasAutoTriggered = false;
            Debug.Log($"QuestPointV2: Reset auto-trigger flag (canTriggerOnlyOnce=false)");
        }
        else
        {
            Debug.Log($"QuestPointV2: Cannot reset auto-trigger (canTriggerOnlyOnce=true)");
        }
    }

    // ------------------------- ICON VISUAL METHODS -------------------------

    private void UpdateIconVisibility()
    {
        if (questIcon == null) return;

        bool shouldShow = DetermineIconVisibility();
        Debug.Log($"QuestPointV2: UpdateIconVisibility: shouldShow={shouldShow} (Mode: {iconDisplayMode})");

        questIcon.gameObject.SetActive(shouldShow);

        if (shouldShow)
        {
            questIcon.SetState(currentQuestState, startPoint, finishPoint);
        }
    }

    private bool DetermineIconVisibility()
    {
        switch (iconDisplayMode)
        {
            case IconDisplayMode.Auto:
                bool isTriggerable = IsQuestTriggerable();
                Debug.Log($"QuestPointV2: Auto mode: IsQuestTriggerable={isTriggerable}");
                return isTriggerable;

            case IconDisplayMode.AlwaysShow:
                Debug.Log($"QuestPointV2: AlwaysShow mode: returning true");
                return true;

            case IconDisplayMode.AlwaysHide:
                Debug.Log($"QuestPointV2: AlwaysHide mode: returning false");
                return false;

            case IconDisplayMode.ShowOnlyOnManual:
                bool isManual = (triggerMode == TriggerMode.Manual);
                Debug.Log($"QuestPointV2: ShowOnlyOnManual mode: isManual={isManual}");
                return isManual;

            default:
                return false;
        }
    }

    private bool IsQuestTriggerable()
    {
        bool canStart = (currentQuestState == QuestState.CAN_START && startPoint);
        bool canFinish = (currentQuestState == QuestState.CAN_FINISH && finishPoint);
        bool isTriggerable = canStart || canFinish;

        Debug.Log($"QuestPointV2: IsQuestTriggerable check - State: {currentQuestState}, StartPoint: {startPoint}, FinishPoint: {finishPoint}, CanStart: {canStart}, CanFinish: {canFinish}, Result: {isTriggerable}");

        return isTriggerable;
    }

    // ------------------------- TRIGGER METHODS -------------------------

    // ...
    public void Interact()
    {
        Debug.Log($"QuestPointV2 Interact() has been called. TriggerMode: {triggerMode}, PlayerIsNear: {playerIsNear}");

        if (triggerMode == TriggerMode.Manual && playerIsNear)
        {
            Debug.Log($"QuestPointV2: Manual trigger condition met, attempting quest trigger");
            AttemptQuestTrigger();
        }
        else
        {
            Debug.Log($"QuestPointV2: Cannot trigger manually - Mode: {triggerMode}, PlayerNear: {playerIsNear}");
        }
    }

    // Method to call a Quest Progress and checks if either Startable or Finishable, then executes it
    public void AttemptQuestTrigger()
    {
        ForceStartQuest();
        ForceFinishQuest();
    }

    // Method to call if you want to forcefully 'Start' a Quest
    public void ForceStartQuest()
    {
        Debug.Log($"QuestPointV2: ForceStartQuest() has been called with values; \n" +
                  $"Current State: {currentQuestState}, " +
                  $"StartPoint: {startPoint}, " +
                  $"FinishPoint: {finishPoint}");

        // Checks if the Quest's State trying to 'Start' was startable,
        // and that the Player triggers it from the Starting Point (Eg. Quest Giver, Quest Location))
        if (currentQuestState == QuestState.CAN_START && startPoint)
        {
            Debug.Log($"QuestPointV2: Conditions met for STARTING quest");
            StartQuest();
        }
        else
        {
            LogCannotTrigger();
        }
    }

    // Method to call if you want to forcefully 'Finish' a Quest
    public void ForceFinishQuest()
    {
        Debug.Log($"QuestPointV2: ForceFinishQuest() has been called with values; \n" +
                  $"Current State: {currentQuestState}, " +
                  $"StartPoint: {startPoint}, " +
                  $"FinishPoint: {finishPoint}");

        // Checks if the Quest's State trying to 'Finish' was endable,
        // and that the Player triggers it from the Starting Point (Eg. Quest Giver, Quest Location))
        if (currentQuestState == QuestState.CAN_FINISH && finishPoint)
        {
            Debug.Log($"QuestPointV2: Conditions met for FINISHING quest");
            FinishQuest();
        }
        else
        {
            LogCannotTrigger();
        }
    }

    private void StartQuest()
    {
        if (GameEventsManager.Instance == null)
        {
            Debug.LogError($"QuestPointV2: Cannot start quest - GameEventsManager.Instance is null!");
            return;
        }

        Debug.Log($"QuestPointV2: ===== STARTING QUEST {questId} =====");
        GameEventsManager.Instance.questEvents.StartQuest(questId);
        Debug.Log($"QuestPointV2: Quest start event sent via {GetTriggerType()}");
    }

    private void FinishQuest()
    {
        if (GameEventsManager.Instance == null)
        {
            Debug.LogError($"QuestPointV2: Cannot finish quest - GameEventsManager.Instance is null!");
            return;
        }

        Debug.Log($"QuestPointV2: ===== FINISHING QUEST {questId} =====");
        GameEventsManager.Instance.questEvents.FinishQuest(questId);
        Debug.Log($"QuestPointV2: Quest finish event sent via {GetTriggerType()}");
    }

    private string GetTriggerType()
    {
        return triggerMode == TriggerMode.Manual ? "manual" : "auto";
    }

    private void LogCannotTrigger()
    {
        Debug.LogWarning($"QuestPointV2: CANNOT TRIGGER QUEST {questId} \n" +
                        $"  Current state: {currentQuestState}\n" +
                        $"  StartPoint: {startPoint}\n" +
                        $"  FinishPoint: {finishPoint}\n" +
                        $"  Required conditions:\n" +
                        $"    - To START: State must be CAN_START AND startPoint=true\n" +
                        $"    - To FINISH: State must be CAN_FINISH AND finishPoint=true");
    }

    // ------------------------- AUTO TRIGGER METHODS -------------------------

    private System.Collections.IEnumerator DelayedAutoTrigger()
    {
        Debug.Log($"QuestPointV2: Starting delayed auto-trigger coroutine with delay {autoTriggerDelay}s");
        yield return new WaitForSeconds(autoTriggerDelay);

        if (playerIsNear && !hasAutoTriggered)
        {
            Debug.Log($"QuestPointV2: Delay completed, player still near, triggering quest");
            hasAutoTriggered = true;
            AttemptQuestTrigger();
        }
        else
        {
            Debug.Log($"QuestPointV2: Delay completed but conditions not met - PlayerNear: {playerIsNear}, HasTriggered: {hasAutoTriggered}");
        }
    }

    private void ResetAutoTriggerFlag()
    {
        hasAutoTriggered = false;
        Debug.Log($"QuestPointV2: Reset auto-trigger flag to false");
    }

    public void ResetAutoTrigger()
    {
        hasAutoTriggered = false;
        Debug.Log($"QuestPointV2: Auto-trigger manually reset");
    }

    // ------------------------- COLLIDER METHODS -------------------------

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"QuestPointV2: OnTriggerEnter with {other.gameObject.name} (Tag: {other.tag})");

        if (!other.CompareTag("Player"))
        {
            Debug.Log($"QuestPointV2: Ignoring non-player collision");
            return;
        }

        playerIsNear = true;
        Debug.Log($"QuestPointV2: Player entered trigger zone! PlayerNear = true");
        HandleAutoTriggerOnEnter();
    }

    private void HandleAutoTriggerOnEnter()
    {
        Debug.Log($"QuestPointV2: HandleAutoTriggerOnEnter - Mode: {triggerMode}, HasAutoTriggered: {hasAutoTriggered}");

        if (triggerMode == TriggerMode.Manual)
        {
            Debug.Log($"QuestPointV2: Manual mode, no auto-trigger");
            return;
        }

        if (hasAutoTriggered)
        {
            Debug.Log($"QuestPointV2: Already auto-triggered, skipping");
            return;
        }

        if (!IsQuestTriggerable())
        {
            Debug.Log($"QuestPointV2: Quest not triggerable in current state");
            return;
        }

        switch (triggerMode)
        {
            case TriggerMode.AutoOnEnter:
                Debug.Log($"QuestPointV2: AutoOnEnter mode - triggering immediately");
                TriggerImmediate();
                break;

            case TriggerMode.AutoWithDelay:
                Debug.Log($"QuestPointV2: AutoWithDelay mode - starting delayed trigger");
                TriggerWithDelay();
                break;
        }
    }

    private void TriggerImmediate()
    {
        hasAutoTriggered = true;
        Debug.Log($"QuestPointV2: Immediate trigger activated");
        AttemptQuestTrigger();
    }

    private void TriggerWithDelay()
    {
        Debug.Log($"QuestPointV2: Starting coroutine for delayed trigger");
        StartCoroutine(DelayedAutoTrigger());
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerIsNear = false;
            Debug.Log($"QuestPointV2: Player exited trigger zone. PlayerNear = false");
        }
    }

    // ------------------------- VALIDATION METHODS -------------------------

    private void ValidateTriggerSettings()
    {
        autoTriggerDelay = Mathf.Max(0f, autoTriggerDelay);

        if (triggerMode == TriggerMode.Manual && autoTriggerDelay > 0f)
        {
            Debug.LogWarning($"QuestPointV2: Warning on {gameObject.name}: autoTriggerDelay is only used in AutoWithDelay mode.", this);
        }

        if (triggerMode == TriggerMode.AutoWithDelay && autoTriggerDelay <= 0f)
        {
            Debug.LogWarning($"QuestPointV2: Warning on {gameObject.name}: AutoWithDelay mode selected but delay is 0. Consider using AutoOnEnter mode.", this);
        }
    }

    private void ValidateIconSettings()
    {
        if (iconDisplayMode == IconDisplayMode.ShowOnlyOnManual && triggerMode != TriggerMode.Manual)
        {
            Debug.LogWarning($"QuestPointV2: Warning on {gameObject.name}: IconDisplayMode is 'ShowOnlyOnManual' but trigger mode is {triggerMode}. Icon will not appear!", this);
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