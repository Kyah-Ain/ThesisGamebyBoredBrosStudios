using System.Collections; // Grants access to collections and data structures like ArrayList
using System.Collections.Generic; // Grants access to collections and data structures like List and Dictionary
using UnityEngine;

public class GameEventTrigger : MonoBehaviour
{
    // ------------------------- VARIABLES -------------------------

    [Header("EVENTS")]
    [SerializeField] GameEvent[] eventsToTrigger = new GameEvent[0];

    [Header("SETTINGS")]
    [SerializeField] bool triggerEventsOnAwake; 
    [SerializeField] bool triggerEventsOnStart;
    [SerializeField] bool triggerEventsOnEnable;

    private bool wasEventsAlreadyTriggered;

    // ------------------------ UNITY METHODS -------------------------

    // Awake is called when the script instance is being loaded
    void Awake() 
    {
        if (!triggerEventsOnAwake || wasEventsAlreadyTriggered) return;

        ExecuteEvents();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() 
    {
        if (!triggerEventsOnStart || wasEventsAlreadyTriggered) return;

        ExecuteEvents();
    }

    // OnEnable is called when the object becomes enabled and active
    void OnEnable() 
    {
        if (!triggerEventsOnEnable || wasEventsAlreadyTriggered) return;

        ExecuteEvents();
    }

    // OnEnable is called when the object becomes disabled and inactive
    void OnDisable()
    {
        wasEventsAlreadyTriggered = false;
    }

    // OnDestroy is called when the object is destroyed
    void OnDestroy()
    {
        wasEventsAlreadyTriggered = false;
    }

    // -------------------------- EXECUTION --------------------------

    // Method to trigger a call to an event/s
    public void ExecuteEvents() 
    {
        wasEventsAlreadyTriggered = true;

        // Iterates through all events in the array
        foreach (GameEvent e in eventsToTrigger) 
        {
            // Triggers each event
            e?.TriggerEvent();
        }  
    }

    // ------------------------- EXTENSIONS -------------------------

    #region EXTENSIONS METHODS

    // OVERLOAD Method to trigger a call and pass a "STRING"
    public void ExecuteEvents(string message)
    {
        wasEventsAlreadyTriggered = true;

        // Iterates through all events in the array
        foreach (GameEvent e in eventsToTrigger)
        {
            // Triggers each event
            e?.TriggerEvent(message);
        }
    }

    // OVERLOAD Method to trigger a call and pass a "INT"
    public void ExecuteEvents(int value)
    {
        wasEventsAlreadyTriggered = true;

        // Iterates through all events in the array
        foreach (GameEvent e in eventsToTrigger)
        {
            // Triggers each event
            e?.TriggerEvent(value);
        }
    }

    // OVERLOAD Method to trigger a call and pass a "FLOAT"
    public void ExecuteEvents(float value)
    {
        wasEventsAlreadyTriggered = true;

        // Iterates through all events in the array
        foreach (GameEvent e in eventsToTrigger)
        {
            // Triggers each event
            e?.TriggerEvent(value);
        }
    }

    // OVERLOAD Method to trigger a call and pass a "BOOL"
    public void ExecuteEvents(bool message)
    {
        wasEventsAlreadyTriggered = true;

        // Iterates through all events in the array
        foreach (GameEvent e in eventsToTrigger)
        {
            // Triggers each event
            e?.TriggerEvent(message);
        }
    }

    #endregion
}
