using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class InteractableObject : MonoBehaviour
{
    // ------------------------- VARIABLES -------------------------

    [Header("UI REFERENCES")]
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
}
