using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputEvents
{
    // ------------------------- VARIABLES -------------------------
    
    [Header("PLAYER ACTIONS")]
    private readonly InputAction interactAction;
    
    [Header("UI ACTIONS")]
    private readonly InputAction submitAction;
    private readonly InputAction navigateAction;
    private readonly InputAction cancelAction;
    
    // ------------------------- EVENTS -------------------------
    public event Action<InputAction.CallbackContext> onInteract;

    public event Action<InputAction.CallbackContext> onSubmit;
    public event Action<InputAction.CallbackContext> onNavigate;
    public event Action<InputAction.CallbackContext> onCancel;
    
    // ----------------------- CONSTRUCTOR -------------------------

    public InputEvents(PrivatePrinceControls ppControls)
    {
        // INPUT ACTIONS
        // Store the specific Input Action we want to monitor
        // ------------------- PLAYER ACTIONS -------------------
        interactAction = ppControls.Player.Interact;
        
        // -------------------- UI ACTIONS --------------------
        submitAction = ppControls.UI.Submit;
        navigateAction = ppControls.UI.NavigateUI;
        cancelAction = ppControls.UI.Cancel;
        
        // CALLER += LISTENERS
        // Subscribes methods to the corresponding inputs set in New Input System
        // ------------------- PLAYER ACTIONS -------------------
        interactAction.performed += OnInteract;
        
        // -------------------- UI ACTIONS --------------------
        submitAction.performed += SubmitPress;
        navigateAction.performed += OnNavigate;
        cancelAction.performed += OnCancel;
    }

    // ------------------------ TRIGGERS -------------------------
    // Methods that are automatically called by Unity's Input System
    void OnInteract(InputAction.CallbackContext context)
    {
        // Broadcast the input to every subscribed script
        onInteract?.Invoke(context);
    }
    
    void SubmitPress(InputAction.CallbackContext context)
    {
        // Broadcast the input to every subscribed script
        onSubmit?.Invoke(context);
    }
    
    void OnNavigate(InputAction.CallbackContext context)
    {
        // Broadcast the input to every subscribed script
        onNavigate?.Invoke(context);
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
        interactAction.performed -= OnInteract;
        submitAction.performed -= SubmitPress;
        navigateAction.performed -= OnNavigate;
        cancelAction.performed -= OnCancel;
    }

    #endregion
}
