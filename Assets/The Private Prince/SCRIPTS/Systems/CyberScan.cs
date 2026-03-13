using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CyberScan : MonoBehaviour
{
    // ------------------------- VARIABLES -------------------------

    // Reference to the PlayerInput component for handling new input system actions and controls
    private PrivatePrinceControls ppControls;

    [SerializeField] private GameEvent onCyberScan; // Placeholder for the Event Trigger Scriptable Object

    public GameObject cyberScan;
    [SerializeField] private bool cyberStatus = false;

    // ------------------------- UNITY METHODS -------------------------

    // ...
    private void Awake()
    {
        // Assigns the gameObject's "Player Input" component for the new input system to this script
        if (ppControls == null && GameplayInputManager.Instance != null)
        {
            // Accesses the controls from the PlayerInputManager singleton instance
            ppControls = GameplayInputManager.Instance.Controls;

            Debug.Log($"New Input System was set: {ppControls}");
        }
        else if (GameplayInputManager.Instance == null)
        {
            Debug.LogError("PlayerInputManager singleton not found! Make sure it exists in the scene.");
        }
    }

    // ...
    private void OnEnable()
    {
        // ...
        cyberScan.SetActive(false);

        // Ensure subscriptions are active when object is enabled
        if (ppControls != null)
        {
            // Subscribes to the performed events
            ppControls.Player.Cyberscan.performed += ToggleCyberScan;
        }
    }

    // ...
    private void OnDisable()
    {
        // Clean up subscriptions when object is disabled
        if (ppControls != null)
        {
            // Subscribes to the performed events
            ppControls.Player.Cyberscan.performed -= ToggleCyberScan;
        }
    }

    // ...
    public void ToggleCyberScan(InputAction.CallbackContext context) 
    {
        // ...
        cyberStatus = !cyberStatus;

        // ...
        cyberScan.SetActive(cyberStatus);

        // ...
        onCyberScan.TriggerEvent();

        //if (cyberStatus == true) 
        //{
        //    onCyberScan.TriggerEvent();
        //}
    } 
}