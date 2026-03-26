using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    // ------------------------- VARIABLES -------------------------

    public static QuestManager Instance { get; private set; }

    private Dictionary<string, Quest> questMap;

    private int currentPlayerLevel;

    // ------------------------- UNITY METHODS -------------------------

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
        transform.SetParent(null);
        DontDestroyOnLoad(this.transform.root);

        //questMap = CreateQuestMap();
    }

    private void OnEnable()
    {
        SubscribeToEvents();
    }

    private void OnDisable()
    {
        UnsubscribeToEvents();
    }

    private void Start()
    {
        //// Recreate quest map with fresh data
        //questMap = CreateQuestMap();
    }

    private void Update()
    {
        if (questMap != null) 
        {
            // loops through all quests
            foreach (Quest quest in questMap.Values)
            {
                // if we are now meeting the requirements, switch over to the CAN_START state
                if (quest.state == QuestState.REQUIREMENTS_NOT_MET && CheckRequirementsMet(quest))
                {
                    ChangeQuestState(quest.info.id, QuestState.CAN_START);
                }
            }
        }
    }

    private void OnApplicationQuit()
    {
        Debug.Log($"QuestManager: OnApplicationQuit() - Saving all quests");

        if (questMap != null)
        {
            foreach (Quest quest in questMap.Values)
            {
                // Optional: Save to PlayerPrefs as backup
                SaveQuest(quest);

                // Your main save system will also save when the game saves
                // So you don't need to call SaveManager here unless you want to auto-save on quit
            }
        }
    }

    // ------------------------- EVENT METHODS -------------------------

    private void SubscribeToEvents() 
    {
        // Unsubsscribe first to ensure having a fresh subscription to all events
        UnsubscribeToEvents();

        // Backup subscription in case it's missed due to execution order
        if (GameEventsManager.Instance != null)
        {
            GameEventsManager.Instance.questEvents.onStartQuest += StartQuest;
            GameEventsManager.Instance.questEvents.onAdvanceQuest += AdvanceQuest;
            GameEventsManager.Instance.questEvents.onFinishQuest += FinishQuest;

            GameEventsManager.Instance.questEvents.onQuestStepStateChange += QuestStepStateChange;

            //GameEventsManager.Instance.playerEvents.onPlayerLevelChange += PlayerLevelChange;

            Debug.Log("QuestManager: Successfully subscribed to all events");
        }
    }

    private void UnsubscribeToEvents() 
    {
        GameEventsManager.Instance.questEvents.onStartQuest -= StartQuest;
        GameEventsManager.Instance.questEvents.onAdvanceQuest -= AdvanceQuest;
        GameEventsManager.Instance.questEvents.onFinishQuest -= FinishQuest;

        GameEventsManager.Instance.questEvents.onQuestStepStateChange -= QuestStepStateChange;

        //GameEventsManager.Instance.playerEvents.onPlayerLevelChange -= PlayerLevelChange no level up currently will add on a later date
    }

    // ------------------------- QUEST METHODS -------------------------

    //private Dictionary<string, Quest> CreateQuestMap()
    //{
    //    // Loads all QuestInfoSO Scriptable Objects undet the Assets/Resources/Quests folder
    //    QuestInfoSO[] allQuest = Resources.LoadAll<QuestInfoSO>("Quests");

    //    // Create the quest map
    //    Dictionary<string, Quest> idToQuestMap = new Dictionary<string, Quest>();
    //    foreach (QuestInfoSO questInfo in allQuest)
    //    {
    //        if (idToQuestMap.ContainsKey(questInfo.id))
    //        {
    //            Debug.LogWarning("Duplicate ID found when creating quest map: " + questInfo.id);
    //        }
    //        idToQuestMap.Add(questInfo.id, LoadQuest(questInfo));
    //    }
    //    return idToQuestMap;
    //}

    private Dictionary<string, Quest> CreateQuestMap(Dictionary<string, QuestData> savedQuestData = null)
    {
        QuestInfoSO[] allQuest = Resources.LoadAll<QuestInfoSO>("Quests");
        Dictionary<string, Quest> idToQuestMap = new Dictionary<string, Quest>();

        foreach (QuestInfoSO questInfo in allQuest)
        {
            Quest quest;

            // Use saved data if available
            if (savedQuestData != null && savedQuestData.ContainsKey(questInfo.id))
            {
                QuestData data = savedQuestData[questInfo.id];
                // Pass the individual parameters from QuestData
                quest = new Quest(questInfo, data.state, data.questStepIndex, data.questStepStates);
            }
            else
            {
                quest = new Quest(questInfo);
            }

            idToQuestMap.Add(questInfo.id, quest);
        }
        return idToQuestMap;
    }

    // ...
    public void InitializeQuests(Dictionary<string, QuestData> savedQuestData = null)
    {
        questMap = CreateQuestMap(savedQuestData);

        // Broadcast initial state of all quests
        foreach (Quest quest in questMap.Values)
        {
            if (quest.state == QuestState.IN_PROGRESS)
            {
                quest.InstantiateCurrentQuestStep(this.transform);
            }
            GameEventsManager.Instance.questEvents.QuestStateChange(quest);
        }
    }

    // ...
    private void StartQuest(string id)
    {
        // Start the quest
        Quest quest = GetQuestById(id);

        // DEBUG: Check quest state - we can't access private field directly
        Debug.Log($"Starting quest: {id}, State: {quest.state}");

        quest.InstantiateCurrentQuestStep(this.transform);
        ChangeQuestState(quest.info.id, QuestState.IN_PROGRESS);
    }

    // ...
    public void LoadQuests() 
    {
        if (questMap != null)
        {
            // Broadcast the initial state of all quests on startup
            foreach (Quest quest in questMap.Values)
            {
                if (quest.state == QuestState.IN_PROGRESS)
                {
                    quest.InstantiateCurrentQuestStep(this.transform);
                }
                GameEventsManager.Instance.questEvents.QuestStateChange(quest);
            }
        }
        else
        {
            InitializeQuests();
        }

        Debug.Log($"QuestManager: THIS IS PROBLEMATIC WHEN BEING CALLED AT Awake() or Start()" +
                  $" if your game has multiple scene ASYNCs loading at the same time" +
                  $" THAT RECEIVES UPDATES FROM THIS MANAGER!! \n" +
                  $"NOTE - If they are ON A DIFFRENT SCENE (The Manager and The Observer) the Manager" +
                  $" would brodcast data that WOULD BE MISSED by the observeer because IT'S NOT" +
                  $" BEEN ENABLED YET WHEN THE MANAGER BRODCAST IT.");
    }

    // ...
    private void AdvanceQuest(string id)
    {
        Debug.Log($"QuestManager: Advancing quest {id}");
    
        Quest quest = GetQuestById(id);

        quest.MoveToNextStep();

        if (quest.CurrentStepExists())
        {
            quest.InstantiateCurrentQuestStep(this.transform);
        }
        else
        {
            ChangeQuestState(quest.info.id, QuestState.CAN_FINISH);
            Debug.Log($"QuestManager: Quest ready to finish - state: {quest.state}");
        }
    }

    private void FinishQuest(string id)
    {
        Debug.Log($"QuestManager: Finishing quest {id}");

        Quest quest = GetQuestById(id);
        if (quest != null)
        {
            Debug.Log($"Before finish - State: {quest.state}");
            ChangeQuestState(quest.info.id, QuestState.FINISHED);
            Debug.Log($"After finish - State: {quest.state}");
        }
        else
        {
            Debug.LogError($"Could not find quest to finish: {id}");
        }
    }

    // ------------------------- CHANGER METHODS -------------------------

    // Method that auto-saves quest progress when quest state changes
    private void ChangeQuestState(string id, QuestState state)
    {
        Quest quest = GetQuestById(id);
        quest.state = state;
        GameEventsManager.Instance.questEvents.QuestStateChange(quest);

        // Auto-save on important quest changes
        SaveManager.Instance.Save();
    }

    private void PlayerLevelChange(int level)
    {
        currentPlayerLevel = level;
    }

    private void QuestStepStateChange(string id, int stepIndex, QuestStepState questStepState)
    {
        Quest quest = GetQuestById(id);
        quest.StoreQuestStepState(questStepState, stepIndex);
        ChangeQuestState(id, quest.state);
    }


    // ------------------------- RETRIEVE METHODS -------------------------

    //public List<Quest> GetAllQuests()
    //{
    //    List<Quest> allQuests = new List<Quest>();
    //    foreach (var quest in questMap.Values)
    //    {
    //        allQuests.Add(quest);
    //    }
    //    return allQuests;
    //}

    public Dictionary<string, QuestData> GetAllQuestData()
    {
        Debug.Log($"QuestManager: GetAllQuestData() called - questMap count: {questMap?.Count ?? 0}");

        if (questMap == null)
        {
            Debug.LogWarning($"QuestManager: questMap is null, returning empty dictionary");
            return new Dictionary<string, QuestData>();
        }

        Dictionary<string, QuestData> questDataMap = new Dictionary<string, QuestData>();
        foreach (var kvp in questMap)
        {
            QuestData questData = kvp.Value.GetQuestData();
            questDataMap.Add(kvp.Key, questData);

            Debug.Log($"QuestManager: Getting data for quest {kvp.Key} - State: {questData.state}, StepIndex: {questData.questStepIndex}");

            // Optional: Log step states for debugging
            for (int i = 0; i < questData.questStepStates.Length; i++)
            {
                Debug.Log($"QuestManager: Quest {kvp.Key} - Step {i} state: {questData.questStepStates[i].state}");
            }
        }

        Debug.Log($"QuestManager: Returning {questDataMap.Count} quest data entries");
        return questDataMap;
    }

    public Quest GetQuestById(string id)
    {
        Quest quest = questMap[id];
        if (quest == null)
        {
            Debug.LogError("Id not found in the Quest Map: " + id);
        }
        return quest;
    }

    private bool CheckRequirementsMet(Quest quest)
    {
        bool meetsRequirements = true;

        // checks player level requirement
        if (currentPlayerLevel < quest.info.levelRequirements)
        {
            meetsRequirements = false;
        }

        // check quest prerequisites for completion
        foreach (QuestInfoSO prerequisiteQuest in quest.info.questPrerequisites)
        {
            if (GetQuestById(prerequisiteQuest.id).state != QuestState.FINISHED)
            {
                meetsRequirements |= false;
            }
        }
        return meetsRequirements;
    }

    //private void ClaimRewards(Quest quest)
    //{
    //GameEventsManager.Instance.goldEvents.GoldGained(quest.info.goldReward);
    //GameEventsManager.instance.playerEvents.ExperienceGained(quest.info.experienceReward);
    //}

    // ------------------------- OPTIONAL METHODS -------------------------

    private void SaveQuest(Quest quest)
    {
        try
        {
            QuestData questData = quest.GetQuestData();

            // This creates a JSON string (for debugging or backup)
            string serializedData = JsonUtility.ToJson(questData);

            // Optional: Save to PlayerPrefs as backup (like the YouTube tutorial)
            PlayerPrefs.SetString(quest.info.id, serializedData);

            Debug.Log($"QuestManager: Saved quest {quest.info.id} to PlayerPrefs: {serializedData}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"QuestManager: Failed to save quest with id {quest.info.id}: {e}");
        }
    }

    private Quest LoadQuest(QuestInfoSO questInfo)
    {
        Quest quest = null;
        //try
        //{
        //    // load quest from saved data
        //    if (PlayerPrefs.HasKey(questInfo.id))
        //    {
        //        string serilizedData = PlayerPrefs.GetString(questInfo.id);
        //        QuestData questData = JsonUtility.FromJson<QuestData>(serilizedData);
        //        quest = new Quest(questInfo, questData.state, questData.questStepIndex, questData.questStepStates);
        //    }
        //    // otherwise, initilize a new quest
        //    else
        //    {
        //        quest = new Quest(questInfo);
        //    }
        //}
        //catch (System.Exception e)
        //{
        //    Debug.LogError("Failed to load quest with id " + quest.info.id + ": " + e);
        //}
        return quest;
    }
}
