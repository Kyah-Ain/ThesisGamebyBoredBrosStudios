using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ain
{
    public abstract class QuestStep : MonoBehaviour
    {
        // ------------------------- VARIABLES -------------------------

        [Header("REFERENCE")]
        private string questId; // Reference to the quest this script's instance belongs to
        
        [Header("STATUS")]
        private bool isFinished = false; // Indicates if the quest step is finished/completed
        
        // ------------------------ INITIALIZERS -------------------------
        #region INITIALIZERS
        
        // Method to assign this script's instance to a quest
        public void InitializeQuestStepID(string questId)
        {
            // Overwrites the stored questId from the caller
            this.questId = questId;
        }

        #endregion

        // ---------------------- INHERITABLE METHODS -------------------------
        #region INHERITABLES
        
        protected void FinishQuestStep()
        {
            isFinished = true;

            GameEventsManager.Instance.questEvents.AdvanceQuest(questId);

            Destroy(this.gameObject);
        }
        
        #endregion
    }
}