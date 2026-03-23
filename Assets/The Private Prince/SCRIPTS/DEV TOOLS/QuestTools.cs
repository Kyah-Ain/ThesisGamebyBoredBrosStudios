using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestTools : MonoBehaviour
{
    // ------------------------- VARIABLES -------------------------

    [Header("Event Placeholders")]
    [Space]
    [SerializeField] private GameEvent onForceStart; 
    [SerializeField] private GameEvent onForceFinish;

    // ------------------------- UNITY METHODS -------------------------

    // Method to Forcefully Start a Quest
    public void ForceStartAQuest() 
    {
        // Brodcast the Event to all Subbcribers
        onForceStart?.TriggerEvent();
    }

    // Method to Forcefully Finish a Quest
    public void ForceFinishAQuest()
    {
        // Brodcast the Event to all Subbcribers
        onForceFinish?.TriggerEvent();
    }
}