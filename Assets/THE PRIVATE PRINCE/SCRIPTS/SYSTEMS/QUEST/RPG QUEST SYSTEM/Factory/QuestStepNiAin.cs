using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ain
{
    public abstract class QuestStep : MonoBehaviour
    {
        // ------------------------- VARIABLES -------------------------

        private bool isFinished = false; // Indicates if the quest step is finished/completed

        // ---------------------- INHERITABLE METHODS -------------------------

        protected void FinishQuestStep()
        {
            isFinished = true;

            // TODO - Advance the quest forward now that we've finished this step

            Destroy(this.gameObject);
        }
    }
}