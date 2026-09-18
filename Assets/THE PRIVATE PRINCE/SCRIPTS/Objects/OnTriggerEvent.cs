using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Events;

public class OnTriggerEvent : MonoBehaviour
{
    // ------------------------- VARIABLES -------------------------
    [Header("COLLIDER EVENTS")]
    public UnityEvent onCollide;
    
    // ----------------------- UNITY METHODS -------------------------
    #region UNITY METHODS
    
    void OnTriggerEnter(Collider actor)
    {
        if (actor.CompareTag("Player"))
        {
            // onCollide?.Invoke();
        }
    }
    
    void OnTriggerExit(Collider actor)
    {
        if (actor.CompareTag("Player"))
        {
            
        }
    }

    void Start()
    {
        onCollide?.Invoke();
    }

    #endregion
}
