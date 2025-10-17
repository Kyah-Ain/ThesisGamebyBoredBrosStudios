using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    private Dictionary<string, Quest> questMap;

    private int currentPlayerLevel;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        questMap = CreateQuestMap();
    }

    private void OnEnable()
    {
        if (GameEventsManager.Instance == null) return;

        Debug.Log("QuestManager: Subscribing to quest events");
        GameEventsManager.Instance.questEvents.onStartQuest += StartQuest;
        GameEventsManager.Instance.questEvents.onAdvanceQuest += AdvanceQuest;
        GameEventsManager.Instance.questEvents.onFinishQuest += FinishQuest;
        GameEventsManager.Instance.questEvents.onQuestStepStateChange += QuestStepStateChange;

        Debug.Log("QuestManager: Successfully subscribed to all events");
    }

    private void OnDisable()
    {
        GameEventsManager.Instance.questEvents.onStartQuest -= StartQuest;
        GameEventsManager.Instance.questEvents.onAdvanceQuest -= AdvanceQuest;
        GameEventsManager.Instance.questEvents.onFinishQuest -= FinishQuest;

        GameEventsManager.Instance.questEvents.onQuestStepStateChange -= QuestStepStateChange;

        //GameEventsManager.Instance.playerEvents.onPlayerLevelChange -= PlayerLevelChange no level up currently will add on a later date
    }

    private void Start()
    {
        // TEMPORARY: Clear corrupted quest data
        PlayerPrefs.DeleteKey("Tandang_Quest");
        PlayerPrefs.DeleteKey("FightClub_Rumor");
        PlayerPrefs.Save();
        Debug.Log("Cleared saved quest data");

        // Recreate quest map with fresh data
        questMap = CreateQuestMap();

        // Backup subscription in case it's missed due to execution order
        if (GameEventsManager.Instance != null)
        {
            GameEventsManager.Instance.questEvents.onStartQuest += StartQuest;
            GameEventsManager.Instance.questEvents.onAdvanceQuest += AdvanceQuest;
            GameEventsManager.Instance.questEvents.onFinishQuest += FinishQuest;
            GameEventsManager.Instance.questEvents.onQuestStepStateChange += QuestStepStateChange;
        }

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

    private void ChangeQuestState(string id, QuestState state)
    {
        Quest quest = GetQuestById(id);
        quest.state = state;
        GameEventsManager.Instance.questEvents.QuestStateChange(quest);
    }

    private void PlayerLevelChange(int level)
    {
        currentPlayerLevel = level;
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

    private void Update()
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

    private void StartQuest(string id)
    {
        // Start the quest
        Quest quest = GetQuestById(id);

        // DEBUG: Check quest state - we can't access private field directly
        Debug.Log($"Starting quest: {id}, State: {quest.state}");

        quest.InstantiateCurrentQuestStep(this.transform);
        ChangeQuestState(quest.info.id, QuestState.IN_PROGRESS);
    }

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
        Quest quest = GetQuestById(id);
        ChangeQuestState(quest.info.id, QuestState.FINISHED);
    }

    //private void ClaimRewards(Quest quest)
    //{
        //GameEventsManager.Instance.goldEvents.GoldGained(quest.info.goldReward);
        //GameEventsManager.instance.playerEvents.ExperienceGained(quest.info.experienceReward);
    //}

    private void QuestStepStateChange(string id, int stepIndex, QuestStepState questStepState)
    {
        Quest quest = GetQuestById(id);
        quest.StoreQuestStepState(questStepState, stepIndex);
        ChangeQuestState(id, quest.state);
    }

    private Dictionary<string, Quest> CreateQuestMap()
    {
        // Loads all QuestInfoSO Scriptable Objects undet the Assets/Resources/Quests folder
        QuestInfoSO[] allQuest = Resources.LoadAll<QuestInfoSO>("Quests");

        // Create the quest map
        Dictionary<string, Quest> idToQuestMap = new Dictionary<string, Quest>();
        foreach (QuestInfoSO questInfo in allQuest)
        {
            if (idToQuestMap.ContainsKey(questInfo.id))
            {
                Debug.LogWarning("Duplicate ID found when creating quest map: " + questInfo.id);
            }
            idToQuestMap.Add(questInfo.id, LoadQuest(questInfo));
        }
        return idToQuestMap;
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

    private void OnApplicationQuit()
    {
        foreach (Quest quest in questMap.Values)
        {
            SaveQuest(quest);   
        }
    }

    private void SaveQuest(Quest quest)
    {
        try
        {
            QuestData questData = quest.GetQuestData();
            string serializedData = JsonUtility.ToJson(questData);
            // PlayerPrefs is a temp, will make an actual save and load system
            PlayerPrefs.SetString(quest.info.id, serializedData);

            Debug.Log(serializedData);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to save quest with id " + quest.info.id + ": " + e);
        }
    }

    private Quest LoadQuest(QuestInfoSO questInfo)
    {
        Quest quest = null;
        try
        {
            // load quest from saved data
            if (PlayerPrefs.HasKey(questInfo.id))
            {
                string serilizedData = PlayerPrefs.GetString(questInfo.id);
                QuestData questData = JsonUtility.FromJson<QuestData>(serilizedData);
                quest = new Quest(questInfo, questData.state, questData.questStepIndex, questData.questStepStates);
            }
            // otherwise, initilize a new quest
            else
            {
                quest = new Quest(questInfo);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to load quest with id " + quest.info.id + ": " + e);
        }
        return quest;
    }
}
