using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInputManager : DoNotDestroyOnLoadManager
{
    // ------------------------- VARIABLES -------------------------

    // SINGLETON instances for global access
    private static PlayerInputManager instance;
    private PrivatePrinceControls ppControls;

    // GETTERS for accessing input actions from other scripts
    public static PlayerInputManager Instance => instance;
    public PrivatePrinceControls Controls => ppControls;

    // ------------------------- UNITY METHODS -------------------------

    // Built-in Unity method called when this script was first loaded
    protected override void Awake()
    {
        base.Awake(); // Call base class Awake that handles persistence logic

        // Implement singleton pattern to ensure only one instance of PlayerInputManager exists
        if (instance == null)
        {
            instance = this; // Set the singleton instance

            // ...
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Debug.Log($"Instance of this PlayerInputManager already exists, destroying this duplicate instance to enforce singleton pattern.");

            Destroy(this.gameObject); // Destroy duplicate instances

            // Exit the Awake method early
            // * to prevent further initialization of this duplicate instance
            return;
        }

        // Initialize the PrivatePrinceControls instance for handling player input
        ppControls = new PrivatePrinceControls();
        ppControls.Player.Enable(); // Enable the 'Player' action map to start receiving input
    }

    // Automated Unity Built-In method being called when this object is destroyed
    private void OnDestroy()
    {
        if (instance == this)
        {
            ppControls?.Player.Disable(); // Disable the 'Player' action map to stop receiving input
            instance = null; // Clear the singleton instance when this object is destroyed
        }
    }
}