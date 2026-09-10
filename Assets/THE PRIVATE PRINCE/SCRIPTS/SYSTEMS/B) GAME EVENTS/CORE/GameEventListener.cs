using System;
using UnityEngine;

using UnityEngine.Events;

// Requires a DebuggerKey.cs for it to debug, otherwise it will be muted
[RequireComponent(typeof(DebuggerNiAinPjls))]

// =========================== CUSTOM CLASS ===========================

#region CUSTOMIZED CLASSES

[Serializable]
public class StringEvent : UnityEvent<string> { }

[Serializable]
public class IntEvent : UnityEvent<int> { }

[Serializable]
public class FloatEvent : UnityEvent<float> { }

[Serializable]
public class BoolEvent : UnityEvent<bool> { }

#endregion

// ============================ MAIN CLASS ============================

public class GameEventListener : MonoBehaviour
{
    // ------------------------- VARIABLES -------------------------

    [Header("REFERENCE")]
    [SerializeField] DebuggerNiAinPjls debuggerNiAin;

    // Reference to the GameEvent ScriptableObject that this listener will respond to
    public GameEvent gameEvent;

    // Reference to the UnityEvents that will be invoked when the GameEvent is triggered
    public UnityEvent onEventTriggered;

    // Extension Events
    #region EVENT EXTENSIONS

        public StringEvent onStringEventTriggered;
        public IntEvent onIntEventTriggered;
        public FloatEvent onFloatEventTriggered;
        public BoolEvent onBoolEventTriggered;

    #endregion

    // --------------------- PREPARATION METHODS -------------------------

    // Awake is called when this script first loaded (A Built-In Unity Method)
    // - first one to be automatically called among all methods.
    public void Awake()
    {
        // Checks if our reference for the script was not set
        if (debuggerNiAin == null)
            // If it is not, then set it automatically by looking for the script class from this object
            debuggerNiAin = this.GetComponent<DebuggerNiAinPjls>();
    }

    // OnEnable is called everytime this script were enabled or activated (A Built-In Unity Method)
    // - second one to be automatically called among all methods.
    void OnEnable()
    {
        // Subscribes this script as a listener to the 'gameEvent' when enabled
        gameEvent.AddListener(this);
    }

    // On Disable is called when the behaviour becomes disabled or inactive (A Built-In Unity Method)
    // - second one to be automatically called among all methods.
    void OnDisable()
    {
        // Unsubscribes this script as a listener to the 'gameEvent' when disabled
        gameEvent.RemoveListener(this);
    }

    // --------------------- EXECUTABLE METHODS -------------------------

    // Event Method to call for an execution to all methods under this event
    public void OnEventTriggered()
    {
        debuggerNiAin.Log(
            $"Event call from {gameEvent} was received from {this.gameObject.name}. \n" +
            $"All assigned methods in 'onEventTriggered' were executed successfully!"
        );

        // Invokes the UnityEvent, which can be configured in the Inspector to call any public method on any script
        onEventTriggered.Invoke();
    }

    // ------------------------- EXTENSIONS -------------------------

    #region EXTENSIONS METHODS

    // Event Method to call and pass on string 
    public void OnStringEventTriggered(string parameter)
    {
        debuggerNiAin.Log(
            $"Event call from {gameEvent} and {parameter} was received from {this.gameObject.name}. \n" +
            $"All assigned methods in 'onStringEventTriggered' were executed successfully!"
        );

        // Invokes the UnityEvent, which can be configured in the Inspector to call any public method on any script
        onStringEventTriggered.Invoke(parameter);
    }

    // Event Method to call and pass an int 
    public void OnIntEventTriggered(int parameter)
    {
        debuggerNiAin.Log(
            $"Event call from {gameEvent} and {parameter} was received from {this.gameObject.name}. \n" +
            $"All assigned methods in 'onIntEventTriggered' were executed successfully!"
        );

        // Invokes the UnityEvent, which can be configured in the Inspector to call any public method on any script
        onIntEventTriggered.Invoke(parameter);
    }

    // Event Method to call and pass on float 
    public void OnFloatEventTriggered(float parameter)
    {
        debuggerNiAin.Log(
            $"Event call from {gameEvent} and {parameter} was received from {this.gameObject.name}. \n" +
            $"All assigned methods in 'onFloatEventTriggered' were executed successfully!"
        );

        // Invokes the UnityEvent, which can be configured in the Inspector to call any public method on any script
        onFloatEventTriggered.Invoke(parameter);
    }

    // Event Method to call and pass a bool 
    public void OnBoolEventTriggered(bool parameter)
    {
        debuggerNiAin.Log(
            $"Event call from {gameEvent} and {parameter} was received from {this.gameObject.name}. \n" +
            $"All assigned methods in 'onBoolEventTriggered' were executed successfully!"
        );

        // Invokes the UnityEvent, which can be configured in the Inspector to call any public method on any script
        onBoolEventTriggered.Invoke(parameter);
    }

    #endregion  
}