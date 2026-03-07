using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Events;

public class GameEventListener : MonoBehaviour
{
    // -------------------------- EVENTS -------------------------

    public GameEvent gameEvent; // Placeholder for the Event Trigger Scriptable Object
    
    [Space]

    public UnityEvent onEventTriggered; // Event method to be fired

    // -------------------------- METHODS -------------------------

    // ...
    void OnEnable()
    {
        gameEvent.AddListener(this);
    }

    // ...
    void OnDisable()
    {
        gameEvent.RemoveListener(this);
    }

    // ...
    public void OnEventTriggered() 
    {
        onEventTriggered.Invoke();
    }
}