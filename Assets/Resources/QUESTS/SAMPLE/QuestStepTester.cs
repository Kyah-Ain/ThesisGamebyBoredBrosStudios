using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.InputSystem;

public class QuestStepTester : Ain.QuestStep
{
    // ----------------------- UNITY METHODS -------------------------
    #region UNITY METHODS

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
    
    #endregion
    
    // ------------------------- SUBSCRIPTIONS -------------------------
    #region SUBSCRIPTIONS

    // Method to subscribe your local method to an event trigger
    void Subscribe()
    {
        // Set subscriptions of these methods to an event
        // Left (Event Call) += Right (Method that would be called)
        GameEventsManager.Instance.inputEvents.onSubmitPressed += MissionComplete;
    }

    // Method to UnSubscribe your local method to an event trigger
    void UnSubscribe()
    {
        // UnSubscribe them methods to an event
        // Left (Event Call) -= Right (Method that would be called)
        GameEventsManager.Instance.inputEvents.onSubmitPressed -= MissionComplete;
    }
    
    #endregion
    
    // ----------------------- PARENT CONTACTS -------------------------

    // Method to call for completing a Quest
    public void MissionComplete(InputAction.CallbackContext context)
    {
        base.FinishQuestStep();
    }
}