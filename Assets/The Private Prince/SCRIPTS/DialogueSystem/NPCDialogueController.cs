using UnityEngine;

public class NPCDialogueController : MonoBehaviour
{
    [System.Serializable]
    public class DialogueState
    {
        public QuestState questState;
        public int minStepIndex = -1;
        public int maxStepIndex = -1;
        public DialogueObject dialogue;
    }

    [SerializeField] private string questId;
    [SerializeField] private DialogueState[] dialogueStates;
    [SerializeField] private DialogueObject defaultDialogue;

    public DialogueObject DefaultDialogue => defaultDialogue;

    private DialogueActivator dialogueActivator;

    private void Start()
    {
        dialogueActivator = GetComponent<DialogueActivator>();

        if (GameEventsManager.Instance != null)
        {
            GameEventsManager.Instance.questEvents.onQuestStateChange += OnQuestStateChange;
        }

        UpdateDialogue();
    }

    private void OnQuestStateChange(Quest quest)
    {
        if (quest.info.id == questId)
        {
            UpdateDialogue();
        }
    }

    private void UpdateDialogue()
    {
        if (QuestManager.Instance == null || string.IsNullOrEmpty(questId))
        {
            SetDialogue(defaultDialogue);
            return;
        }

        Quest quest = QuestManager.Instance.GetQuestById(questId);
        if (quest == null)
        {
            SetDialogue(defaultDialogue);
            return;
        }

        DialogueObject bestMatch = FindBestDialogueForState(quest);
        SetDialogue(bestMatch != null ? bestMatch : defaultDialogue);
    }

    private DialogueObject FindBestDialogueForState(Quest quest)
    {
        DialogueObject bestMatch = null;
        int bestPriority = -1;

        foreach (DialogueState state in dialogueStates)
        {
            if (state.questState == quest.state)
            {
                int priority = CalculatePriority(state, quest);
                if (priority > bestPriority)
                {
                    bestPriority = priority;
                    bestMatch = state.dialogue;
                }
            }
        }

        return bestMatch;
    }

    private int CalculatePriority(DialogueState state, Quest quest)
    {
        // Check step index constraints
        bool stepValid = true;
        if (state.minStepIndex >= 0 && quest.currentQuestStepIndex < state.minStepIndex)
            stepValid = false;
        if (state.maxStepIndex >= 0 && quest.currentQuestStepIndex > state.maxStepIndex)
            stepValid = false;

        if (!stepValid)
            return -1;

        // Higher priority for more specific step ranges
        int priority = 0;
        if (state.minStepIndex >= 0 || state.maxStepIndex >= 0)
            priority += 10;

        return priority;
    }

    private void SetDialogue(DialogueObject dialogue)
    {
        if (dialogueActivator != null && dialogue != null)
        {
            dialogueActivator.UpdateDialogueObject(dialogue);
        }
    }

    public void ResetToDefault()
    {
        Debug.Log($"NPCDialogueController: Resetting to default dialogue for {questId}");

        if (QuestManager.Instance == null || string.IsNullOrEmpty(questId))
        {
            SetDialogue(defaultDialogue);
            return;
        }

        Quest quest = QuestManager.Instance.GetQuestById(questId);
        if (quest == null)
        {
            SetDialogue(defaultDialogue);
            return;
        }

        // Only reset to default if quest hasn't started
        if (quest.state == QuestState.REQUIREMENTS_NOT_MET || quest.state == QuestState.CAN_START)
        {
            SetDialogue(defaultDialogue);
        }
        else
        {
            // Quest has started, use normal state handling
            UpdateDialogue();
        }
    }

    private void OnDisable()
    {
        if (GameEventsManager.Instance != null)
        {
            GameEventsManager.Instance.questEvents.onQuestStateChange -= OnQuestStateChange;
        }
    }
}