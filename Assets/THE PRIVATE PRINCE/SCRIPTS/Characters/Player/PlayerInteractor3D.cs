using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.InputSystem;

public class PlayerInteractor3D : MonoBehaviour
{
    // ------------------------- VARIABLES -------------------------

    [Header("CORE")] 
    [SerializeField] Transform interactEmmiter; // The starting point of the interaction
    [SerializeField] [ReadOnly] Vector3 interactDirection; // The direction of where to emit the interaction
    [SerializeField] [ReadOnly] Vector2 inputVector; // The current input value/coordinates
    
    [Header("SETTINGS")]
    [SerializeField] [Range(0f,100f)] float interactRadius = 0.5f; // Dictates the size of the interaction (how wide it is)
    [SerializeField] [Range(0f,100f)] float interactLength = 5f; // Dictates the length of the interaction (how long it is)
    [SerializeField] LayerMask exemptionMask; // List of layers that would be ignored during interaction casts
    
    // ----------------------- UNITY METHODS -------------------------
    #region UNITY METHODS
    
    // Awake is called when this script was first initialized & loaded
    private void Awake()
    {
        // Sets the cast to emit to the "Right" as default
        interactDirection = interactEmmiter.right;
    }
    
    // OnEnable is called when the object becomes enabled and active
    void OnEnable()
    {
        Subscribe();
    }

    // OnDisable is called when the object becomes disabled
    void OnDisable()
    {
        UnSubscribe();
    }
    
    // Update is called once per frame
    private void Update()
    {
        SetDirection();
    }

    #endregion
    
    // ----------------------- EVENT METHODS -------------------------
    #region EVENT METHODS

    // Method to subscribe your local method to an event trigger
    protected virtual void Subscribe()
    {
        // Set subscriptions of these methods to an event
        // Left (Event Call) += Right (Method that would be called)
        GameEventsManager.Instance.inputEvents.onMovement += ReadMovement;
        GameEventsManager.Instance.inputEvents.onInteract += Interact;
        // GameEventsManager.Instance.inputEvents.onSubmit += Interact;
    }

    // Method to UnSubscribe your local method to an event trigger
    protected virtual void UnSubscribe()
    {
        // UnSubscribe them methods from an event
        // Left (Event Call) -= Right (Method that would be called)
        GameEventsManager.Instance.inputEvents.onMovement -= ReadMovement;
        GameEventsManager.Instance.inputEvents.onInteract -= Interact;
        // GameEventsManager.Instance.inputEvents.onSubmit -= Interact;
    }
    
    #endregion 
    
    // ----------------------- INTERACTION METHODS -------------------------
    #region INTERACTION METHODS

    // Method to perform an interaction 
    void Interact(InputAction.CallbackContext context)
    {
        // Emits a sphere scan &
        // Only proceeds further if it hits objects that are not in the "exemptionMask"
        if (Physics.SphereCast(
                interactEmmiter.position, // The origin point of the cast
                interactRadius, // How big is the cast
                interactDirection, // What direction it casts
                out RaycastHit hit, // What is the result of the cast
                interactLength, // How long is the cast
                ~exemptionMask // What to ignore when hit by the cast
            )
        )
        {
            Debug.Log($"PlayerInteractor3D: Raycast hits {hit.collider.name}.");
            
            // Checks for an implementation of IInteractable to ensure that the object is an interactable
            if (hit.collider.TryGetComponent<IInteractable>(out IInteractable interactable))
            {
                Debug.Log($"PlayerInteractor3D: The hit has Interactable.");
                
                // Interact the scanned interactable
                interactable.Interacted();
            }
        }
    }

    #endregion
    
    // --------------------------- HELPERS -------------------------
    #region HELPERS

    // Method to set face direction of the interactor based on Input
    void SetDirection()
    {
        // Prevents setting the face direction to right when facing left but no input
        if (inputVector.x < 0.1f & inputVector.x > -0.1f) return;
        
        // Only proceeds if the input was greater than zero (indicating right)
        if (inputVector.x > 0f)
        {
            // Sets the cast to emit to the "Right"
            // Then early exits to prevent executing further code below
            interactDirection = interactEmmiter.right;
            return;
        }
        
        // Sets the cast to emit to the "Left"
        interactDirection = -interactEmmiter.right;
    }
    
    // Method for receiving movement inputs
    void ReadMovement(InputAction.CallbackContext context)
    {
        // Reads Input from the Event-Based New Input System
        inputVector = context.ReadValue<Vector2>();
    }

    #endregion
    
    // -------------------------- DEBUGGERS -------------------------
    #region DEBUGGERS

    // Method to draw Scene Wireframes on the Unity Scene only
    void OnDrawGizmos()
    {
        if (interactEmmiter == null) return;

        Gizmos.color = Color.yellow;

        Vector3 origin = interactEmmiter.position;
        Vector3 end = origin + interactDirection * interactLength;

        // Sphere at the start and end of the cast
        Gizmos.DrawWireSphere(origin, interactRadius);
        Gizmos.DrawWireSphere(end, interactRadius);

        // Connect the two spheres so the sweep is visible as a "capsule"
        DrawCastConnectors(origin, end, interactDirection, interactRadius);
    }

    // Method to draw the connecting lines between the start and end spheres of a sphere cast
    void DrawCastConnectors(Vector3 origin, Vector3 end, Vector3 direction, float radius)
    {
        // Find two perpendicular axes to the cast direction
        Vector3 up = Vector3.Cross(direction, Vector3.right);
        if (up.sqrMagnitude < 0.001f) up = Vector3.Cross(direction, Vector3.forward);
        up.Normalize();
        Vector3 side = Vector3.Cross(direction, up).normalized;

        Gizmos.DrawLine(origin + up * radius, end + up * radius);
        Gizmos.DrawLine(origin - up * radius, end - up * radius);
        Gizmos.DrawLine(origin + side * radius, end + side * radius);
        Gizmos.DrawLine(origin - side * radius, end - side * radius);
    }

    #endregion
}