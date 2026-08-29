using UnityEngine;
using UnityEngine.InputSystem;

namespace Ain
{    
    // Required DebuggerNiAinPjls.cs for this to be able to monitor debugs, otherwise use the old one
    [RequireComponent(typeof(DebuggerNiAinPjls))]
    public class QuestPoint : MonoBehaviour
    {
        // ------------------------- VARIABLES -------------------------

        [Header("REFERENCES")]
        [SerializeField] DebuggerNiAinPjls debuggerNiAin; // Custom debugging script from your dev Ain
        [SerializeField] QuestInfoSO questInfoForPoint; // Reference to the Quest Data Informations
        // private PrivatePrinceControls ppControls; // Note - Ain's old version input handles (Step 1)

        [Header("QUEST")]
        string questId;
        QuestState currentQuestState;

        [Header("SETTINGS")]
        [SerializeField] bool questStartPoint;
        [SerializeField] bool questFinishPoint;

        [Header("STATUS")]
        bool isPlayerNear; // Prompts for the system to know if the player is close enough to interact this

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
            
            #region Ain's old version input handles (Step 2)
            
                // Note - Ain's old version input handles (Step 2)
                // // Evaluates if there is controls initialized in the "GameplayInputManager"
                // if (GameplayInputManager.Instance.Controls == null)
                // {
                //     debuggerNiAin.Error("PlayerInputManager singleton not found! Make sure it exists in the scene.");
                // }
                // else 
                // {
                //     // Accesses the controls from the PlayerInputManager singleton instance
                //     ppControls = GameplayInputManager.Instance.Controls;
                //
                //     debuggerNiAin.Error($"New Input System was set: {ppControls}");
                // }
            
            #endregion

            // Assigns a quest id from the QuestInfoSO
            questId = questInfoForPoint.id;
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

        // OnTriggerEnter is called when this script's object collide with another object
        void OnTriggerEnter(Collider actor)
        {
            if (actor.gameObject.CompareTag("Player"))
            {    
                // Logs the collision
                isPlayerNear = true;
            };
        }

        // OnTriggerExit is called when this script's object un-collide with another object
        void OnTriggerExit(Collider actor)
        {
            if (actor.gameObject.CompareTag("Player"))
            {    
                // Un-logs the collision
                isPlayerNear = false;
            };
        }

        #endregion
        
        // ------------------------- SUBSCRIPTIONS -------------------------
        #region SUBSCRIPTIONS

        // Method to subscribe your local method to an event trigger
        void Subscribe()
        {
            // Set subscriptions of these methods to an event
            // Left (Event Call) += Right (Method that would be called)
            GameEventsManager.Instance.inputEvents.onSubmitPressed += SubmitPressed;
            GameEventsManager.Instance.questEvents.onQuestStateChange += QuestStateChange;
            
            // Note - Ain's old version input handles (Step 3)
            // ppControls.Player.Interact.performed += SubmitPressed;
        }

        // Method to UnSubscribe your local method to an event trigger
        void UnSubscribe()
        {
            // UnSubscribe them methods to an event
            // Left (Event Call) -= Right (Method that would be called)
            GameEventsManager.Instance.inputEvents.onSubmitPressed -= SubmitPressed;
            GameEventsManager.Instance.questEvents.onQuestStateChange -= QuestStateChange;
            
            // Note - Ain's old version input handles (Step 4)
            // ppControls.Player.Interact.performed -= SubmitPressed;
        }

        // ------------------------- EVENT LISTENERS -------------------------

        // Method to update the Quest State for this point 
        void QuestStateChange(Quest quest)
        {
            // Checks if the update receiving is meant for this quest
            if(quest.info.id.Equals(questId))
            {
                // Temporary placeholder for previous state of this quest point
                string previousState = $"{currentQuestState}";

                // Update the state of this from the passed state value from the caller
                currentQuestState = quest.state;

                debuggerNiAin.Log(
                    $"Quest State Changed for Quest ID: {questId} \n" +
                    $"from {previousState} to {currentQuestState}"
                );
            }
        }

        // ------------------------- EVENT TRIGGERS -------------------------

        // Method to Start/Finished a Quest
        void SubmitPressed(InputAction.CallbackContext context)
        {
            // Checks if the player was inside the collider for this script
            if (isPlayerNear)
            {
                // Triggers an event (FOR DEBUGGING PURPOSES)
                // GameEventsManager.Instance.questEvents.StartQuest(questId);
                // GameEventsManager.Instance.questEvents.AdvanceQuest(questId);
                // GameEventsManager.Instance.questEvents.FinishQuest(questId);

                // Evaluates if the quest can be start and was interacted at starting point
                if (currentQuestState.Equals(QuestState.CAN_START) && 
                    questStartPoint)
                {
                    StartQuest();
                }
                // Evaluates if the quest can be finished and was interacted at finishing point
                else if (currentQuestState.Equals(QuestState.CAN_FINISH) &&  
                    questFinishPoint)
                {
                    CompleteQuest();
                }
            } 
        }

        #endregion

        // ------------------------- MISSION METHODS -------------------------
        #region MISSION METHODS

        // Method to call a request for starting a Quest
        void StartQuest()
        {
            GameEventsManager.Instance.questEvents.StartQuest(questId);
        }

        // Method to call a request for finishing a Quest
        void CompleteQuest()
        {
            GameEventsManager.Instance.questEvents.FinishQuest(questId);
        }

        #endregion
    }
}