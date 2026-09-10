using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputEvents
{
    // ------------------------- VARIABLES -------------------------
    
    [Header("INPUT ACTIONS")]
    private readonly InputAction submitAction;
    private readonly InputAction proceedAction;
    private readonly InputAction cancelAction;
    
    // ------------------------- EVENTS -------------------------

    public event Action<InputAction.CallbackContext> onSubmitPressed;
    public event Action<InputAction.CallbackContext> onProceed;
    public event Action<InputAction.CallbackContext> onCancel;
    
    // ----------------------- CONSTRUCTOR -------------------------

    public InputEvents(PrivatePrinceControls ppControls)
    {
        // INPUT ACTIONS
        // Store the specific Input Action we want to monitor
        // ------------------- PLAYER ACTIONS -------------------
        submitAction = ppControls.Player.Interact;
        
        // -------------------- UI ACTIONS --------------------
        proceedAction = ppControls.UI.Proceed;
        cancelAction = ppControls.UI.Cancel;
        
        // CALLER += LISTENERS
        // Subscribes methods to the corresponding inputs set in New Input System
        // ------------------- PLAYER ACTIONS -------------------
        submitAction.performed += SubmitPress;
        
        // -------------------- UI ACTIONS --------------------
        proceedAction.performed += OnProceed;
        cancelAction.performed += OnCancel;
    }

    // ------------------------ TRIGGERS -------------------------
    // Methods that are automatically called by Unity's Input System
    
    void SubmitPress(InputAction.CallbackContext context)
    {
        // Broadcast the input to every subscribed script
        onSubmitPressed?.Invoke(context);
    }
    
    void OnProceed(InputAction.CallbackContext context)
    {
        // Broadcast the input to every subscribed script
        onProceed?.Invoke(context);
    }
    
    void OnCancel(InputAction.CallbackContext context)
    {
        // Broadcast the input to every subscribed script
        onCancel?.Invoke(context);
    }
    
    // ------------------------- CLEANUP -------------------------
    #region CLEANUP
    
    // Method to UnSubscribe to InputEvents
    public void Dispose()
    {
        // Stop listening to the Input System
        submitAction.performed -= SubmitPress;
        proceedAction.performed -= OnProceed;
        cancelAction.performed -= OnCancel;
    }

    #endregion
}
