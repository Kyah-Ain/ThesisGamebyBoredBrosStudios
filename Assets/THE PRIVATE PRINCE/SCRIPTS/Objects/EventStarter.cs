using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Events;

public class EventStarter : MonoBehaviour
{
    // ------------------------- VARIABLES -------------------------
    [Header("COLLIDER EVENTS")]
    public UnityEvent onCollide;
    
    [Header("SETTINGS")]
    [SerializeField] bool enableUseOnceAndDestroy;
    [Space]
    [SerializeField] bool addDelay = true;
    [SerializeField][Range(0f,10f)] float delayTime; 
    
    [Header("STATUS")]
    private bool _isToggledAlready;
    
    // ----------------------- UNITY METHODS -------------------------
    #region UNITY METHODS
    
    // OnTriggerEnter is called once per collision
    void OnTriggerEnter(Collider actor)
    {
        // Only register collisions from gameObjects tagged as "Player"
        if (actor.CompareTag("Player"))
        {
            // Only executes the logic inside if it hasn't been executed yet
            if (!_isToggledAlready)
            {
                StartCoroutine(StartEvent());
            }
        }
    }
    
    // OnTriggerExit is called once per exiting collision
    void OnTriggerExit(Collider actor)
    {
        // Only register collisions from gameObjects tagged as "Player"
        if (actor.CompareTag("Player"))
        {
            /*
                Flips the flag to indicate that it has been reset &
                now available to be re-used
            */ 
            _isToggledAlready = false;
        }
    }

    #endregion
    
    // ----------------------- CUSTOM METHODS -------------------------

    // Coroutine Method to process a delay
    IEnumerator StartEvent()
    {
        // Pauses the execution with the amount of time passed in the parameter
        yield return new WaitForSeconds(delayTime);
        
        /*
            Flips the flag to indicate that it has been used &
            that it needs to be reset to be able to be re-used
        */ 
        _isToggledAlready = true;
                
        // Broadcast the event to anyone subscribing or listening
        onCollide?.Invoke();
        
        // Proceed only if single used was toggled true or was enabled
        if (enableUseOnceAndDestroy)
        {
            // Additional pause to wait for every logic to be executed before destroying this object
            yield return new WaitForSeconds(1f);
            
            // Destroy or wipe the instance of this script's gameObject entirely
            Destroy(this.gameObject);
        }
    }
}
