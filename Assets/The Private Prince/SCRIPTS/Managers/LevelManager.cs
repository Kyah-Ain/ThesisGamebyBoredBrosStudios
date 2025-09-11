using System.Collections; // Grants access to collections and data structures like ArrayList, Hashtable, etc.
using System.Collections.Generic; // Grants access to generic collections like List, Dictionary, etc.
using UnityEngine; // Grants access to Unity's core classes and functions, such as MonoBehaviour, GameObject, Transform, etc.
using UnityEngine.UI; // Grants access to Unity's UI classes and functions, such as Button, Text, Image, etc.

public class LevelManager : MonoBehaviour
{
    // ------------------------- VARIABLES -------------------------
    [Header("Save Manager Attributes")]
    public int highestLevel; // The highest level reached by the player
    public int lastLevel; // The last level the player played

    [Header("UI References")]
    public Button loadButton; // Reference to the Load button in the Main Menu
    public Button[] loadLevelButtons; // Array of buttons for loading levels

    // -------------------------- METHODS ---------------------------
    // Built-in Unity method that is called at the start of activiting this whole script
    private void Start()
    {
        highestLevel = PlayerPrefs.GetInt("player_highestLevel"); // Get the highest level reached by the player from PlayerPrefs
        lastLevel = PlayerPrefs.GetInt("player_lastLevel"); // Get the last level played by the player from PlayerPrefs
    }

    // Built-in Unity method that is called once per frame
    private void Update()
    {
        for (int index = 0; index < highestLevel; index++)
        {
            if (index < loadLevelButtons.Length) // Safety check
            {
                loadLevelButtons[index].interactable = true;
                Debug.Log($"Button [{index}] has been unlocked!");
            }
            else
            {
                Debug.LogWarning($"Tried to unlock button {index}, but only {loadLevelButtons.Length} buttons exist!");
                break; // Exit early to avoid unnecessary iterations
            }
        }
    }
}