using System.Collections.Generic;
using UnityEngine;

namespace Ain
{
    // Required DebuggerNiAinPjls.cs for this to be able to monitor debugs, otherwise use the old one
    [RequireComponent(typeof(DebuggerNiAinPjls))]
    public class QuestManager : MonoBehaviour
    {
        // ------------------------- VARIABLES -------------------------

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

            InitializedQuestMap();

            // THESE ARE FOR DEBUGGING PURPOSES ONLY
            Quest quest = GetQuestById("Mission_1");
            debuggerNiAin.Log(DebugQuestAttributes(quest));
            debuggerNiAin.Log(DebugQuestStats(quest));
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
        private Quest GetQuestById(string id)
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
                $"Initialized {quest.info.questName}: \n" +
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
