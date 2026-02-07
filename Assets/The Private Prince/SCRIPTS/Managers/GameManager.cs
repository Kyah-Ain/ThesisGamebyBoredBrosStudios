using System.Collections; // Grants access to collections and data structures like ArrayList, Hashtable, etc.
using System.Collections.Generic; // Grants access to generic collections like List, Dictionary, etc.
using UnityEngine; // Grants access to Unity's core classes and functions, such as MonoBehaviour, GameObject, Transform, etc.
using UnityEngine.UI; // Grants access to Unity's UI classes and functions, such as Button, Text, Image, etc.
using UnityEngine.SceneManagement; // Grants access to Unity's scene management classes and functions, such as SceneManager, Scene, etc.
using UnityEngine.Events; // Grants access to Unity's event system classes and functions, such as UnityEvent, which is used for creating custom events in Unity

public class GameManager : MonoBehaviour
{
    // ---------------------------- VARIABLES -------------------------

    // Singleton instance for global access (On Reading Onleh)
    public static GameManager Instance { get; private set; }

    [Header("Script References")]
    public LevelManager levelManager; // Reference to the SaveManager.cs that handles saving and loading levels
    public ActivationManager activationManager; // Reference to the PanelManager.cs that handles UI panels and prompts

    //[Header("Scene Reference")]
    //public string defaultSceneToLoad = "INPUT HERE YOUR DEFAULT SCENE NAME"; // Reference to the scene that will be loaded 

    // ---------------------------- METHODS ---------------------------

    // ...
    public void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            Debug.Log("Cleared all saved data for new game session");

            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Method to start a new game
    public void StartNewGame(string startingSceneName = null) 
    {
        // Evaluate if the player has already played at least one level by checking the highest level reached
        if (levelManager.highestLevel > 1)
        {
            activationManager.Activate(); // Open the prompt panel to confirm starting a new game
        }
        else // If the player has not played any levels yet, we can directly start a new game without confirmation
        {
            ResetGame(); // Reset the game if no levels have been played yet
            LoadScene(startingSceneName); // Loads a specific Unity scene 
        }
    }

    // Method that loads a specific Unity scene by its name (which can be set in the Inspector)
    public void LoadScene(string levelSceneName = null) 
    {
        // Evaluates if the scene name isn't a null default name
        if (levelSceneName != null)
        {
            SceneManager.LoadScene(levelSceneName); // Load the specified scene by its name
        }
        else // Prompt an error if the scene name is not set in the Inspector
        {
            Debug.LogError("Scene to load is not specified!"); // Log an error if the scene name is not set
        }
    }

    // Method that quickly loads the player to the last level played
    public void LoadLastScene()
    {
        SceneManager.LoadScene($"Level_{levelManager.lastLevel}"); // Load the last level played by the player
    }

    // Method to reset the game progress back to zero
    public void ResetGame() 
    {
        PlayerPrefs.SetInt("player_highestLevel", 0); // Reset the highest level reached by the player
        PlayerPrefs.SetInt("player_lastLevel", 0); // Reset the last level played by the player
    }

    // Method to quit the game
    public void QuitGame() 
    {
        Application.Quit(); // Quit the application
    }
}
