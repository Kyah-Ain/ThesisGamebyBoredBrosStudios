using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Events;
// using static UnityEngine.Rendering.DebugUI;

public class InteractableObject : MonoBehaviour, IInteractable
{
    // ------------------------- VARIABLES -------------------------

    [Header("EVENTS")]
    [SerializeField] UnityEvent onTriggerEnter;
    [SerializeField] UnityEvent onTriggerExit;
    [SerializeField] UnityEvent onInteract;
    [SerializeField] UnityEvent onUnInteract;
    
    [Header("SETTINGS")]
    [SerializeField] GameObject[] listToDestroy;
    [SerializeField] bool isInteractable;
    [SerializeField] bool enableUseOnceAndDestroy;
    
    // [Header("UI")]
    // [SerializeField] GameObject[] interactablePrompts;
    // [SerializeField] GameObject navigationVisual;

    // ------------------------- UNITY METHODS -------------------------
    
    // Awake is called when this script was first initialized & loaded
    void Awake()
    {
        
    }

    // ...
    void Start()
    {
        
    }

    // Built-In Unity method that called when a gameObject with a Collider enters
    private void OnTriggerEnter(Collider actor)
    {
        // Filters the trigger event to only respond to a 'Player' tagged gameObject
        if (actor.CompareTag("Player"))
        {
            onTriggerEnter?.Invoke();
            
            // navigationVisual.SetActive(false);
            //
            // foreach (var panel in interactablePrompts)
            // {
            //     // Show the interactable panel when the player enters the trigger area
            //     panel.SetActive(true);
            // }
        }
    }

    // Built-In Unity method that called when a gameObject with a Collider exits
    private void OnTriggerExit(Collider actor)
    {
        // Filters the trigger event to only respond to a 'Player' tagged gameObject
        if (actor.CompareTag("Player"))
        {
            onTriggerExit?.Invoke();
            
            // navigationVisual.SetActive(true);
            //
            // foreach (var panel in interactablePrompts) 
            // {
            //     // Hides the interactable panel when the player exits the trigger area
            //     panel.SetActive(false);
            // }
        }
    }

    // ------------------------- INTERFACE -------------------------

    // Method to execute logics when being interacting
    public void Interacted()
    {
        if (!isInteractable) return;
        
        // Executes the event if it's not null (broadcasts the event to the listener/s or subscriber/s)
        onInteract?.Invoke();

        // Only proceeds if Use Once was enabled
        if (enableUseOnceAndDestroy)
        {
            // Traverse through all objects that would be destroyed individually
            foreach (GameObject destroyable in listToDestroy)
            {
                // Destroys the game object this script is attached to
                Destroy(destroyable.gameObject);
            }
        }
    }
    
    // Method to execute logics when un-interacting
    public void UnInteracted()
    {
        if (!isInteractable) return;
        
        // Executes the event if it's not null (broadcasts the event to the listener/s or subscriber/s)
        onUnInteract?.Invoke();
    }
}