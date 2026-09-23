using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputEvents
{
    // ------------------------- EVENTS -------------------------
    [field: Header("PLAYER EVENTS")]
    public event Action<InputAction.CallbackContext> onInteract;
    public event Action<InputAction.CallbackContext> onMovement;

    [field: Header("UI EVENTS")]
    public event Action<InputAction.CallbackContext> onSubmit;
    public event Action<InputAction.CallbackContext> onNavigate;
    public event Action<InputAction.CallbackContext> onCancel;
    
    // ------------------------- VARIABLES -------------------------
    
    [Header("PLAYER ACTIONS")]
    private readonly InputAction interactAction;
    private readonly InputAction movementAction;
    
    [Header("UI ACTIONS")]
    private readonly InputAction submitAction;
    private readonly InputAction navigateAction;
    private readonly InputAction cancelAction;
    
    // ----------------------- CONSTRUCTOR -------------------------

    public InputEvents(PrivatePrinceControls ppControls)
    {
        #region INPUT ACTIONS
            // INPUT ACTIONS - stores the specific Input Action we want to monitor
        
            // ------------------- PLAYER ACTIONS -------------------
            interactAction = ppControls.Player.Interact;
            movementAction = ppControls.Player.Move;
            
            // -------------------- UI ACTIONS --------------------
            submitAction = ppControls.UI.Submit;
            navigateAction = ppControls.UI.NavigateUI;
            cancelAction = ppControls.UI.Cancel;

        #endregion

        #region EVENT TRIGGERS
            // EVENT TRIGGERS - subscribe methods to the corresponding inputs set in New Input System
            // CALLER += LISTENERS

            // --------------- PLAYER EVENT TRIGGER -----------------
            interactAction.performed += OnInteract;
            movementAction.performed += OnMovement;
            movementAction.canceled += OnMovement;
            
            // ----------------- UI EVENT TRIGGER ------------------
            submitAction.performed += SubmitPress;
            navigateAction.performed += OnNavigate;
            cancelAction.performed += OnCancel;

        #endregion
    }

    // ------------------------ TRIGGERS -------------------------
    // Methods that are automatically called by Unity's Input System
    
    #region for PLAYER
    
        void OnInteract(InputAction.CallbackContext context)
        {
            // Broadcast the input to every subscribed script
            onInteract?.Invoke(context);
        }
        
        void OnMovement(InputAction.CallbackContext context)
        {
            // Broadcast the input to every subscribed script
            onMovement?.Invoke(context);
        }

    #endregion

    #region for UI
    
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

    #endregion
    
    
    // ------------------------- CLEANUP -------------------------
    #region CLEANUP
    
    // Method to UnSubscribe to InputEvents
    public void Dispose()
    {
        // Stop listening to the Input System
        interactAction.performed -= OnInteract;
        movementAction.performed -= OnMovement;
        movementAction.canceled -= OnMovement;
        
        submitAction.performed -= SubmitPress;
        navigateAction.performed -= OnNavigate;
        cancelAction.performed -= OnCancel;
    }

    #endregion
}
