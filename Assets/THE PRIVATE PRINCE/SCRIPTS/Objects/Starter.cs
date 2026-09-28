using System.Collections;
using UnityEngine;

using UnityEngine.Events;

public class Starter : MonoBehaviour
{
    // ------------------------- VARIABLES -------------------------
    [Header("COLLIDER EVENTS")]
    public UnityEvent onStart;
    
    [Header("SETTINGS")]
    [SerializeField] bool enableTriggerOnAwake;
    [SerializeField] bool enableTriggerOnStart;
    [SerializeField] bool enableTriggerOnTriggerEnter;
    [SerializeField] bool enableUseOnceAndDestroy;
    [Space]
    [SerializeField] bool addDelay = true;
    [SerializeField][Range(0f,10f)] float delayTime; 

    [Header("TRIGGER ENTER SETTINGS")]
    // Only objects with this tag can trigger the Starter (leave empty to accept any object)
    [SerializeField] string requiredTag = "Player";
    
    [Header("STATUS")]
    [SerializeField][ReadOnly] bool _isToggledAlready;

    // Remembers that Awake already triggered, so Start doesn't fire a second time
    private bool _hasTriggeredOnAwake;
    
    // ----------------------- UNITY METHODS ------------------------
    #region UNITY METHODS
    
    // Awake is called when this script was first initialized & loaded
    private void Awake()
    {
        // ...
        // NOTE: Other scripts may not have run their own Awake/Start yet at this point,
        // so listeners that depend on their own initialization may hit null references.
        // If that happens, use Start instead (or add a one-frame delay).
        if (enableTriggerOnAwake && !_isToggledAlready)
        {
            _hasTriggeredOnAwake = true;
            TriggerStart();
        }
    }
    
    // Start is called once before the first frame update
    void Start()
    {
        // ...
        // Skipped if Awake already triggered, to avoid firing twice when both options are enabled
        if (enableTriggerOnStart && !_isToggledAlready && !_hasTriggeredOnAwake)
        {
            TriggerStart();
        }
    }

    // OnTriggerEnter is called when another collider enters this object's trigger collider
    // NOTE: This object needs a Collider with 'Is Trigger' checked, and either this object
    // or the object entering needs a Rigidbody, otherwise this will never be called.
    private void OnTriggerEnter(Collider other)
    {
        // Ignore if this option is disabled or the Starter is still busy (not yet reset)
        if (!enableTriggerOnTriggerEnter || _isToggledAlready) return;

        // If a tag is required, ignore any object that doesn't have it
        if (!string.IsNullOrEmpty(requiredTag) && !other.CompareTag(requiredTag)) return;

        TriggerStart();
    }

    // Method to call for starting the Starter
    public void TriggerStart()
    {
        /*
            Flips the flag to indicate that it has been used &
            that it needs to be reset to be able to be re-used
        */ 
        _isToggledAlready = true;
        
        StartCoroutine(StartWithDelay());
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
        
        // Proceed only if single used was toggled true or was enabled
        if (enableUseOnceAndDestroy)
        {
            // Additional pause to wait for every logic to be executed before destroying this object
            yield return new WaitForSeconds(1f);
            
            // Destroy or wipe the instance of this script's gameObject entirely
            Destroy(gameObject);
        }
        else
        {
            // Reset the flag only when the object is kept, so it can be re-used.
            // When destroying, the flag stays true so it can't be re-triggered during the extra wait.
            _isToggledAlready = false;
        }
    }
}