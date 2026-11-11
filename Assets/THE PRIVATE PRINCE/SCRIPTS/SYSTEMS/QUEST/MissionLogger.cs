using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionLogger : MonoBehaviour
{
    // ------------------------- VARIABLES -------------------------

    [Header("REFERENCE")]
    [SerializeField] GameEventTrigger eventTrigger;
    [SerializeField] QuestInfoSO questInfoSO;

    [Header("SETTINGS")]
    [SerializeField] bool showStepProgress = true;

    // Public property so other scripts can flip it at runtime (logic lives in SetShowStepProgress)
    public bool ShowStepProgress
    {
        get => showStepProgress;
        set => SetShowStepProgress(value);
    }

    // Retrieves the Quest's data using its Quest Id
    private Ain.Quest QuestData => Ain.QuestManager.Instance.GetQuestById(questInfoSO.id);

    // ------------------------- UNITY METHODS -------------------------

    void OnEnable()
    {
        GameEventsManager.Instance.questEvents.onQuestStepProgress += OnQuestStepProgress;
    }

    void OnDisable()
    {
        GameEventsManager.Instance.questEvents.onQuestStepProgress -= OnQuestStepProgress;
    }

    // Lets you tick the checkbox in the Inspector during Play Mode and see it update live
    void OnValidate()
    {
        if (Application.isPlaying && isActiveAndEnabled) UpdateQuestLog();
    }

    // ------------------------- TOGGLE METHODS -------------------------

    // For a UI Toggle (shows up under "Dynamic bool" in the UnityEvent dropdown)
    public void SetShowStepProgress(bool value)
    {
        if (showStepProgress == value) return;
        showStepProgress = value;
        UpdateQuestLog(); // Refresh immediately so the change is visible right away
    }

    // For a Button or any event that has no bool parameter
    public void ToggleShowStepProgress()
    {
        SetShowStepProgress(!showStepProgress);
    }

    // ------------------------- LOG METHODS -------------------------

    public void UpdateQuestLog()
    {
        Ain.Quest quest = QuestData;

        if (quest == null) return;

        // Prevents a finished quest on repopulating the log
        if (quest.state == QuestState.FINISHED) return;

        string text = $"{questInfoSO.questName}";

        Ain.QuestStep step = quest.CurrentStep;

        // Only append progress when the toggle is on
        if (showStepProgress && step != null && step.HasProgress)
        {
            text += $"\n{step.CurrentCount}/{step.TargetCount}";
        }

        eventTrigger.ExecuteEvents(text);
    }

    public void ResetQuestLog()
    {
        eventTrigger.ExecuteEvents("");
    }

    // --------------------------- HELPERS --------------------------

    void OnQuestStepProgress(string id)
    {
        if (id == questInfoSO.id) UpdateQuestLog();
    }
}