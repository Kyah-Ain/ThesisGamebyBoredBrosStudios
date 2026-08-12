using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.InputSystem;

public enum ActionMapType
{
    GlobalKeys,
    Player,
    UI
}

// Required DebuggerNiAinPjls.cs for this to be able to monitor debugs, otherwise use the old one
[RequireComponent(typeof(DebuggerNiAinPjls))]
// Sets this script to execute before most other scripts
[DefaultExecutionOrder(-100)]
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
    private PrivatePrinceControls.UIActions UIMap => ppControls.UI; // Shortcut to access UserNavigation action map

    [Header("REFERENCES")]
    [SerializeField] DebuggerNiAinPjls debuggerNiAin; // Custom debugging script from your dev Ain

    [Header("MAP STATUS")]
    [SerializeField] List <ActionMapType> activatedMaps = new(); // Stores the currently active input map type (Player, UI, etc.)
    [SerializeField] List <InputActionMap> currentActiveMaps = new();// Stores the currently active InputActionMap reference (used for enabling/disabling maps)
   
    // ------------------------- UNITY METHODS -------------------------

    // Built-in Unity method called when this script was first loaded
    private void Awake()
    {
        // Checks if our reference for the script was not set
        if(debuggerNiAin == null)
            // If it is not, then set it automatically by looking for the script class from this object
            debuggerNiAin = this.GetComponent<DebuggerNiAinPjls>();
        
        // Ensure only one instance of InputManager exists (Singleton pattern)
        if (instance == null)
        {
            // Assign this instance as the global reference
            instance = this;

            // Keep this object alive across scene changes
            DontDestroyOnLoad(this.transform.root.gameObject);
        }
        else
        {
            debuggerNiAin.Log($"A copy of GameplayInputManager has been deleted: {this.gameObject.name}");
            
            Destroy(this.gameObject); // Destroy duplicate InputManager instances
        }

        // Initialize the PrivatePrinceControls Instance for handling Action Maps 
        ppControls = new PrivatePrinceControls();

        // Prompt that the control has been successfull
        debuggerNiAin.Log($"Successfully persist InputManager through {this.transform.root.gameObject.name}");
    }

    // OnEnable is called when the object becomes enabled and active
    void OnEnable()
    {
        // Enable the default action map when this object becomes active
        EnableDefaultMap();
    }

    // OnDisable is called when the object becomes disabled
    void OnDisable()
    {
        // Disable all input maps to prevent unwanted input processing
        DisableAllMaps();
    }

    // OnDestroy is called when the object is destroyed
    void OnDestroy()
    {
        // Only clean up if this instance is the active singleton
        if (Instance == this)
        {
            DisableAllMaps(); // Ensure no input remains active

            // Clear singleton reference
            instance = null;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // For TESTING PURPOSES ONLY:
        // Switch input maps using enum dropdown in the Inspector during play mode to verify functionality
        //SwitchMap(currentMapType);
    }

    // ------------------------- MAP METHODS -------------------------

    // Method to enable a new input map using a string representation of the enum value
    public void EnableMap(string newMapTypeStr)
    {
        // Convert string to enum (Case-Insensitive) and pass it to the main logic below
        if (System.Enum.TryParse(newMapTypeStr, true, out ActionMapType parsedEnum))
        {
            MapEnabler(parsedEnum);
        }
        else
        {
            debuggerNiAin.Log($"String '{newMapTypeStr}' could not be converted to ActionMapType.");
        }
    }

    #region ENABLE EXTENSION

        // Enables the default input map when the game starts
        public void EnableDefaultMap()
        {
            //// Set the default map type (Usually the "UI" Action Map)
            //currentActionMap = GetMap(currentMapType);

            //// Enable the default action map at first initialization
            //currentActionMap.Enable();

            EnableMap("UI");  
            // EnableMap("Player"); 
        }

    #endregion
    
    // Method to disable a new input map using a string representation of the enum value
    public void DisableMap(string newMapTypeStr)
    {
        // Convert string to enum (Case-Insensitive) and pass it to the main logic below
        if (System.Enum.TryParse(newMapTypeStr, true, out ActionMapType parsedEnum))
        {
            MapDisabler(parsedEnum);
        }
        else
        {
            debuggerNiAin.Log($"String '{newMapTypeStr}' could not be converted to ActionMapType.");
        }
    }

    #region DISABLE EXTENSION

        // Disables all input maps 
        public void DisableAllMaps()
        {
            // Disable all known action maps and clear the current action map reference
            foreach (InputActionMap map in currentActiveMaps) 
            {
                map.Disable();
            }
        }

    #endregion

    // ----------------------- HELPER METHODS -------------------------

    // Converts enum values into actual InputActionMap references directly
    private InputActionMap GetMap(ActionMapType mapType)
    {
        switch (mapType)
        {
            case ActionMapType.GlobalKeys:
                return Controls.GlobalKeys;

            case ActionMapType.UI:
                return Controls.UI;

            case ActionMapType.Player:
                return Controls.Player;

            default:
                debuggerNiAin.Log($"No mapping exists for InputMapType '{mapType}'.");
                return null;
        }
    }

    // ------------------------- PROCESSOR METHODS -------------------------

    // Method to enable a new input map 
    void MapEnabler(ActionMapType newMapType)
    {
        // Get the InputActionMap associated with the requested enum value
        InputActionMap newActionMap = GetMap(newMapType); 

        // Stop if the requested map does not exist or is already active
        if (newActionMap != null && !currentActiveMaps.Contains(newActionMap)) // && newMapType != currentMapType (BACKUP)
        {
            // Update the current map type & action map reference to the new values
            //currentMapType = newMapType; // Fixed syntax error here
            //currentActionMap = newActionMap;

            // Enable the new action map
            newActionMap.Enable();
            //currentActionMap.Enable();

            // Add the new action map to the list of active map
            currentActiveMaps.Add(newActionMap);

            // Updates the map tracking list to include the newly activated map
            activatedMaps.Add(newMapType);

            debuggerNiAin.Log($"Successfully enabled {newActionMap} Action Map!");
        }
    }

    // Method to disable a new input map 
    void MapDisabler(ActionMapType newMapType)
    {
        // Get the InputActionMap associated with the requested enum value
        InputActionMap selectedActionMap = GetMap(newMapType);

        // Stop if the requested map does not exist
        if (selectedActionMap != null && currentActiveMaps.Contains(selectedActionMap))
        {
            // Disable one of the current active action map
            selectedActionMap.Disable();

            // Remove the disabled action map from the list of active maps
            currentActiveMaps.Remove(selectedActionMap);

            // Updates the map tracking list to remove a disabled map
            activatedMaps.Remove(newMapType);

            debuggerNiAin.Log($"Successfully disabled {selectedActionMap} Action Map!");
        }
    }
}