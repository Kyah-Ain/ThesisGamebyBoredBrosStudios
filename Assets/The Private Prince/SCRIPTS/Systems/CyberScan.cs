using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Events;
using UnityEngine.InputSystem;

public class CyberScan : MonoBehaviour
{
    // ------------------------- VARIABLES -------------------------

    [Header("REFERENCES")]
    [SerializeField] DebuggerNiAinPjls debuggerNiAin; // Custom debugging script from your dev Ain
    
    [Header("EVENT")] // The event that fired locally at the Inspector
    [SerializeField] private UnityEvent onCyberScanning;  
    [SerializeField] private UnityEvent onUnCyberScanning;
    
    [Header("STATUS")]
    [SerializeField][ReadOnly] bool cyberStatus; // Status to track for the state of the CyberScan

    // ------------------------- UNITY METHODS -------------------------
    #region UNITY METHODS
    
    // Awake is called when this script was first initialized & loaded
    private void Awake()
    {
        // Checks if our reference was null or not been set
        if (debuggerNiAin == null)
        {
            // If it is not, then set it automatically by looking for the script class from this object
            debuggerNiAin = this.GetComponent<DebuggerNiAinPjls>();
        }
    }

    // OnEnable is called when the object becomes enabled and active
    private void OnEnable()
    {
        Subscribe();
    }

    // OnDisable is called when the object becomes disabled
    private void OnDisable()
    {
        UnSubscribe();
    }
    
    #endregion
    
    // ----------------------- EVENT METHODS -------------------------
    #region EVENT METHODS

    // Method to subscribe your local method to an event trigger
    protected virtual void Subscribe()
    {
        // Set subscriptions of these methods to an event
        // Left (Event Call) += Right (Method that would be called)
        GameEventsManager.Instance.inputEvents.onCyberScan += PerformCyberScan;
    }

    // Method to UnSubscribe your local method to an event trigger
    protected virtual void UnSubscribe()
    {
        // UnSubscribe them methods from an event
        // Left (Event Call) -= Right (Method that would be called)
        GameEventsManager.Instance.inputEvents.onCyberScan -= PerformCyberScan;
    }
    
    #endregion

    // ------------------------- CYBERSCAN METHODS -------------------------
    
    // Method to perform CyberScanning
    void PerformCyberScan(InputAction.CallbackContext context)
    {
        // Toggles CyberScan's status per each call
        cyberStatus = !cyberStatus;
        
        // Only register when CyberStatus set to true
        if (cyberStatus)
        {
            // Invoke the Event if it's not null (at least have one Subscriber/Listener)
            onCyberScanning?.Invoke();
        }
        // Only register when CyberStatus set to false
        else
        {
            // Invoke the Event if it's not null (at least have one Subscriber/Listener)
            onUnCyberScanning?.Invoke();
        }
    } 
}