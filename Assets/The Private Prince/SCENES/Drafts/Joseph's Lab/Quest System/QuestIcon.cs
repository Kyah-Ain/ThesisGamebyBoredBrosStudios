using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestIcon : MonoBehaviour
{
    [Header("Icons")]

    [SerializeField] private GameObject requirementsNotMetStartIcon;

    [SerializeField] private GameObject requirementsNotMetFinishIcon;

    [SerializeField] private GameObject canStartIcon;

    [SerializeField] private GameObject canFinishIcon;

    public void SetState(QuestState newState, bool startPoint, bool finishPoint)
    {
        //set all to inactive
        requirementsNotMetStartIcon.SetActive(false);
        requirementsNotMetFinishIcon.SetActive(false);
        canStartIcon.SetActive(false);
        canFinishIcon.SetActive(false);

        switch (newState)
        {
            case QuestState.REQUIREMENTS_NOT_MET:
                if (startPoint) { requirementsNotMetStartIcon.SetActive(true); }
                break;
            case QuestState.CAN_START:
                if (startPoint) { requirementsNotMetFinishIcon.SetActive(true); }
                break;
            case QuestState.IN_PROGRESS:
                if (startPoint) { canStartIcon.SetActive(true); }
                break;
            case QuestState.CAN_FINISH:
                if (startPoint) { canFinishIcon.SetActive(true); }
                break;
            case QuestState.FINISHED:
                break;
            default:
                Debug.LogWarning("Quest State not recognized by switch statement for quest icon: " + newState);
                break;
        }
    }
}
