using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "QuestInfoSO", menuName = "ScriptableObjects/QuestInfoSO", order = 1)]
public class QuestInfoSO : ScriptableObject
{
    [field: SerializeField] public string id { get; private set; }

    //ensure the id is always the name of the Scribtable Object asset

    [Header("General")]

    public string displayName;

    [Header("Requirements")]

    public int levelRequirements;
    public QuestInfoSO[] questPrerequisites;

    [Header("Steps")]

    public GameObject[] questStepPrefabs;

    [Header("Rewards")]

    public int expReward;



    private void OnValidate()
    {
        #if UNITY_EDITOR
        id = this.name;
        UnityEditor.EditorUtility.SetDirty(this);
        #endif
    }
}
