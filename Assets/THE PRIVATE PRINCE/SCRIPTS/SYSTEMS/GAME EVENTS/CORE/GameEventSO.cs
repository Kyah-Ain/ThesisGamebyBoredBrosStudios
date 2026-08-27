using System.Collections.Generic;
using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "GameEvent", menuName = "EventObjects/GameEvent")]
public class GameEvent : ScriptableObject
{
    // ------------------------- VARIABLES -------------------------

    // List of listeners that will respond to this event when it's raised
    [SerializeField] List<GameEventListener> listeners = new List<GameEventListener>();

    // --------------------- SUBSCRIPTION METHODS -------------------------

    // Method to subscribes other script to the 'listeners' list
    public void AddListener(GameEventListener listener)
    {
        // Subscribing the script that references this method
        listeners.Add(listener);
    }

    // Method to unsubscribes other script to the 'listeners' list
    public void RemoveListener(GameEventListener listener)
    {
        // Unsubscribing the script that references this method
        listeners.Remove(listener);
    }

    // ----------------------- EXECUTE METHODS -------------------------

    // Method that triggers a call to an event of all 'VOID' method subscribers
    public void TriggerEvent()
    {
        // Calls the 'OnEventTriggered()' method in every script that is subscribed to this event
        for (int i = listeners.Count - 1; i >= 0; i--)
        {
            listeners[i].OnEventTriggered();
        }
    }

    // ------------------------- EXTENSIONS -------------------------

    #region EXTENSIONS METHODS

    // OVERLOAD Method that triggers all 'STRING' method subscribers
    public void TriggerEvent(string parameter)
    {
        // Calls the 'OnEventTriggered()' method in every script that is subscribed to this event
        for (int i = listeners.Count - 1; i >= 0; i--)
        {
            listeners[i].OnStringEventTriggered(parameter);
        }
    }

    // OVERLOAD Method that triggers all 'INT' method subscribers
    public void TriggerEvent(int parameter)
    {
        // Calls the 'OnEventTriggered()' method in every script that is subscribed to this event
        for (int i = listeners.Count - 1; i >= 0; i--)
        {
            listeners[i].OnIntEventTriggered(parameter);
        }
    }

    // OVERLOAD Method that triggers all 'FLOAT' method subscribers
    public void TriggerEvent(float parameter)
    {
        // Calls the 'OnEventTriggered()' method in every script that is subscribed to this event
        for (int i = listeners.Count - 1; i >= 0; i--)
        {
            listeners[i].OnFloatEventTriggered(parameter);
        }
    }

    // OVERLOAD Method that triggers all 'BOOL' method subscribers
    public void TriggerEvent(bool parameter)
    {
        // Calls the 'OnEventTriggered()' method in every script that is subscribed to this event
        for (int i = listeners.Count - 1; i >= 0; i--)
        {
            listeners[i].OnBoolEventTriggered(parameter);
        }
    }

    #endregion  
}