using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestPointsStateSaver : MonoBehaviour
{
    // ------------------------- VARIABLES -------------------------

    private string objId; // Unique ID

    [SerializeField] private string chapterId = "Prologue_";

    // ------------------------- UNITY METHODS -------------------------

    private void Awake()
    {
        objId = this.gameObject.name;
    }

    private void OnEnable()
    {
        if (SaveManager.Instance.questCheckpoints.Contains(chapterId + objId))
        {
            // It was destroyed in a previous session, so remove it immediately on load.
            Destroy(this.gameObject);
        }
    }

    // ------------------------- CUSTOM METHODS -------------------------

    // Call this from a UnityEvent, trigger, or any other game logic
    // to permanently destroy this object and remember its state.
    public void CacheDestroyThis()
    {
        if (!SaveManager.Instance.questCheckpoints.Contains(chapterId + objId))
        {
            SaveManager.Instance.questCheckpoints.Add(chapterId + objId);
        }

        Destroy(this.gameObject);
    }
}