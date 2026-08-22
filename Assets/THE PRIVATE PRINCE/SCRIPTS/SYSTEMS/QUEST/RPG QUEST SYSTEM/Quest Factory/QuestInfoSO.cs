using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "QuestInfoSO", menuName = "ScriptableObjects/QuestInfoSO", order = 1)]
public class QuestInfoSO : ScriptableObject
{
    // ------------------------- VARIABLES -------------------------
    [field: SerializeField] public string id { get; private set; } // Unique identifier for the quest, set to the name of the ScriptableObject asset (automatically assigned in OnValidate)

    [Header("General")]
    public string questName = "You Haven't Set a Quest Name Yet"; // The name of the quest that will be shown to the player

    [Header("Requirements")]
    public int levelRequirement; // The minimum player level required to start the quest
    public QuestInfoSO[] questPrerequisites; // An array of other quests that must be completed before this quest can be started

    [Header("Steps")]
    public GameObject[] questStepPrefabs; // An array of GameObject prefabs that represent the different steps of the quest

    [Header("Rewards")]
    public int goldReward; // The amount of currency the player will receive upon completing the quest
    public int expReward; // The amount of experience points the player will receive upon completing the quest

    // ------------------------- METHODS -------------------------

    // Built-In Uity Method to ensure the 'id' field is always set to the name of the ScriptableObject asset
    private void OnValidate()
    {
        // This method is called in the Unity Editor when the script is loaded or a value is changed in the inspector
        // - it ensures that the 'id' field is always set to the name of the ScriptableObject asset
        // - which can be useful for identifying quests by their asset name

        // If we're in the Unity Editor, set the 'id' field to the name of the asset
        #if UNITY_EDITOR
            id = this.name; // Set the 'id' field to the name of the ScriptableObject asset
            UnityEditor.EditorUtility.SetDirty(this); // Mark the ScriptableObject as dirty to ensure the change is saved
        #endif // End of Unity Editor check
    }
}