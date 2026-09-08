using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Required DebuggerNiAinPjls.cs for this to be able to monitor debugs, otherwise use the old one
[RequireComponent(typeof(DebuggerNiAinPjls))]
public class DialogueBaseQuestPoint : Ain.QuestPoint
{
    // ----------------------- UNITY METHODS -------------------------
    #region UNITY METHODS

    // Awake is called when this script was first initialized & loaded
    protected override void Awake()
    {
        base.Awake();
    }

    // OnEnable is called when the object becomes enabled and active
    protected override void OnEnable()
    {
        Subscribe();
    }

    // OnDisable is called when the object becomes disabled
    protected override void OnDisable()
    {
        UnSubscribe();
    }

    // OnTriggerEnter is called when this script's object collide with another object
    protected override void OnTriggerEnter(Collider actor)
    {
        // Do nothing...
    }

    // OnTriggerExit is called when this script's object un-collide with another object
    protected override void OnTriggerExit(Collider actor)
    {
        // Do nothing...
    }

    #endregion
    
    // ------------------------- SUBSCRIPTIONS -------------------------
    #region SUBSCRIPTIONS

    // Method to subscribe your local method to an event trigger
    protected override void Subscribe()
    {
        // Set subscriptions of these methods to an event
        // Left (Event Call) += Right (Method that would be called)
        GameEventsManager.Instance.questEvents.onQuestStateChange += base.QuestStateChange;
    }

    // Method to UnSubscribe your local method to an event trigger
    protected override void UnSubscribe()
    {
        // UnSubscribe them methods to an event
        // Left (Event Call) -= Right (Method that would be called)
        GameEventsManager.Instance.questEvents.onQuestStateChange -= base.QuestStateChange;
    }
    
    #endregion
    
    // ------------------------- MISSION METHODS -------------------------
    #region MISSION METHODS

    // Method to call a request for starting a Quest
    public new void StartQuest()
    {
        // Evaluates if the quest can be started
        if (base.currentQuestState.Equals(QuestState.CAN_START))
        {
            base.StartQuest();
        }
    }

    // Method to call a request for finishing a Quest
    public new void CompleteQuest()
    {
        // Evaluates if the quest can be finished
        if (base.currentQuestState.Equals(QuestState.CAN_FINISH))
        {
            base.CompleteQuest();
        }
    }
    
    #endregion
}
