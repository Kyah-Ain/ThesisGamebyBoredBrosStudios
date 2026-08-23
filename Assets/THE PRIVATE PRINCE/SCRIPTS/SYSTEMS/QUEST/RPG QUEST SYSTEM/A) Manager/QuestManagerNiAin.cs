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

            // // THESE ARE FOR DEBUGGING PURPOSES ONLY
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
            InitializedQuestStates();
        }

        #endregion

        // ------------------------- SUBSCRIPTIONS -------------------------
        #region UNITY METHODS

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
            // TO DO - start the quest

            debuggerNiAin.Log($"Started Quest: {id}");
        }

        // Method to Advance a Quest
        public void AdvanceQuest(string id)
        {
            // TO DO - start the quest

            debuggerNiAin.Log($"Advanced Quest: {id}");
        }

        // Method to Finish a Quest
        public void FinishQuest(string id)
        {
            // TO DO - start the quest

            debuggerNiAin.Log($"Finished Quest: {id}");
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

        // ---------------------------- GETTERS -------------------------
        #region GETTERS
        
        // Method to retrieve a specific quest reference
        public Quest GetQuestById(string id)
        {
            // Stores a quest reference to a temporary variable
            Quest quest = questMap[id]; 

            // Prompts a log if the id 
            if (quest == null)
            {
                debuggerNiAin.Error($"A quest with the id ({id}) was not found in the Quest Map.");
            }

            // Returns the search result for the quest reference
            return quest;
        }

        #endregion

        // ---------------------------- DEBUGGERS -------------------------
        #region GETTERS

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
