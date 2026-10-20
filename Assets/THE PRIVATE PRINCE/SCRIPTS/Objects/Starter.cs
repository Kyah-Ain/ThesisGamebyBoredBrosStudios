using System;
using System.Collections;
using UnityEngine;

using UnityEngine.Events;
using UnityEngine.InputSystem;

public class Starter : MonoBehaviour, IInteractable
{
    // ------------------------- LOCAL ENUM -------------------------

    enum StarterType
    {
        TriggerManually,
        TriggerOnAwake,
        TriggerOnEnable,
        TriggerOnStart,
        TriggerOnEnterCollider,
        TriggerOnInteract
    }
    
    enum UseType
    {
        UseAndDontDisable,
        UseOnceAndDisable,
        UseOnceAndDestroy
    }

    // ------------------------- VARIABLES -------------------------
    [Header("COLLIDER EVENTS")]
    [Space]
    [SerializeField] bool requiresInteractInput;
    public UnityEvent onStart;
    [Space]
    public UnityEvent onTriggerEnter;
    [Space]
    public UnityEvent onTriggerExit;
    
    [Header("SETTINGS")]
    [SerializeField] StarterType starterType = StarterType.TriggerManually;
    [SerializeField] UseType useType = UseType.UseAndDontDisable;
    [SerializeField] bool isResettable = true;
    [SerializeField] bool addDelay = true;
    [SerializeField][Range(0f,10f)] float delayTime; 

    [Header("TRIGGER ENTER SETTINGS")]
    [SerializeField] string requiredTag = "Player"; // Only objects with this tag can trigger this (leave empty to accept any object)
    
    [Header("STATUS")]
    [SerializeField][ReadOnly] bool _isToggledAlready;
    [SerializeField][ReadOnly] bool _isPlayerInRange;
    
    // Remembers that Awake already triggered, so Start doesn't fire a second time
    private bool _hasTriggeredOnAwake;
    
    // ----------------------- UNITY METHODS ------------------------
    #region UNITY METHODS
    
    // Awake is called when this script was first initialized & loaded
    void Awake()
    {
        // Proceeds only if the Trigger type was set to Awake and hasn't been toggled yet
        if (starterType == StarterType.TriggerOnAwake && !_isToggledAlready)
        {
            _hasTriggeredOnAwake = true;
            TriggerStart();
        }
    }
    
    // OnEnable is called when the object becomes enabled and active
    void OnEnable()
    {
        // Proceeds only if the Trigger type was set to OnEnable and hasn't been toggled yet
        if (starterType == StarterType.TriggerOnEnable && !_isToggledAlready)
        {
            _hasTriggeredOnAwake = true;
            TriggerStart();
        }
        
        // Subscribes the method to an Input Event
        GameEventsManager.Instance.inputEvents.onInteract += OnInteractInput;
    }
    
    // OnDisable is called when the object becomes disabled
    void OnDisable()
    {
        // UnSubscribes the method to an Input Event
        GameEventsManager.Instance.inputEvents.onInteract -= OnInteractInput;
        
        // Only proceed if it's resettable
        if (isResettable)
        {
            Reset();
        }
    }

    // Start is called once before the first frame update
    void Start()
    {
        // Proceeds only if the Trigger type was set to OnStart and hasn't been toggled yet
        if (starterType == StarterType.TriggerOnStart && !_isToggledAlready)
        {
            TriggerStart();
        }
    }

    // OnTriggerEnter is called when another collider enters this object's trigger collider
    void OnTriggerEnter(Collider actor)
    {
        /* NOTE:
            This object needs a Collider with 'Is Trigger' checked, and either this object
            or the object entering needs a Rigidbody, otherwise this will never be called.
        */
        
        // Proceeds only if the Trigger type was set to OnEnterCollider
        if (starterType != StarterType.TriggerOnEnterCollider) return;

        // If a tag is required, ignore any object that doesn't have it
        if (!string.IsNullOrEmpty(requiredTag) && !actor.CompareTag(requiredTag)) return;

        // Prompts that the player entered the Collider
        _isPlayerInRange = true;
        
        // Executes OnTrigger event
        onTriggerEnter?.Invoke();
        
        // Prevents the Trigger to start the Starter immediately to wait for Input instead
        if (requiresInteractInput) return;

        // Executes the event
        TriggerStart();
    }
    
    // OnTriggerEnter is called when another collider exits this object's trigger collider
    void OnTriggerExit(Collider actor)
    {
        // Proceeds only if the Trigger type was set to OnEnterCollider
        if (starterType != StarterType.TriggerOnEnterCollider) return;

        // If a tag is required, ignore any object that doesn't have it
        if (!string.IsNullOrEmpty(requiredTag) && !actor.CompareTag(requiredTag)) return;

        // Prompts that the player entered the Collider
        _isPlayerInRange = false;
        
        onTriggerExit?.Invoke();
    }

    #endregion
    
    // --------------------- STARTER METHODS ---------------------
    #region STARTER METHODS
    
    // Method to call for starting the Starter
    public void TriggerStart()
    {
        // Flags that this script has been toggled already
        _isToggledAlready = true;
        
        StartCoroutine(StartWithDelay());
    }
    
    // Method to reset the Starter
    public void Reset()
    {
        // Reset's the Player status when this was disabled while it was set to true
        _isPlayerInRange = false;
        
        // Reset to the state before Entering this gameObject
        onTriggerExit?.Invoke();
    }

    #endregion
    
    // ------------------- EVENT SUBSCRIPTIONS --------------------
    #region EVENT SUBSCRIPTIONS
    
    // New Input System Method that are called when subscribed to an Input Event
    void OnInteractInput(InputAction.CallbackContext context)
    {
        // Only register Input when the Player was in the object's Interactable Range
        if (_isPlayerInRange)
        {
            TriggerStart();
            onTriggerEnter?.Invoke();
        }
    }

    #endregion
    
    // ------------------ INTERFACE APPLICATIONS --------------------
    #region INTERFACE METHODS

    // Overload Method to call for Interacting an object through Interface
    public void Interacted()
    {
        // Proceeds only if the Trigger type was set to OnInteract
        if (starterType != StarterType.TriggerOnInteract) return;

        TriggerStart();
    }
    
    // Overload Method to call for Un-Interacting an object using Interface
    public void UnInteracted()
    {
        // Necessary to Implement due to Interface rule,
        // but since I have no use to it yet,
        // I could just implement it and leave it blank.
    }
    
    #endregion  
    
    // ----------------------- CUSTOM METHODS -------------------------

    // Coroutine Method to process a delay
    IEnumerator StartWithDelay()
    {
        // Pauses the execution with the amount of time passed in the parameter
        // (only if the delay is enabled and greater than zero)
        // NOTE: WaitForSeconds is affected by Time.timeScale. Use WaitForSecondsRealtime
        // instead if this should still run while the game is paused.
        if (addDelay && delayTime > 0f)
        {
            yield return new WaitForSeconds(delayTime);
        }
                
        // Broadcast the event to anyone subscribing or listening
        onStart.Invoke();
        
        // Proceed only if single used & disable was chosen
        if (useType == UseType.UseOnceAndDisable)
        {
            // Reset the flag only when the object is kept, so it can be re-used.
            // When destroying, the flag stays true so it can't be re-triggered during the extra wait.
            _isToggledAlready = false;
            
            // Disable this object after using
            this.gameObject.SetActive(false);
        }
        // Proceed only if single used & destroy was chosen
        else if (useType == UseType.UseOnceAndDestroy)
        {
            // Additional pause to wait for every logic to be executed before destroying this object
            yield return new WaitForSeconds(1f);
            
            // Destroy or wipe the instance of this script's gameObject entirely
            Destroy(gameObject);
        }
        // Default fallback if conditions above haven't met
        else if (useType == UseType.UseAndDontDisable)
        {
            // Reset the flag only when the object is kept, so it can be re-used.
            // When destroying, the flag stays true so it can't be re-triggered during the extra wait.
            _isToggledAlready = false;
        }
    }
}