using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputEvents
{
    // ------------------------- VARIABLES -------------------------
    
    [Header("INPUT ACTIONS")]
    private readonly InputAction submitAction;
    
    // ------------------------- EVENTS -------------------------

    public event Action<InputAction.CallbackContext> onSubmitPressed;
    
    // ----------------------- CONSTRUCTOR -------------------------

    public InputEvents(PrivatePrinceControls ppControls)
    {
        // INPUT ACTIONS
        // Store the specific Input Action we want to monitor
        submitAction = ppControls.Player.Interact;
        
        // CALLER += LISTENERS
        // Subscribes methods to the corresponding inputs set in New Input System
        submitAction.performed += SubmitPress;
    }

    // ------------------------ TRIGGERS -------------------------

    // Method that automatically called by Unity's Input System
    public void SubmitPress(InputAction.CallbackContext context)
    {
        // Broadcast the input to every subscribed script
        onSubmitPressed?.Invoke(context);
    }
    
    // ------------------------- CLEANUP -------------------------
    
    // Method to UnSubscribe to InputEvents
    public void Dispose()
    {
        // Stop listening to the Input System
        submitAction.performed -= SubmitPress;
    }
}
