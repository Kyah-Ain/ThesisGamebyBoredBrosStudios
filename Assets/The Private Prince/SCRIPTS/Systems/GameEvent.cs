using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameEvent", menuName = "EventObjects/GameEvent")]
public class GameEvent : ScriptableObject
{
    // ------------------------- VARIABLES -------------------------

    private List<GameEventListener> listeners = new List<GameEventListener>();

    // -------------------------- METHODS -------------------------

    // ...
    public void TriggerEvent() 
    {
        // ...
        for (int i = listeners.Count - 1; i >= 0; i--) 
        {
            // ...
            listeners[i].OnEventTriggered();
        }
    }

    // ...
    public void AddListener(GameEventListener listener) 
    {
        // ...
        listeners.Add(listener);
    }

    // ...
    public void RemoveListener(GameEventListener listener)
    {
        // ...  
        listeners.Remove(listener);
    }
}