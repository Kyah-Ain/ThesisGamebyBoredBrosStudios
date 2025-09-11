using System.Collections; // Grants access to collections and data structures like ArrayList, Hashtable, etc.
using System.Collections.Generic; // Grants access to generic collections like List, Dictionary, etc.
using UnityEngine; // Grants access to Unity's core classes and functions, such as MonoBehaviour, GameObject, Transform, etc.

public class SaveManager : MonoBehaviour
{
    // --------------------------- VARIABLES --------------------------
    [Header("Stage Attributes")]
    public int currentLevel; // The highest level reached by the player

    // ---------------------------- METHODS ---------------------------
    // Built-in Unity method that is called at the start of activating this whole script
    private void Start()
    {
        // Save the last level played by the player
        PlayerPrefs.SetInt("player_lastLevel", currentLevel); 

        // Ensures to only save the highest level a player reached
        if (PlayerPrefs.GetInt("player_highestLevel") < currentLevel) 
        {
            // Update the highest level reached by the player
            PlayerPrefs.SetInt("player_highestLevel", currentLevel); 
        }
        // Forcefully save all PlayerPrefs data to the player disk (Precaution when a game crash)
        PlayerPrefs.Save(); 
    }
}
