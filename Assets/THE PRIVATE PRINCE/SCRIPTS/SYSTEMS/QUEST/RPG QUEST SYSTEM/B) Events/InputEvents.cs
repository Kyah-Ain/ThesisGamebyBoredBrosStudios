using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputEvents
{
    // ------------------------- EVENTS -------------------------
    
    [field: Header("PLAYER EVENTS")]
    public event Action<InputAction.CallbackContext> onMovement;
    public event Action<InputAction.CallbackContext> onInteract;
    public event Action<InputAction.CallbackContext> onCyberScan;
    public event Action<InputAction.CallbackContext> onSprint;

    [field: Header("UI EVENTS")]
    public event Action<InputAction.CallbackContext> onSubmit;
    public event Action<InputAction.CallbackContext> onNavigate;
    public event Action<InputAction.CallbackContext> onRestart;
    public event Action<InputAction.CallbackContext> onCancel;
    
    [field: Header("PUZZLE EVENTS")]
    public event Action<InputAction.CallbackContext> onPuzzleSubmit;
    
    // ------------------------- VARIABLES -------------------------
    
    [Header("PLAYER ACTIONS")]
    private readonly InputAction movementAction;
    private readonly InputAction interactAction;
    private readonly InputAction cyberScanAction;
    private readonly InputAction sprintAction;
    
    [Header("UI ACTIONS")]
    private readonly InputAction submitAction;
    private readonly InputAction navigateAction;
    private readonly InputAction restartAction;
    private readonly InputAction cancelAction;
    
    [Header("PUZZLE ACTIONS")]
    private readonly InputAction puzzleSubmit;
    
    // ----------------------- CONSTRUCTOR -------------------------

    public InputEvents(PrivatePrinceControls ppControls)
    {
        #region INPUT ACTIONS
            // INPUT ACTIONS - stores the specific Input Action we want to monitor
        
            // ------------------- PLAYER ACTIONS -------------------
            movementAction = ppControls.Player.Move;
            interactAction = ppControls.Player.Interact;
            cyberScanAction = ppControls.Player.Cyberscan;
            sprintAction = ppControls.Player.Sprint;
            
            // -------------------- UI ACTIONS --------------------
            submitAction = ppControls.UI.Submit;
            navigateAction = ppControls.UI.NavigateUI;
            restartAction = ppControls.UI.Restart;
            cancelAction = ppControls.UI.Cancel;
            
            // -------------------- PUZZLE ACTIONS --------------------
            puzzleSubmit = ppControls.UI.PuzzleSubmit;
            
        #endregion

        #region EVENT TRIGGERS
            // EVENT TRIGGERS - subscribe methods to the corresponding inputs set in New Input System
            // CALLER += LISTENERS

            // --------------- PLAYER EVENT TRIGGER -----------------
            interactAction.performed += OnInteract;
            movementAction.performed += OnMovement;
            sprintAction.performed += OnSprint;
            movementAction.canceled += OnMovement;
            cyberScanAction.canceled += OnCyberScan;
            sprintAction.canceled += OnSprint;
            
            // ----------------- UI EVENT TRIGGER ------------------
            submitAction.performed += SubmitPress;
            navigateAction.performed += OnNavigate;
            navigateAction.canceled += OnNavigate;
            restartAction.performed += OnRestart;
            cancelAction.performed += OnCancel;
            
            // ---------------- PUZZLE EVENT TRIGGER ------------------
            puzzleSubmit.performed += SubmitPuzzlePress;

        #endregion
    }

    // ------------------------ TRIGGERS -------------------------
    // Methods that are automatically called by Unity's Input System
    
    #region for PLAYER
    
        void OnMovement(InputAction.CallbackContext context)
        {
            // Broadcast the input to every subscribed script
            onMovement?.Invoke(context);
        }
        
        void OnSprint(InputAction.CallbackContext context)
        {
            onSprint?.Invoke(context);
        }
    
        void OnInteract(InputAction.CallbackContext context)
        {
            // Broadcast the input to every subscribed script
            onInteract?.Invoke(context);
        }
        
        void OnCyberScan(InputAction.CallbackContext context)
        {
            // Broadcast the input to every subscribed script
            onCyberScan?.Invoke(context);
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
        
        void OnRestart(InputAction.CallbackContext context)
        {
            // Broadcast the input to every subscribed script
            onRestart?.Invoke(context);
        }
        
        void OnCancel(InputAction.CallbackContext context)
        {
            // Broadcast the input to every subscribed script
            onCancel?.Invoke(context);
        }

    #endregion
    
    #region for PUZZLE
    
    void SubmitPuzzlePress(InputAction.CallbackContext context)
    {
        // Broadcast the input to every subscribed script
        onPuzzleSubmit?.Invoke(context);
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
        sprintAction.performed -= OnSprint;
        movementAction.canceled -= OnMovement;
        sprintAction.canceled -= OnSprint;
        cyberScanAction.canceled -= OnCyberScan;
        
        submitAction.performed -= SubmitPress;
        navigateAction.performed -= OnNavigate;
        navigateAction.canceled -= OnNavigate;
        restartAction.performed -= OnRestart;
        cancelAction.performed -= OnCancel;
        
        puzzleSubmit.performed -= SubmitPuzzlePress;
    }

    #endregion
}
