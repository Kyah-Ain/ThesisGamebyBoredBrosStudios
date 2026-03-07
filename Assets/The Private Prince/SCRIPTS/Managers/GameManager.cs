using System.Collections; // Grants access to collections and data structures like ArrayList, Hashtable, etc.
using System.Collections.Generic; // Grants access to generic collections like List, Dictionary, etc.
using UnityEngine; // Grants access to Unity's core classes and functions, such as MonoBehaviour, GameObject, Transform, etc.
using UnityEngine.UI; // Grants access to Unity's UI classes and functions, such as Button, Text, Image, etc.
using UnityEngine.SceneManagement; // Grants access to Unity's scene management classes and functions, such as SceneManager, Scene, etc.
using UnityEngine.Events; // Grants access to Unity's event system classes and functions, such as UnityEvent, which is used for creating custom events in Unity

public class GameManager : MonoBehaviour
{
    // ---------------------------- VARIABLES -------------------------

    //// Singleton value, changeabl only here on this script
    //private static GameManager instance;

    //// Singleton instance for global access (On Reading Onleh)
    //public static GameManager Instance => instance;

    [Header("Script References")]
    public ActivationManager activationManager; // Reference to the PanelManager.cs that handles UI panels and prompts

    private List<AsyncOperation> _scenesToLoad = new List<AsyncOperation>();

    public string _persistentGameplay = "Replace This With Your Scene Name!";

    [SerializeField] private GameObject _loadingBarObject;
    [SerializeField] private Image _loadingBar;

    // ---------------------------- METHODS ---------------------------

    // ...
    public void Awake()
    {
        _loadingBarObject.SetActive(false);
    }

    // Method to start a new game
    public void StartNewGame(string startingSceneName = null) 
    {
        _loadingBarObject.SetActive(true);

        // Evaluate if the player has already played at least one level by checking the highest level reached
        if (LevelManager.Instance.highestLevel > 1)
        {
            activationManager.Activate(); // Open the prompt panel to confirm starting a new game
        }
        else // If the player has not played any levels yet, we can directly start a new game without confirmation
        {
            LevelManager.Instance.ResetLevel(); // Reset the game if no levels have been played yet
            LoadScene(startingSceneName); // Loads a specific Unity scene 
            StartCoroutine(ProgressLoadingBar());
        }
    }

    // Method that loads a specific Unity scene by its name (which can be set in the Inspector)
    public void LoadScene(string levelSceneName = null) 
    {
        // Evaluates if the scene name isn't a null default name
        if (levelSceneName != null)
        {
            _scenesToLoad.Add(SceneManager.LoadSceneAsync(levelSceneName));
            LoadSceneAdditive(_persistentGameplay);
        }
        else // Prompt an error if the scene name is not set in the Inspector
        {
            Debug.LogError("Scene to load is not specified!"); // Log an error if the scene name is not set
        }
    }

    // Method that loads a specific Unity scene by its name (which can be set in the Inspector)
    public void LoadSceneAdditive(string levelSceneName = null)
    {
        // Evaluates if the scene name isn't a null default name
        if (levelSceneName != null)
        {
            _scenesToLoad.Add(SceneManager.LoadSceneAsync(levelSceneName, LoadSceneMode.Additive));
        }
        else // Prompt an error if the scene name is not set in the Inspector
        {
            Debug.LogError("Scene to load is not specified!"); // Log an error if the scene name is not set
        }
    }

    // Method that quickly loads the player to the last level played
    public void LoadLastScene()
    {
        SceneManager.LoadSceneAsync($"Level_{LevelManager.Instance.lastLevel}"); // Load the last level played by the player
    }

    // Method to quit the game
    public void QuitGame() 
    {
        Application.Quit(); // Quit the application
    }

    // ...
    public IEnumerator ProgressLoadingBar() 
    {
        // Starting ... 
        float loadProgress = 0.0f;

        // ...
        for (int i = 0; i < _scenesToLoad.Count; i++) 
        {
            // ...
            while (!_scenesToLoad[i].isDone) 
            {
                // ...
                loadProgress += _scenesToLoad[i].progress;
                _loadingBar.fillAmount = loadProgress / _scenesToLoad.Count;
                yield return null;
            }
        }
    }
}