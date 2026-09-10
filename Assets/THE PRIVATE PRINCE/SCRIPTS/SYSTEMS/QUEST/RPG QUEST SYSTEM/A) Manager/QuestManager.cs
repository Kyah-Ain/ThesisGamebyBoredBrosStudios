using System.Collections.Generic;
using UnityEngine;

// Required DebuggerNiAinPjls.cs for this to be able to monitor debugs, otherwise use the old one
[RequireComponent(typeof(DebuggerNiAinPjls))]
public class QuestManager : MonoBehaviour
{
    // // --------------------------- VARIABLES ---------------------------
    // #region VARIABLES

    // // Singleton instance of this script for global reference (for Readable Access Only)
    // public static QuestManager Instance { get; private set; } 

    // [Header("REFERENCES")]
    // [SerializeField] DebuggerNiAinPjls debuggerNiAin; // Custom debugging script from your dev Ain
    // [SerializeField] Dictionary<string, Ain.Quest> questMap; // List of reference to all quests created
    
    // [Header("STATUS")]
    // [SerializeField] int currentPlayerLevel = 0; // Reference to the current experience of the player

    // #endregion

    // // --------------------------- UNITY METHODS ---------------------------
    // #region UNITY METHODS

    // // Awake is called when this script was first initialized & loaded
    // private void Awake()
    // {
    //     // Checks if our reference for the script was not set
    //     if (debuggerNiAin == null)
    //     {
    //         // If it is not, then set it automatically by looking for the script class from this object
    //         debuggerNiAin = this.GetComponent<DebuggerNiAinPjls>();
    //     }

    //     // Checks if we already have an instance of this script active
    //     if (Instance != null)
    //     {
    //         // Destroy this script && 
    //         // Skips the remaining logics inside this method 
    //         Destroy(this.gameObject);
    //         return;
    //     }

    //     // Set this script to be the instance active for global access
    //     Instance = this;

    //     // Optional if you want to detach this gameObject to any parent
    //     // transform.SetParent(null); 

    //     // Persist this object's heirachy from being destroyed between load scenes
    //     DontDestroyOnLoad(this.transform.root.gameObject);
    // }

    // // OnEnable is called when the object becomes enabled and active
    // private void OnEnable()
    // {
    //     Subscribe();
    // }

    // // OnDisable is called when the object becomes disabled
    // private void OnDisable()
    // {
    //     Unsubscribe();
    // }

    // // Update is called once per frame
    // private void Update()
    // {
    //     UnlockQuests();
    // }

    // // // OnApplicationQuit is called when the application was closed
    // // private void OnApplicationQuit()
    // // {
    // //     if (questMap == null) return;

    // //     // Backup save to PlayerPrefs on quit; SaveManager handles the primary save path
    // //     foreach (Quest quest in questMap.Values)
    // //     {
    // //         SaveQuest(quest);
    // //     }
    // // }

    // #endregion

    // // --------------------------- SUBSCRIPTION METHODS ---------------------------
    // #region SUBSCRIPTION METHODS

    // private void Subscribe()
    // {
    //     if (GameEventsManager.Instance == null) return;

    //     // Unsubscribe first so re-enabling never double-subscribes
    //     Unsubscribe();

    //     GameEventsManager.Instance.questEvents.onStartQuest += StartQuest;
    //     GameEventsManager.Instance.questEvents.onAdvanceQuest += AdvanceQuest;
    //     GameEventsManager.Instance.questEvents.onFinishQuest += FinishQuest;
    //     GameEventsManager.Instance.questEvents.onQuestStepStateChange += ChangeQuestStepState;
    // }

    // private void Unsubscribe()
    // {
    //     if (GameEventsManager.Instance == null) return;

    //     GameEventsManager.Instance.questEvents.onStartQuest -= StartQuest;
    //     GameEventsManager.Instance.questEvents.onAdvanceQuest -= AdvanceQuest;
    //     GameEventsManager.Instance.questEvents.onFinishQuest -= FinishQuest;
    //     GameEventsManager.Instance.questEvents.onQuestStepStateChange -= ChangeQuestStepState;
    // }

    // #endregion

    // // --------------------------- QUEST METHODS ---------------------------
    // #region QUEST METHODS

    // private void StartQuest(string id)
    // {
    //     Quest quest = GetQuestById(id);
    //     if (quest == null) return;

    //     quest.InstantiateCurrentQuestStep(transform);
    //     ChangeQuestState(quest.info.id, QuestState.IN_PROGRESS);
    // }

    // private void AdvanceQuest(string id)
    // {
    //     Quest quest = GetQuestById(id);
    //     if (quest == null) return;

    //     quest.MoveToNextStep();

    //     if (quest.CurrentStepExists())
    //     {
    //         quest.InstantiateCurrentQuestStep(transform);
    //     }
    //     else
    //     {
    //         ChangeQuestState(quest.info.id, QuestState.CAN_FINISH);
    //     }
    // }

    // private void FinishQuest(string id)
    // {
    //     Quest quest = GetQuestById(id);
    //     if (quest == null)
    //     {
    //         debuggerNiAin.Error($"QuestManager: Could not find quest to finish: {id}");
    //         return;
    //     }

    //     ChangeQuestState(quest.info.id, QuestState.FINISHED);
    // }

    // // Broadcasts every quest's current state. Call after InitializeQuests(),
    // // and only once the receiving scene/UI is guaranteed to already be listening.
    // public void LoadQuests()
    // {
    //     if (questMap == null)
    //     {
    //         InitializeQuests();
    //         return;
    //     }

    //     foreach (Ain.Quest quest in questMap.Values)
    //     {
    //         if (quest.state == QuestState.IN_PROGRESS)
    //         {
    //             quest.InstantiateCurrentQuestStep(transform);
    //         }
    //         GameEventsManager.Instance.questEvents.QuestStateChange(quest);
    //     }
    // }

    // #endregion

    // // --------------------------- SETTERS ---------------------------
    // #region SETTERS

    // // Central place quest state changes flow through: updates state,
    // // broadcasts it, and triggers an autosave.
    // private void ChangeQuestState(string id, QuestState state)
    // {
    //     Ain.Quest quest = GetQuestById(id);
    //     if (quest == null) return;

    //     quest.state = state;
    //     GameEventsManager.Instance.questEvents.QuestStateChange(quest);
    //     SaveManager.Instance.Save();
    // }

    // private void ChangeQuestStepState(string id, int stepIndex, QuestStepState questStepState)
    // {
    //     Ain.Quest quest = GetQuestById(id);
    //     if (quest == null) return;

    //     quest.StoreQuestStepState(questStepState, stepIndex);
    //     ChangeQuestState(id, quest.state);
    // }

    // private void ChangePlayerLevel(int level)
    // {
    //     currentPlayerLevel = level;
    // }

    // #endregion

    // // --------------------------- GETTERS ---------------------------
    // #region GETTERS

    // public Ain.Quest GetQuestById(string id)
    // {
    //     if (questMap != null && questMap.TryGetValue(id, out Ain.Quest quest))
    //     {
    //         return quest;
    //     }

    //     debuggerNiAin.Error($"QuestManager: Id not found in the Quest Map: {id}");
    //     return null;
    // }

    // public Dictionary<string, QuestData> GetAllQuestData()
    // {
    //     Dictionary<string, QuestData> questDataMap = new Dictionary<string, QuestData>();

    //     if (questMap == null) return questDataMap;

    //     foreach (var kvp in questMap)
    //     {
    //         questDataMap.Add(kvp.Key, kvp.Value.GetQuestData());
    //     }

    //     return questDataMap;
    // }

    // #endregion

    // // --------------------------- PROCESSORS ---------------------------
    // #region PROCESSORS

    // public void InitializeQuests(Dictionary<string, QuestData> savedQuestData = null)
    // {
    //     questMap = CreateQuestMap(savedQuestData);

    //     foreach (Quest quest in questMap.Values)
    //     {
    //         if (quest.state == QuestState.IN_PROGRESS)
    //         {
    //             quest.InstantiateCurrentQuestStep(transform);
    //         }
    //         GameEventsManager.Instance.questEvents.QuestStateChange(quest);
    //     }
    // }

    // private Dictionary<string, Quest> CreateQuestMap(Dictionary<string, QuestData> savedQuestData)
    // {
    //     QuestInfoSO[] allQuests = Resources.LoadAll<QuestInfoSO>("Quests");
    //     Dictionary<string, Quest> idToQuestMap = new Dictionary<string, Quest>();

    //     foreach (QuestInfoSO questInfo in allQuests)
    //     {
    //         if (idToQuestMap.ContainsKey(questInfo.id))
    //         {
    //             debuggerNiAin.Warn($"QuestManager: Duplicate quest ID found: {questInfo.id}");
    //             continue;
    //         }

    //         bool hasSavedData = savedQuestData != null && savedQuestData.TryGetValue(questInfo.id, out QuestData data);

    //         idToQuestMap.Add(
    //             questInfo.id,
    //             hasSavedData
    //                 ? new Quest(questInfo, savedQuestData[questInfo.id].state, savedQuestData[questInfo.id].questStepIndex, savedQuestData[questInfo.id].questStepStates)
    //                 : new Quest(questInfo)
    //         );
    //     }

    //     return idToQuestMap;
    // }

    // #endregion

    // // --------------------------- HELPERS ---------------------------
    // #region HELPERS

    // bool CheckRequirementsMet(Ain.Quest quest)
    // {
    //     // Checks if the player's experience level is enough for the quest
    //     if (currentPlayerLevel < quest.info.levelRequirement)
    //     {
    //         // Immediately skip this quest
    //         return false;
    //     }

    //     // Iterates through all Pre-Requisite quests of this Quest
    //     foreach (QuestInfoSO prerequisite in quest.info.questPrerequisites)
    //     {
    //         // Checks if the Pre-Requisites for this Quests are already finished 
    //         if (GetQuestById(prerequisite.id).state != QuestState.FINISHED)
    //         {
    //             return false;
    //         }
    //     }

    //     return true;
    // }

    // // Method to call for unlocking Quests
    // void UnlockQuests()
    // {
    //     // Immediately exit the logic if there's no quests generated at all
    //     if (questMap == null) return;

    //     // Promote any quest whose requirements just became met
    //     foreach (Ain.Quest quest in questMap.Values)
    //     {
    //         // Holds the value if a quest was startable
    //         bool isQuestStartable = CheckRequirementsMet(quest);

    //         // Checks if the quest was previously un-startable &&
    //         // If its now startable
    //         if (quest.state == QuestState.REQUIREMENTS_NOT_MET && 
    //             isQuestStartable)
    //         {
    //             // Sets a quest to be startable
    //             ChangeQuestState(quest.info.id, QuestState.CAN_START);
    //         }
    //     }
    // }

    // #endregion

    // // --------------------------- OPTIONALS ---------------------------
    // #region OPTIONALS

    // //private void ClaimRewards(Quest quest)
    // //{
    // //  GameEventsManager.Instance.goldEvents.GoldGained(quest.info.goldReward);
    // //  GameEventsManager.instance.playerEvents.ExperienceGained(quest.info.experienceReward);
    // //}

    // // Lightweight backup save/load path, separate from the main SaveManager system.
    // private void SaveQuest(Ain.Quest quest)
    // {
    //     try
    //     {
    //         string serializedData = JsonUtility.ToJson(quest.GetQuestData());
    //         PlayerPrefs.SetString(quest.info.id, serializedData);
    //     }
    //     catch (System.Exception e)
    //     {
    //         debuggerNiAin.Error($"QuestManager: Failed to save quest '{quest.info.id}': {e}");
    //     }
    // }

    // private Quest LoadQuest(QuestInfoSO questInfo)
    // {
    //     try
    //     {
    //         if (PlayerPrefs.HasKey(questInfo.id))
    //         {
    //             string serializedData = PlayerPrefs.GetString(questInfo.id);
    //             QuestData questData = JsonUtility.FromJson<QuestData>(serializedData);
    //             return new Quest(questInfo, questData.state, questData.questStepIndex, questData.questStepStates);
    //         }

    //         return new Quest(questInfo);
    //     }
    //     catch (System.Exception e)
    //     {
    //         debuggerNiAin.Error($"QuestManager: Failed to load quest '{questInfo.id}': {e}");
    //         return null;
    //     }
    // }

    // #endregion
}