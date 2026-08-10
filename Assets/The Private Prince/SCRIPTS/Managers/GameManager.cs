using System.Collections; // Grants access to collections and data structures like ArrayList, Hashtable, etc.
using System.Collections.Generic; // Grants access to generic data structures like List, Dictionary, etc.
using UnityEngine; // Grants access to Unity's core classes and functions, such as MonoBehaviour, GameObject, Transform, etc.

using UnityEngine.UI; // Grants access to Unity's UI classes, such as Button, Text, Image, etc.
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement; // Grants access to Unity's scene management classes, such as SceneManager, Scene, etc.
using UnityEngine.Events; // Grants access to Unity's event system classes and functions, such as UnityEvent, which is used to create custom events in the Inspector

// Required DebuggerNiAinPjls.cs for this to be able to monitor debugs, otherwise use the old one
[RequireComponent(typeof(DebuggerNiAinPjls))]
public class GameManager : MonoBehaviour
{
    // ---------------------------- VARIABLES -------------------------

    private static GameManager instance; // Singleton value, changeabl only here on this script
    public static GameManager Instance => instance; // Singleton instance for global access (On Reading Onleh)
    private PrivatePrinceControls ppControls; // In-Game Control Map

    [Header("EVENTS")]
    [SerializeField] UnityEvent onFreshStart;

    [Header("REFERENCES")]
    [SerializeField] DebuggerNiAinPjls debuggerNiAin; // Custom debugging script from your dev Ain
    [SerializeField] List<AsyncOperation> _scenesToLoad = new List<AsyncOperation>(); // Tracker for Loading Bar
    [SerializeField] GameObject _loadingBarObject;
    [SerializeField] Image _loadingBar;

    public ActivationManager activationManager; // Reference to the PanelManager.cs that handles UI panels and prompts

    [Header("DATA")]
    [SerializeField] string mainMenu = "Replace This With Your Main Menu's Scene Name";
    [SerializeField] string startingScene;
    //public string _loadingScreenScene = "Type Here Your Scene Name!";

    // ---------------------------- UNITY METHODS ---------------------------

    // ...
    public void Awake()
    {
        _loadingBarObject.SetActive(false);

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

        // Checks if our reference for the script was not set
        if (debuggerNiAin == null)
            // If it is not, then set it automatically by looking for the script class from this object
            debuggerNiAin = this.GetComponent<DebuggerNiAinPjls>();

        // Evaluates if an InputManager instance exists in the scene (for reference)
        if (GameplayInputManager.Instance == null)
        {
            debuggerNiAin.Log("GameplayInputManager Instance is NULL!");

            return;
        }

        // Automatically sets the 'Initialized' input control maps from InputManager 
        if (GameplayInputManager.Instance.Controls == null)
        {
            debuggerNiAin.Log("GameplayInputManager Controls is NULL!");

            return;
        }

        // Prepared the controls to be ready for use  
        ppControls = GameplayInputManager.Instance.Controls;
    }

    // OnEnable is called when the object becomes enabled and active
    void OnEnable()
    {
        // Set observers for the User Input
        Subscribe();
    }

    // OnDisable is called when the object becomes disabled
    void OnDisable()
    {
        // Hibernate observers for the User Input to save resources
        Unsubscribe();
    }

    // OnDestroy is called when the object is destroyed
    void OnDestroy()
    {
        // Hibernate observers for the User Input to save resources
        Unsubscribe();

        // Only clean up if this instance is the active singleton
        if (Instance == this)
        {
            // Clear singleton reference
            instance = null;
        }
    }

    // ---------------------- PREPARATION METHODS -------------------------

    // Method to subscribe to events as a listener
    public void Subscribe()
    {
        // Proceeds only if the input control reference was successfully set
        if (ppControls == null) return;

        // SUBSCRIBE METHODS to the input action events
        ppControls.GlobalKeys.Settings.performed += LoadToMainMenu;
    }

    // Method to unsubscribe from events 
    public void Unsubscribe()
    {
        // Proceeds only if the input control reference was successfully set
        if (ppControls == null) return;

        // UNSUBSCRIBE METHODS to the input action events
        ppControls.GlobalKeys.Settings.performed -= LoadToMainMenu;
    }

    // ---------------------------- GAME NAVIGATIONS ---------------------------

    // ...
    public void TryStart()
    {
        // ...
        _loadingBarObject.SetActive(true);

        // Evaluate if the player has already played at least one level by checking the highest level reached
        if (LevelManager.Instance.highestLevel > 1)
        {
            // Open the prompt panel to confirm starting a new game
            activationManager.Activate();

            return;
        }

        // ...
        onFreshStart?.Invoke();
    }

    // Method to start a new game
    public void StartNewGame()
    {
        // ...
        LevelManager.Instance.ResetLevel(); // Reset the game if no levels have been played yet

        // ...
        LoadScene(startingScene);
    }

    // Method to load a specific scene
    public void LoadScene(string levelSceneName)
    {
        // Evaluates if the scene name is null or empty
        if (string.IsNullOrEmpty(levelSceneName))
        {
            debuggerNiAin.Error("Scene to load is not specified!");

            return;
        }

        // Reset the loading bar before starting a new load
        _loadingBar.fillAmount = 0.0f;

        // Make sure the loading bar is visible
        _loadingBarObject.SetActive(true);

        // ...
        StartCoroutine(LoadSceneAsync(levelSceneName));
    }

    // Method that asynchronously loads a specific Unity scene
    private IEnumerator LoadSceneAsync(string levelSceneName)
    {
        debuggerNiAin.Log($"GameManager: Starting async load for scene: {levelSceneName}");

        // ...
        AsyncOperation sceneLoad = SceneManager.LoadSceneAsync(
            levelSceneName,
            LoadSceneMode.Single
        );

        // Track the scene loading operation
        _scenesToLoad.Clear();
        _scenesToLoad.Add(sceneLoad);

        // ...
        yield return StartCoroutine(ProgressLoadingBar());

        debuggerNiAin.Log($"GameManager: Finished loading scene: {levelSceneName}");
    }

    // OVERLOAD Method that loads a specific Unity scene asynchronously
    public void LoadSceneAdditive(string levelSceneName)
    {
        // Evaluates if the scene name is null or empty
        if (string.IsNullOrEmpty(levelSceneName))
        {
            debuggerNiAin.Error("Scene to load is not specified!");
            return;
        }

        // ...
        AsyncOperation sceneLoad = SceneManager.LoadSceneAsync(
            levelSceneName,
            LoadSceneMode.Additive
        );

        // Tracks the scene loading operation
        _scenesToLoad.Add(sceneLoad);
    }

    // Method that quickly loads the player to the last level played
    public void LoadLastScene()
    {
        // ...
        LoadScene($"Level_{LevelManager.Instance.lastLevel}");
    }

    // Method to load back to Main Menu
    public void LoadToMainMenu(InputAction.CallbackContext context)
    {
        // Loads the scene name passed on the parameter ("MainMenu" on default)
        SceneManager.LoadScene(mainMenu);
    }

    // Method to quit the game
    public void QuitGame()
    {
        Application.Quit(); // Quit the application
    }

    // // ...
    // public void LoadScene()
    // {
    //     // Evaluates if the scene name isn't a null default name
    //     if (SaveManager.Instance.currentRegionPoint != null)
    //     {
    //         _scenesToLoad.Add(SceneManager.LoadSceneAsync(SaveManager.Instance.currentRegionPoint));
    //         LoadSceneAdditive(_persistentGameplay);
    //     }
    //     else // Prompt an error if the scene name is not set in the Inspector
    //     {
    //         Debug.LogError("Scene to load is not specified!"); // Log an error if the scene name is not set
    //     }

    //     StartCoroutine(ProgressLoadingBar());
    // }

    // ---------------------------- STATUS ---------------------------

    // ...
    public void StartProgressLoading()
    {
        // ...
        StartCoroutine(ProgressLoadingBar());
    }

    // ...
    public IEnumerator ProgressLoadingBar()
    {
        debuggerNiAin.Log("GameManager: Entered the ProgressLoadingBar Coroutine");

        // Evaluates if there are no scenes currently being loaded
        if (_scenesToLoad.Count <= 0)
        {
            debuggerNiAin.Warn("GameManager: No scenes are currently being loaded!");
            yield break;
        }

        // Starting ...
        float targetProgress = 0.0f;

        // ...
        while (true)
        {
            // Check if references are valid before using them
            if (_loadingBar == null || _loadingBarObject == null)
            {
                debuggerNiAin.Warn("Loading bar references were destroyed - exiting coroutine");
                yield break; // Exit the coroutine completely
            }

            // ...
            float totalProgress = 0.0f;
            bool allScenesLoaded = true;

            // Calculate the average loading progress of all tracked scenes
            for (int i = 0; i < _scenesToLoad.Count; i++)
            {
                debuggerNiAin.Log($"GameManager: Loading Scene {i}");

                // Check if the scene has finished loading
                if (!_scenesToLoad[i].isDone)
                {
                    allScenesLoaded = false;
                }

                // Unity's scene loading progress normally reaches 0.9 before activation
                // Convert the 0.0 - 0.9 range into a 0.0 - 1.0 range
                float sceneProgress = Mathf.Clamp01(_scenesToLoad[i].progress / 0.9f);

                totalProgress += sceneProgress;
            }

            // ...
            targetProgress = totalProgress / _scenesToLoad.Count;

            // Smoothly move the loading bar toward the target progress
            _loadingBar.fillAmount = Mathf.MoveTowards(
                _loadingBar.fillAmount,
                targetProgress,
                2.0f * Time.deltaTime
            );

            // ...
            if (allScenesLoaded)
            {
                break;
            }

            yield return null;
        }

        // ...
        while (_loadingBar.fillAmount < 1.0f)
        {
            // Smoothly finish the loading bar
            _loadingBar.fillAmount = Mathf.MoveTowards(
                _loadingBar.fillAmount,
                1.0f,
                2.0f * Time.deltaTime
            );

            yield return null;
        }

        // Make absolutely sure the loading bar reaches 100%
        _loadingBar.fillAmount = 1.0f;

        debuggerNiAin.Log("GameManager: All scenes loaded!");

        // ...
        _scenesToLoad.Clear();

        // ...
        // Keep this disabled if the loading bar should remain visible
        // If you want it hidden after loading, uncomment this section.

        // if (_loadingBarObject != null)
        // {
        //     _loadingBarObject.SetActive(false);
        // }
    }
}