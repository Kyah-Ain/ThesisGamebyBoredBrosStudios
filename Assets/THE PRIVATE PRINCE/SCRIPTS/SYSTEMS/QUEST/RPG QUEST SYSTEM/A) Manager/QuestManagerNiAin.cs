using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Ain
{
    // Required DebuggerNiAinPjls.cs for this to be able to monitor debugs, otherwise use the old one
    [RequireComponent(typeof(DebuggerNiAinPjls))]
    public class QuestManager : MonoBehaviour
    {
        // ------------------------- VARIABLES -------------------------

        // Global Reference to this script (Read Only, cannot modify)
        public static QuestManager Instance { get; private set; }

        [Header("REFERENCES")]
        [SerializeField] DebuggerNiAinPjls debuggerNiAin; // Custom debugging script from your dev Ain
        Dictionary<string, Quest> questMap; // Dictionary list  of references for Quests

        [Header("TEMPORARY VARIABLES")]
        int currentPlayerLevel = 0; // Stores the current player level

        // ----------------------- UNITY METHODS -------------------------
        #region UNITY METHODS

        // Awake is called when this script was first initialized & loaded
        private void Awake()
        {
            // Checks if our reference for the script was not set
            if (debuggerNiAin == null)
            {
                // If it is not, then set it automatically by looking for the script class from this object
                debuggerNiAin = this.GetComponent<DebuggerNiAinPjls>();
            }

            // Checks if this instance is a duplicate
            if (Instance != null)
            {
                // Prompts a message then deletes this instance immediately
                debuggerNiAin.Log(
                    $"Found a duplicate for this Manager, deleting this now."
                );

                Destroy(this.gameObject);
            }
            
            // Set this script as the one and only instance in the game
            Instance = this;

            // Detach this gameobject to any parent object its attached to
            transform.SetParent(null);

            InitializedQuestMap();

            // Persist this object so it wont destroy between game loads
            DontDestroyOnLoad(this.transform.root.gameObject);

            // THESE ARE FOR DEBUGGING PURPOSES ONLY
            // Quest quest = GetQuestById("Mission_1");
            // debuggerNiAin.Log(DebugQuestAttributes(quest));
            // debuggerNiAin.Log(DebugQuestStats(quest));
        }

        // OnEnable is called when the object becomes enabled and active
        void OnEnable()
        {
            Subscribe();
        }

        // OnDisable is called when the object becomes disabled
        void OnDisable()
        {
            UnSubscribe();
        }

        // OnDisable is called when the object becomes disabled
        void Start()
        {
            // InitializedQuestStates();
        }

        // Update is called once per frame after Start()
        void Update()
        {
            UpdateUnlockedQuests();
        }

        #endregion

        // ------------------------- SUBSCRIPTIONS -------------------------
        #region SUBSCRIPTIONS METHODS

        // Method to subscribe your local method to an event trigger
        void Subscribe()
        {
            // Set subscriptions of these methods to an event
            // Left (Event Listener) += Right (Method that would be called)
            GameEventsManager.Instance.questEvents.onStartQuest += StartQuest;
            GameEventsManager.Instance.questEvents.onAdvanceQuest += AdvanceQuest;
            GameEventsManager.Instance.questEvents.onFinishQuest += FinishQuest;
        }

        // Method to UnSubscribe your local method to an event trigger
        void UnSubscribe()
        {
            // UnSubscribe them methods to an event
            // Left (Event Listener) -= Right (Method that would be removed)
            GameEventsManager.Instance.questEvents.onStartQuest -= StartQuest;
            GameEventsManager.Instance.questEvents.onAdvanceQuest -= AdvanceQuest;
            GameEventsManager.Instance.questEvents.onFinishQuest -= FinishQuest;
        }

        // -------------------------- PROCESSORS -------------------------

        // Method to Start a Quest
        public void StartQuest(string id)
        {
            // Stores the retrieved Quest to a temporary variable
            Quest quest = GetQuestById(id);
            
            // Spawns a Quest Step under this QuestManager's gameObject
            quest.InstantiateCurrentQuestStep(this.transform);
            
            // Declares the Quest progressable and up fr completion
            ChangeQuestState(quest.info.id, QuestState.IN_PROGRESS);

            debuggerNiAin.Log(
                $"Started Quest: \n " +
                $"{quest.info.id} - {quest.info.questName}"
            );
        }

        // Method to Advance a Quest
        public void AdvanceQuest(string id)
        {
            // Stores the retrieved Quest to a temporary variable
            Quest quest = GetQuestById(id);
            
            // Increments the Quest Step of the Quest
            quest.MoveToNextStep();
            
            // Checks if there's still Quest Step to fulfill after moving next earlier
            if (quest.CurrentStepExists())
            {
                // Spawns a Quest Step under this QuestManager's gameObject
                quest.InstantiateCurrentQuestStep(this.transform);
            }
            // Condition if any statement above hasn't met
            else
            {
                // Declares the Quest finishable and claimable for rewards
                ChangeQuestState(quest.info.id, QuestState.CAN_FINISH);
            }
            
            debuggerNiAin.Log(
                $"Advanced Quest: \n " +
                $"{quest.info.id} - {quest.info.questName}"
            );
        }

        // Method to Finish a Quest
        public void FinishQuest(string id)
        {
            // Stores the retrieved Quest to a temporary variable
            Quest quest = GetQuestById(id);
            
            // Declares the Quest Completely Done
            ChangeQuestState(quest.info.id, QuestState.FINISHED);

            debuggerNiAin.Log(
                $"Finished Quest: \n " +
                $"{quest.info.id} - {quest.info.questName}"
            );
        }

        // Method to update a Quest Status
        public void ChangeQuestState(string id, QuestState questState)
        {
            // Retrieves a quest from the dictionary and stores it to a temp variable
            Quest quest = GetQuestById(id);
            
            // Set the state retrieved to the desired state passed from the parameter
            quest.state = questState;
            
            // Broadcast the update to the listeners of the event 
            GameEventsManager.Instance.questEvents.QuestStateChange(quest);
        }

        #endregion

        // -------------------------- INITIALIZERS -------------------------
        #region INITIALIZERS

        // Method to initialize a Quest Map
        public void InitializedQuestMap()
        {
            // Creates a fresh quest map 
            questMap = CreateQuestMap();
        }

        // Method to initialize a Quest States
        public void InitializedQuestStates()
        {
            // Iterates through each Quest in the Quest Map
            foreach (Quest quest in questMap.Values)
            {
                // Triggers the event (broadcasts the initial state of all quests)
                GameEventsManager.Instance.questEvents.QuestStateChange(quest);
            }
        }

        // Method to return a Quest Map
        Dictionary<string, Quest> CreateQuestMap()
        {
            // Stores all Scriptable Quest Infos into a temporary list
            // These Scriptable Infos can be found at "Assets/Resources/Quests"
            QuestInfoSO[] allQuest = Resources.LoadAll<QuestInfoSO>("QUESTS");
            
            // Initialize a temporary dictionary list of references for Quests
            Dictionary<string, Quest> idToQuestMap = new Dictionary<string, Quest>();

            // Iterates through each Quest data from all the Scriptable Quest gathered
            foreach (QuestInfoSO questInfoSO in allQuest)
            {
                // Checks if the Quest trying to add to the map was already in the map
                if (idToQuestMap.ContainsKey(questInfoSO.id))
                {
                    // Prompts a log then proceed to the next loop immediately
                    debuggerNiAin.Error($"Duplicated Quest Id found: {questInfoSO.id}");
                    continue;
                }

                // Adds that quest to the temporary dictionary list
                idToQuestMap.Add(questInfoSO.id, new Quest(questInfoSO));
            }

            // Returns the temporary dictionary list's values
            return idToQuestMap;
        }

        #endregion

        // -------------------------- QUEST UPDATE -------------------------
        #region QUEST UPDATES
        
        // Method to unlock quests based on player's level and pre-requisite quests
        void UpdateUnlockedQuests()
        {
            // Iterates through each quests stored inside questMap
            foreach (Quest quest in questMap.Values)
            {
                // Checks if the quest haven't start yet and
                // that the player's progress was enough to avail the quest
                if (quest.state == QuestState.REQUIREMENTS_NOT_MET &&
                    CheckRequirementsMet(quest))
                {
                    // Set a quest startable/progressable
                    ChangeQuestState(quest.info.id, QuestState.CAN_START);
                }
            }
        }
        
        // NOTE ~ optional here
        // Method to update our level tracking to the latest player level
        void UpdatePlayerLvl(int level)
        {
            // Overwrites the stored playerLevel from the parameter
            currentPlayerLevel = level;
        }
        
        #endregion

        // ---------------------------- GETTERS -------------------------
        #region GETTERS
        
        // Method to retrieve a specific quest reference using its ID
        public Quest GetQuestById(string id)
        {
            // Stores a quest reference to a temporary variable
            Quest quest = questMap[id]; 

            // Prompts a log if the Quest trying to retrieve was in the Quest Map
            if (quest == null)
            {
                debuggerNiAin.Error($"A quest with the id ({id}) was not found in the Quest Map.");
            }

            // Returns the search result for the quest reference
            return quest;
        }

        #endregion

        // ---------------------------- HELPERS -------------------------
        #region HELPERS

        // Method to check if the player reached a quest requirements
        bool CheckRequirementsMet(Quest quest)
        {
            // Checks if the player's experience level is enough for the quest
            if (currentPlayerLevel < quest.info.levelRequirement)
            {
                // Immediately skip this quest
                return false;
            }

            // Iterates through all Pre-Requisite quests of this Quest
            foreach (QuestInfoSO prerequisite in quest.info.questPrerequisites)
            {
                // Checks if the Pre-Requisites for this Quests are already finished 
                if (GetQuestById(prerequisite.id).state != QuestState.FINISHED)
                {
                    return false;
                }
            }

            return true;
        }

        #endregion

        // ---------------------------- DEBUGGERS -------------------------
        #region DEBUGGERS

        // Method to output the Quest's Attributes
        string DebugQuestAttributes(Quest quest)
        {
            string QuestAttributes =
            (
                $"Initialized \"{quest.info.questName}:\" \n" +
                $"levelRequirement ({quest.info.levelRequirement}); " +
                $"questPrerequisites ({quest.info.questPrerequisites.Length}); " +
                $"questStepPrefabs ({quest.info.questStepPrefabs.Length}); " +
                $"goldsReward ({quest.info.goldReward}); " +
                $"expReward ({quest.info.expReward})."
            );

            return QuestAttributes;
        }

        // Method to output the Quest's Status
        string DebugQuestStats(Quest quest)
        {
            string QuestStats =
            (
                $"{quest.info.questName}'s Quest State: \n" +
                $"state ({quest.state}), " +
                $"isStepsAvailable ({quest.CurrentStepExists()})."
            );

            return QuestStats;
        }

        #endregion
    }
}