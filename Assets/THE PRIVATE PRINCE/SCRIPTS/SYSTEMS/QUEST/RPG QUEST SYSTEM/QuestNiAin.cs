using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ain
{
    public class Quest
    {
        // ------------------------- VARIABLES -------------------------

        // static info
        public QuestInfoSO info; // Reference for QuestInfoSO.cs

        // state info
        public QuestState state; // Reference for QuestState.cs

        public int currentQuestStepIndex { get; private set; } // Task's current progress

        // ------------------------ CONSTRUCTOR -------------------------

        // Method to set internal values when initializing a Quest
        public Quest(QuestInfoSO questInfo)
        {
            // Fills the internal values of the copy/instance of this Quest
            this.info = questInfo;
            this.state = QuestState.REQUIREMENTS_NOT_MET;
            this.currentQuestStepIndex = 0;
        }

        // ------------------------ INSTANTIATOR -------------------------

        // Method to spawn the Quest steps in the Heirachy
        public void InstantiateCurrentQuestStep(Transform parentTransform)
        {
            // Get's hold to the reference for the current quest step index's prefab
            GameObject questStepPrefab = GetCurrentQuestStepPrefab();

            // Checks if there's a prefab we can materialize or spawn 
            if (questStepPrefab != null)
            {
                // Instantiate the object quest step prefab under a parentObject
                Object.Instantiate<GameObject>(questStepPrefab, parentTransform);
            }
        }

        // --------------------------- HELPERS -------------------------

        // Method to retrieves the Gameobject of the current step progress
        private GameObject GetCurrentQuestStepPrefab()
        {
            // Evaluates if there's a Quest Step left to materialize
            if (CurrentStepExists())
            {
                // Returns a reference to the QuestInfoSO.cs' current questStepPrefab
                GameObject questStepPrefab = info.questStepPrefabs[currentQuestStepIndex];

                return questStepPrefab;
            }

            // Returns an error message and a null if the Quest hasn't finished when it should be
            Debug.LogWarning("You've reached the Quest's last step already." + 
                             "The quest should be finished by now. \n" +
                             $"You're tryng to access: QuestId = {info.id}, StepIndex = {currentQuestStepIndex}");

            return null;
        }

        // Method to move up the Quest Step progress
        public void MoveToNextStep()
        {
            // Progress the quest
            currentQuestStepIndex++;
        }

        // Method to prevent list/array out of bounds when progressing step
        public bool CurrentStepExists()
        {
            // Returns true if there are steps left to finish, otherwise false
            return currentQuestStepIndex < info.questStepPrefabs.Length;
        }
    }
}