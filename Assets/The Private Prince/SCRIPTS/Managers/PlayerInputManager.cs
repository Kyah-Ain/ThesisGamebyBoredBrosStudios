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
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject); // Destroy duplicate instances
        }
        else
        {
            instance = this; // Set the singleton instance
            DontDestroyOnLoad(this.gameObject); // Persist across scene loads
        }

        // Initialize the PrivatePrinceControls instance for handling player input
        ppControls = new PrivatePrinceControls();
        ppControls.Player.Enable(); // Enable the 'Player' action map to start receiving input
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            ppControls?.Player.Disable(); // Disable the 'Player' action map to stop receiving input
            instance = null; // Clear the singleton instance when this object is destroyed
        }
    }
}