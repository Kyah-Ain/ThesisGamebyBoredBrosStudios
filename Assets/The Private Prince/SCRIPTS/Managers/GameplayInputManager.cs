using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.InputSystem;

public class GameplayInputManager : MonoBehaviour
{
    // ------------------------- VARIABLES -------------------------

    // SINGLETON instances for global access
    private static GameplayInputManager instance;
    private PrivatePrinceControls ppControls;

    // GETTERS for accessing input actions from other scripts
    public static GameplayInputManager Instance => instance;
    public PrivatePrinceControls Controls => ppControls;

    // SHORTCUTS for accessing specific action maps more easily from other scripts
    private PrivatePrinceControls.PlayerActions PlayerMap => ppControls.Player; // Shortcut to access Player action map
    private PrivatePrinceControls.UserNavigationActions UserNavigationMap => ppControls.UserNavigation; // Shortcut to access UserNavigation action map

    private InputActionMap currentActionMap; // Stores the currently active action map for easy reference when switching maps

    // ------------------------- UNITY METHODS -------------------------

    // Built-in Unity method called when this script was first loaded
    private void Awake()
    {
        // Implement singleton pattern to ensure only one instance of PlayerInputManager exists
        if (instance == null)
        {
            instance = this; // Set the singleton instance

            // Marks this GameObjects' root parent if there is one, and sets it to itself if there's none
            DontDestroyOnLoad(this.transform.root);
        }
        else
        {
            Debug.Log($"Instance of this PlayerInputManager already exists, destroying this duplicate instance to enforce singleton pattern.");

            Destroy(this.gameObject); // Destroy duplicate instances

            // Exit the Awake method early
            // * to prevent further initialization of this duplicate instance
            return;
        }

        // Initialize the PrivatePrinceControls Instance for handling Action Maps 
        ppControls = new PrivatePrinceControls();

        //// Set the default action map to Player (can be changed later with SwitchActionMap method)
        //currentActionMap = ppControls.Player;

        currentActionMap = ppControls.UserNavigation;

        currentActionMap.Enable(); // Enable the default action map to start receiving input
    }

    // Method for Switching Action Maps in the New Input System
    public void SwitchActionMap(string actionMapName) 
    {
        // Check if current map exists
        if (currentActionMap == null) 
        {
            Debug.LogError("currentActionMap is null! Make sure it's set in Awake.");
            return;
        }

        // Look for the map specified by string name parameter 
        var newActionMap = ppControls.asset.FindActionMap(actionMapName);

        // Check if the requested map exists at all
        if (newActionMap == null)
        {
            Debug.LogError($"Action map '{actionMapName}' not found! Staying on current map: '{currentActionMap.name}'.");
            return;
        }

        // Check if we're already on this map
        if (currentActionMap.name == actionMapName)
        {
            Debug.Log($"Action map '{currentActionMap.name}' is already active. No need to switch.");
            return;
        }

        // Performs the Map Switching 
        Debug.Log($"Switching from '{currentActionMap.name}' to '{actionMapName}'");
        currentActionMap.Disable(); // Disable the currently active action map
        currentActionMap = newActionMap;  // Updates the current action map (overwriting the previous map stored)
        currentActionMap.Enable();  // Enable the new action map
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