using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Events;
// using static UnityEngine.Rendering.DebugUI;

public class InteractableObject : MonoBehaviour, IInteractable
{
    // ------------------------- VARIABLES -------------------------

    [Header("EVENTS")]
    [SerializeField] UnityEvent onInteract;
    [SerializeField] UnityEvent onUnInteract;

    [Header("UI")]
    [SerializeField] GameObject[] interactablePrompts;
    [SerializeField] GameObject navigationVisual;

    // ------------------------- UNITY METHODS -------------------------

    // Built-In Unity method that called when a gameObject with a Collider enters
    private void OnTriggerEnter(Collider actor)
    {
        // Filters the trigger event to only respond to a 'Player' tagged gameObject
        if (actor.CompareTag("Player"))
        {
            navigationVisual.SetActive(false);

            foreach (var panel in interactablePrompts)
            {
                // Show the interactable panel when the player enters the trigger area
                panel.SetActive(true);
            }
        }
    }

    // Built-In Unity method that called when a gameObject with a Collider exits
    private void OnTriggerExit(Collider actor)
    {
        // Filters the trigger event to only respond to a 'Player' tagged gameObject
        if (actor.CompareTag("Player"))
        {
            navigationVisual.SetActive(true);

            foreach (var panel in interactablePrompts) 
            {
                // Hides the interactable panel when the player exits the trigger area
                panel.SetActive(false);
            }
        }
    }

    // ------------------------- INTERFACE -------------------------

    // Method to execute logics when being interacting
    public void Interact()
    {
        // Executes the event if it's not null (broadcasts the event to the listener/s or subscriber/s)
        onInteract?.Invoke();
    }

    // Method to execute logics when un-interacting
    public void UnInteract()
    {
        // Executes the event if it's not null (broadcasts the event to the listener/s or subscriber/s)
        onUnInteract?.Invoke();
    }
}