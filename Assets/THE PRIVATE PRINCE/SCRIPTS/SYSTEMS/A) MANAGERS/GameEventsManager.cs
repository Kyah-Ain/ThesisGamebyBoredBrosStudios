using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Required DebuggerNiAinPjls.cs for this to be able to monitor debugs, otherwise use the old one
[RequireComponent(typeof(DebuggerNiAinPjls))]

// Sets this script to execute before most other scripts (the lower = the earlier)
[DefaultExecutionOrder(-50)]

public class GameEventsManager : MonoBehaviour
{
    // ------------------------- VARIABLES -------------------------

    // Global Reference to this script (Read Only, cannot modify)
    public static GameEventsManager Instance { get; private set; }

    [Header("REFERENCES")]
    [SerializeField] DebuggerNiAinPjls debuggerNiAin; // Custom debugging script from your dev Ain

    [Header("EVENTS")]
    public Ain.QuestEvents questEvents;

    public InputEvents inputEvents;
    // public QuestEvents questEvents;

    // ----------------------- UNITY METHODS -------------------------
    #region UNITY METHODS

    // Awake is called when this script was first initialized & loaded
    void Awake()
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

        // Calls the Initialization
        InitializeEvents();

        // Persist this object so it wont destroy between game loads
        DontDestroyOnLoad(this.transform.root.gameObject);
    }

    #endregion

    // ----------------------- INITIALIZERS -------------------------

    // Metthod for setting the events to be referenceable
    void InitializeEvents()
    {
        // Establish reference to the Events
        questEvents = new Ain.QuestEvents();
        inputEvents = new InputEvents(GameplayInputManager.Instance.Controls);

        // questEventsNiAin = new Ain.QuestEvents();
    }
}