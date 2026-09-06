using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.InputSystem;
using UnityEngine.Events;

// Required DebuggerNiAinPjls.cs for this to be able to monitor debugs, otherwise use the old one
[RequireComponent(typeof(DebuggerNiAinPjls))]
public class DialogueForNPC : MonoBehaviour
{
    // ------------------------- VARIABLES -------------------------
    
    [Header("EVENTS")]
    public UnityEvent onNPCTalk;
    
    [Header("REFERENCES")]
    [SerializeField] DebuggerNiAinPjls debuggerNiAin; // Custom debugging script from your dev Ain
    [SerializeField] private DialogueInfoSO dialogueInfo;
    
    [Header("STATUS")]
    [SerializeField] private bool isPlayerNear;
    
    // ----------------------- UNITY METHODS -------------------------
    #region UNITY METHODS
    
    // Awake is called when this script was first initialized & loaded
    private void Awake()
    {
        // Checks if our reference for the script was not set
        if (debuggerNiAin == null)
        {
            // If it is not, then set it automatically by looking for the script class from this object
            debuggerNiAin = this.GetComponent<DebuggerNiAinPjls>();
        }
    }

    // OnEnable is called when the object becomes enabled and active
    void OnEnable()
    {
        Subscribe();
    }

    // OnDisable is called when the object becomes disabled
    void OnDisable()
    {
        UnSubscribe();
    }
    
    // OnTriggerEnter is called when this script's object collide with another object 
    private void OnTriggerEnter(Collider actor)
    {
        // Proceeds inside the block only if this collided with a Player
        if (actor.CompareTag("Player"))
        {
            isPlayerNear = true;
        }
    }
    
    // OnTriggerExit is called when this script's object un-collide with another object
    private void OnTriggerExit(Collider actor)
    {
        // Proceeds inside the block only if this collided with a Player
        if (actor.CompareTag("Player"))
        {
            isPlayerNear = false;
        }
    }

    #endregion
    
    // ------------------------- SUBSCRIPTIONS -------------------------
    #region SUBSCRIPTIONS

    // Method to subscribe your local method to an event trigger
    void Subscribe()
    {
        // Set subscriptions of these methods to an event
        // Left (Event Call) += Right (Method that would be called)
        GameEventsManager.Instance.inputEvents.onSubmitPressed += Talk;
    }

    // Method to UnSubscribe your local method to an event trigger
    void UnSubscribe()
    {
        // UnSubscribe them methods to an event
        // Left (Event Call) -= Right (Method that would be called)
        GameEventsManager.Instance.inputEvents.onSubmitPressed -= Talk;
    }
    
    #endregion
    
    // ----------------------- NPC METHODS -------------------------
    #region NPC METHODS

    // Method to call for making the NPC talk
    void Talk(InputAction.CallbackContext context)
    {
        // Proceeds only if the player is in the interaction zone 
        if (isPlayerNear)
        {
            // Triggers all triggerable included under this Event Array in the Inspector
            onNPCTalk?.Invoke();
        }
    }

    #endregion
    
    // ------------------------ DEBUGGERS -------------------------
    #region DEBUGGERS

    // Method to call for debugging a dialogue for this NPC
    void DebugDialogue()
    {
        // Prints all the dialogues in the console
        foreach (string dialogueLines in dialogueInfo.DialogueLines.HasRequestLines)
        {
            debuggerNiAin.Log(dialogueLines);
        }
            
        foreach (string dialogueLines in dialogueInfo.DialogueLines.IdleLines)
        {
            debuggerNiAin.Log(dialogueLines);
        }
            
        foreach (string dialogueLines in dialogueInfo.DialogueLines.WaitingForCompletionLines)
        {
            debuggerNiAin.Log(dialogueLines);
        }
            
        foreach (string dialogueLines in dialogueInfo.DialogueLines.CanFinishRequestLines)
        {
            debuggerNiAin.Log(dialogueLines);
        }
    }

    #endregion
}