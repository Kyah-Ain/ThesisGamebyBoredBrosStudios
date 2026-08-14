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
    [SerializeField] UnityEvent onBackToMenu;
    [SerializeField] UnityEvent onLoadingScenes;
    [SerializeField] UnityEvent onFinishLoadingScenes;

    [Header("REFERENCES")]
    [SerializeField] DebuggerNiAinPjls debuggerNiAin; // Custom debugging script from your dev Ain

    // Scene names waiting to be loaded
    [SerializeField] List<string> _sceneQueue = new List<string>();

    // AsyncOperations currently being processed
    [SerializeField] List<AsyncOperation> _scenesToLoad = new List<AsyncOperation>();

    [SerializeField] GameObject _loadingBarObject;
    [SerializeField] Image _loadingBar;

    public ActivationManager activationManager; // Reference to the PanelManager.cs that handles UI panels and prompts

    [Header("DATA")]
    [SerializeField] string mainMenu = "Replace This With Your Main Menu's Scene Name";
    
    [Header("STATUS")]
    [SerializeField] bool enableLoadScreenDelay;
    [SerializeField] float loadScreenDelay = 3f;

    // [SerializeField] string startingScene;
    //public string _loadingScreenScene = "Type Here Your Scene Name!";

    // ---------------------------- UNITY METHODS ---------------------------

    // ...
    public void Awake()
    {
        // Checks if our reference for the script was not set
        if (debuggerNiAin == null)
            // If it is not, then set it automatically by looking for the script class from this object
            debuggerNiAin = this.GetComponent<DebuggerNiAinPjls>();

        _loadingBarObject.SetActive(false);

        // Implement singleton pattern to ensure only one instance of PlayerInputManager exists
        if (instance == null)
        {
            instance = this; // Set the singleton instance

            // Marks this GameObjects' root parent if there is one, and sets it to itself if there's none
            DontDestroyOnLoad(this.transform.root.gameObject);
        }
        else
        {
            Debug.Log($"Instance of this PlayerInputManager already exists, destroying this duplicate instance to enforce singleton pattern.");

            Destroy(this.gameObject); // Destroy duplicate instances
        }

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

        // // Evaluate if the player has already played at least one level by checking the highest level reached
        // if (LevelManager.Instance.highestLevel > 1)
        // {
        //     // Open the prompt panel to confirm starting a new game
        //     activationManager.Activate();

        //     return;
        // }

        // ...
        onFreshStart?.Invoke();
    }

    // Method to start a new game
    public void StartNewGame()
    {
        // ...
        LevelManager.Instance.ResetLevel(); // Reset the game if no levels have been played yet

        // ...
        // Queue your scenes here before starting the loading process.

        // Example:
        // QueueScene("PersistentGameplay");
        // QueueScene("Level_1");

        // StartQueuedSceneLoading();
    }

    // This method now means:
    // "Queue this scene and immediately begin processing the queue."
    //
    // It is kept as a convenience method so your existing Unity Events
    // that call LoadScene() do not need to be completely replaced.
    public void LoadScene(string levelSceneName)
    {
        // Evaluates if the scene name is null or empty
        if (string.IsNullOrEmpty(levelSceneName))
        {
            debuggerNiAin.Error("Scene to load is not specified!");

            return;
        }

        // Clear anything previously waiting
        ClearSceneQueue();

        // Queue the requested scene
        QueueScene(levelSceneName);

        // Begin loading
        StartQueuedSceneLoading();
    }

    // This method now means:
    // "Queue this scene without loading it yet."
    //
    // IMPORTANT:
    // If you want a scene to actually begin loading, call
    // StartQueuedSceneLoading() after queueing your scenes.
    public void LoadSceneAdditive(string levelSceneName)
    {
        // Evaluates if the scene name is null or empty
        if (string.IsNullOrEmpty(levelSceneName))
        {
            debuggerNiAin.Error("Scene to queue is not specified!");

            return;
        }

        // Queue the scene instead of immediately loading it
        QueueScene(levelSceneName);
    }

    // Method that quickly loads the player to the last level played
    public void LoadLastScene()
    {
        // Queue the last level and start loading it
        LoadScene($"Level_{LevelManager.Instance.lastLevel}");
    }

    // Method to load back to Main Menu
    public void LoadToMainMenu(InputAction.CallbackContext context)
    {
        onBackToMenu?.Invoke();

        // LoadSceneAdditive(mainMenu);

        // StartQueuedSceneLoading();

        // // Loads the scene name passed on the parameter ("MainMenu" on default)
        // SceneManager.LoadScene(mainMenu);
    }

    // Method to quit the game
    public void QuitGame()
    {
        Application.Quit(); // Quit the application
    }


    // -------------------------------- SCENE QUEUE ---------------------------

    // Method to add a scene to the loading queue
    public void QueueScene(string levelSceneName)
    {
        // Evaluates if the scene name is null or empty
        if (string.IsNullOrEmpty(levelSceneName))
        {
            debuggerNiAin.Error("Scene to queue is not specified!");

            return;
        }

        // Adds the scene name to the queue
        _sceneQueue.Add(levelSceneName);

        debuggerNiAin.Log(
            $"GameManager: Queued scene '{levelSceneName}'. Queue count: {_sceneQueue.Count}"
        );
    }

    // Method to clear every scene currently waiting in the queue
    public void ClearSceneQueue()
    {
        // ...
        _sceneQueue.Clear();

        debuggerNiAin.Log("GameManager: Scene queue cleared!");
    }

    // Method to start loading all scenes currently in the queue
    public void StartQueuedSceneLoading()
    {
        onLoadingScenes?.Invoke();

        // Evaluates if there are no scenes waiting in the queue
        if (_sceneQueue.Count <= 0)
        {
            debuggerNiAin.Warn("GameManager: Cannot start loading because the scene queue is empty!");

            return;
        }

        // Prevents multiple loading processes from being started
        if (_scenesToLoad.Count > 0)
        {
            debuggerNiAin.Warn("GameManager: A scene loading process is already active!");

            return;
        }

        // Reset the loading bar
        if (_loadingBar != null)
        {
            _loadingBar.fillAmount = 0.0f;
        }

        // Make sure the loading bar is visible
        if (_loadingBarObject != null)
        {
            _loadingBarObject.SetActive(true);
        }

        // Start processing the queue
        StartCoroutine(ProcessSceneQueue());
    }

    // --------------------------- PROCESSOR --------------------------

    // Method that processes the scene queue one scene at a time
    private IEnumerator ProcessSceneQueue()
    {
        debuggerNiAin.Log(
            $"GameManager: Beginning scene queue processing. Scenes queued: {_sceneQueue.Count}"
        );

        // Store the total amount of scenes for the loading bar
        int totalScenes = _sceneQueue.Count;

        // Keep track of how many scenes have completely loaded
        int completedScenes = 0;

        // Process every scene in the queue
        for (int i = 0; i < _sceneQueue.Count; i++)
        {
            // Get the current scene name
            string sceneName = _sceneQueue[i];

            // The FIRST scene uses Single.
            // Every scene AFTER the first uses Additive.
            LoadSceneMode loadMode;

            if (i == 0)
            {
                loadMode = LoadSceneMode.Single;

                debuggerNiAin.Log(
                    $"GameManager: Loading FIRST scene '{sceneName}' using LoadSceneMode.Single."
                );
            }
            else
            {
                loadMode = LoadSceneMode.Additive;

                debuggerNiAin.Log(
                    $"GameManager: Loading scene '{sceneName}' using LoadSceneMode.Additive."
                );
            }

            // Begin loading the current scene
            AsyncOperation sceneLoad = SceneManager.LoadSceneAsync(
                sceneName,
                loadMode
            );

            // Check if Unity failed to create the loading operation
            if (sceneLoad == null)
            {
                debuggerNiAin.Error(
                    $"GameManager: Failed to create loading operation for scene '{sceneName}'."
                );

                continue;
            }

            // Track the currently active loading operation
            _scenesToLoad.Add(sceneLoad);

            // Wait for this scene to finish while updating the loading bar
            while (!sceneLoad.isDone)
            {
                // Check if references are valid before using them
                if (_loadingBar == null || _loadingBarObject == null)
                {
                    debuggerNiAin.Warn(
                        "Loading bar references were destroyed - exiting coroutine."
                    );

                    yield break;
                }

                // Unity's AsyncOperation.progress normally goes from 0.0 to 0.9
                // before the scene finishes activating.
                //
                // Convert:
                // 0.0 - 0.9
                //
                // into:
                // 0.0 - 1.0
                float currentSceneProgress = Mathf.Clamp01(
                    sceneLoad.progress / 0.9f
                );

                // Calculate the progress contributed by completed scenes
                float completedProgress = completedScenes;

                // Add the progress of the scene currently loading
                float totalProgress = completedProgress + currentSceneProgress;

                // Calculate the overall progress of the entire queue
                float targetProgress = totalProgress / totalScenes;

                // Smoothly move the UI toward the target progress
                _loadingBar.fillAmount = Mathf.MoveTowards(
                    _loadingBar.fillAmount,
                    targetProgress,
                    2.0f * Time.deltaTime
                );

                yield return null;
            }

            // This scene has completely finished loading
            completedScenes++;

            // Make sure the completed scene contributes its full amount
            float completedTargetProgress =
                (float)completedScenes / totalScenes;

            // Smoothly move the bar toward the completed scene percentage
            while (_loadingBar.fillAmount < completedTargetProgress)
            {
                // Check if references are valid before using them
                if (_loadingBar == null || _loadingBarObject == null)
                {
                    debuggerNiAin.Warn(
                        "Loading bar references were destroyed - exiting coroutine."
                    );

                    yield break;
                }

                _loadingBar.fillAmount = Mathf.MoveTowards(
                    _loadingBar.fillAmount,
                    completedTargetProgress,
                    2.0f * Time.deltaTime
                );

                yield return null;
            }

            // Remove the completed AsyncOperation from the tracker
            _scenesToLoad.Remove(sceneLoad);

            debuggerNiAin.Log(
                $"GameManager: Finished loading scene '{sceneName}'. " +
                $"Queue progress: {completedScenes}/{totalScenes}"
            );

            StartCoroutine(LoadScreenDelay());
        }

        // ---------------------------- FINISH ---------------------------

        // Make absolutely sure the loading bar reaches 100%
        if (_loadingBar != null)
        {
            while (_loadingBar.fillAmount < 1.0f)
            {
                _loadingBar.fillAmount = Mathf.MoveTowards(
                    _loadingBar.fillAmount,
                    1.0f,
                    2.0f * Time.deltaTime
                );

                yield return null;
            }

            _loadingBar.fillAmount = 1.0f;
        }

        debuggerNiAin.Log("GameManager: All queued scenes have finished loading!");

        // Clear the queue now that everything has been processed
        _sceneQueue.Clear();

        // Clear any remaining AsyncOperations
        _scenesToLoad.Clear();

        // ...
        // Keep this disabled if the loading bar should remain visible.
        // If you want it hidden after loading, uncomment this section.

        // if (_loadingBarObject != null)
        // {
        //     _loadingBarObject.SetActive(false);
        // }
    }

    // Method to delay the loadscreen appearance
    IEnumerator LoadScreenDelay()
    {
        debuggerNiAin.Log("Loading Screen Timer Started");

        // ...
        yield return new WaitForSeconds(loadScreenDelay);

        // ...
        if (enableLoadScreenDelay)
        {
            debuggerNiAin.Log("Loading Screen Timer Fnished");

            // ...
            onFinishLoadingScenes?.Invoke();
        }
    }
}