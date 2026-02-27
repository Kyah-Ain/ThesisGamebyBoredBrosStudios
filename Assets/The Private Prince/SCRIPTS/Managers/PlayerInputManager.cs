using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInputManager : MonoBehaviour
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
    private void Awake()
    {
        // Implement singleton pattern to ensure only one instance of PlayerInputManager exists
        if (instance == null)
        {
            instance = this; // Set the singleton instance

            // Marks this GameObjects' root parent if there is one, and sets it to itself if there's none
            DontDestroyOnLoad(this.gameObject.transform.root);
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