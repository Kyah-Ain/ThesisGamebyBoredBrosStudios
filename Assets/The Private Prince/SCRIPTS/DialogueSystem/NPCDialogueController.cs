using UnityEngine;

public class NPCDialogueController : MonoBehaviour
{
    [SerializeField] private string questId;
    [SerializeField] private DialogueObject defaultDialogue;
    [SerializeField] private DialogueObject inProgressDialogue;
    [SerializeField] private DialogueObject readyToCompleteDialogue;
    [SerializeField] private DialogueObject afterCompletionDialogue; // NEW

    private DialogueActivator dialogueActivator;
    private bool questStepCompleted = false; // NEW: Track if this NPC's part is done

    private void Start()
    {
        dialogueActivator = GetComponent<DialogueActivator>();
        GameEventsManager.Instance.questEvents.onQuestStateChange += OnQuestStateChange;
        GameEventsManager.Instance.questEvents.onQuestStepStateChange += OnQuestStepStateChange; // NEW
    }

    private void OnQuestStateChange(Quest quest)
    {
        if (quest.info.id == questId)
        {
            switch (quest.state)
            {
                case QuestState.REQUIREMENTS_NOT_MET:
                case QuestState.CAN_START:
                    dialogueActivator.UpdateDialogueObject(defaultDialogue);
                    break;
                case QuestState.IN_PROGRESS:
                    if (questStepCompleted && afterCompletionDialogue != null)
                    {
                        dialogueActivator.UpdateDialogueObject(afterCompletionDialogue);
                    }
                    else
                    {
                        dialogueActivator.UpdateDialogueObject(inProgressDialogue);
                    }
                    break;
                case QuestState.CAN_FINISH:
                    dialogueActivator.UpdateDialogueObject(readyToCompleteDialogue);
                    break;
                case QuestState.FINISHED:
                    if (afterCompletionDialogue != null)
                        dialogueActivator.UpdateDialogueObject(afterCompletionDialogue);
                    break;
            }
        }
    }

    // NEW: Track when this NPC's quest step is completed
    private void OnQuestStepStateChange(string questId, int stepIndex, QuestStepState questStepState)
    {
        if (questId == "Tandang_Quest")
        {
            // Store Owner completes step 1 (getting package)
            if (this.questId == "Tandang_Quest" && stepIndex == 1)
            {
                questStepCompleted = true;
                // Update dialogue immediately
                Quest quest = QuestManager.Instance.GetQuestById(questId);
                OnQuestStateChange(quest);
            }
        }
    }

    private void OnDisable()
    {
        if (GameEventsManager.Instance != null)
        {
            GameEventsManager.Instance.questEvents.onQuestStateChange -= OnQuestStateChange;
            GameEventsManager.Instance.questEvents.onQuestStepStateChange -= OnQuestStepStateChange;
        }
    }
}