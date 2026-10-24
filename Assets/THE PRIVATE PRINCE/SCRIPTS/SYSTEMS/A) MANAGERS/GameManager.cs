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
    // ---------------------------- VARIABLES --------------------------

    [Header("REFERENCES")]
    [SerializeField] DebuggerNiAinPjls debuggerNiAin; // Custom debugging script from your dev Ain
    private static GameManager instance; // Singleton value, changeable only here on this script
    public static GameManager Instance => instance; // Singleton instance for global access (On Reading Onleh)
    private PrivatePrinceControls ppControls; // In-Game Control Map

    [Header("EVENTS")]
    [SerializeField] UnityEvent onFreshStart;
    [SerializeField] UnityEvent onBackToMenu;
    [SerializeField] UnityEvent onLoadingScenes;
    [SerializeField] UnityEvent onFinishLoadingScenes;

    [Header("VISUALS")]
    [SerializeField] Image _loadingBar;
    [SerializeField] float loadingBarSpeed = 2f; // How fast the bar fills toward the real progress

    [Header("QUEUE (Single + Additive)")]
    // Scene names waiting to be loaded
    [SerializeField] List<string> _sceneQueue = new List<string>();

    // OFF = the first queued scene stays the active scene (old behavior).
    // ON  = the LAST queued scene becomes the active scene (handy for "PersistentGameplay" + "Level_1").
    [SerializeField] bool setLastQueuedSceneActive;

    [Header("ADDITIVE LOAD / UNLOAD")]
    // Scenes unloaded after LoadSceneAndSetActive() finishes.
    // Leave EMPTY to only load (no error). Empty names and scenes that aren't loaded are skipped.
    [SerializeField] string[] _scenesToUnload;

    // Should additive load / unload requests also show the loading screen events + bar?
    [SerializeField] bool showLoadingScreenOnAdditive;

    [Header("STATUS")]
    [SerializeField][ReadOnly] bool isProcessingScenes;
    [SerializeField] bool enableLoadScreenDelay;
    [SerializeField] float loadScreenDelay = 3f;

    // The progress value currently shown on the loading bar (kept separately so the bar can be missing)
    private float _barProgress;

    // True while any scene load / unload request is running
    public bool IsBusy => isProcessingScenes;

    // A scene request that arrived while another one was still running
    private struct SceneRequest
    {
        public List<string> scenesToLoad;
        public bool firstSceneIsSingle;
        public string sceneToActivate;
        public List<string> scenesToUnload;
        public bool showLoadingScreen;
    }

    // Requests wait here and run one by one, in the order they were made
    private readonly Queue<SceneRequest> _pendingRequests = new Queue<SceneRequest>();

    // ---------------------------- UNITY METHODS ---------------------------

    // ...
    public void Awake()
    {
        // Checks if our reference for the script was not set
        if (debuggerNiAin == null)
            // If it is not, then set it automatically by looking for the script class from this object
            debuggerNiAin = this.GetComponent<DebuggerNiAinPjls>();

        // Ensure only one instance of GameManager exists (Singleton pattern)
        if (instance != null && instance != this)
        {
            debuggerNiAin.Log($"A copy of GameManager has been deleted: {this.gameObject.name}");

            Destroy(this.gameObject); // Destroy duplicate GameManager instances

            // Exits early and skips executing logics further after this
            return;
        }

        // Assign this instance as the global reference
        instance = this;

        // Detach this gameObject as a child of any gameObject
        this.transform.SetParent(null);

        // Keep this object alive across scene changes
        DontDestroyOnLoad(this.gameObject);

        // Try to get the controls now. If the InputManager isn't ready yet, Start() will try again.
        if (!TryResolveControls())
            debuggerNiAin.Log("GameManager: Controls not ready in Awake, will retry in Start.");
    }

    // Start runs after EVERY Awake in the scene, so the InputManager should exist by now
    void Start()
    {
        // Duplicates are being destroyed, they must not subscribe to anything
        if (instance != this) return;

        // Retry for the case where the InputManager woke up AFTER this script
        Subscribe();
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

    // Method that tries to grab the control map from the InputManager. Returns true when ready.
    private bool TryResolveControls()
    {
        // Already have it
        if (ppControls != null) return true;

        if (GameplayInputManager.Instance == null) return false;
        if (GameplayInputManager.Instance.Controls == null) return false;

        ppControls = GameplayInputManager.Instance.Controls;
        return true;
    }

    // Method to subscribe to events as a listener
    public void Subscribe()
    {
        // Only the real singleton listens to input
        if (instance != this) return;

        // Proceeds only if the input control reference could be set
        if (!TryResolveControls()) return;

        // Remove first so calling Subscribe() twice never stacks duplicate listeners
        ppControls.GlobalKeys.Settings.performed -= LoadToMainMenu;
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

    // Lets a loading screen that lives inside a scene register its bar after a scene change
    public void SetLoadingBar(Image bar)
    {
        _loadingBar = bar;
        ApplyBar();
    }

    // ---------------------------- GAME NAVIGATIONS ---------------------------

    // ...
    public void TryStart()
    {
        // ...
        onFreshStart?.Invoke();
    }

    // Method to start a new game
    public void StartNewGame()
    {
        // Reset the game if no levels have been played yet
        if (LevelManager.Instance != null)
            LevelManager.Instance.ResetLevel();
        else
            debuggerNiAin.Warn("GameManager: LevelManager.Instance is NULL, skipped ResetLevel().");

        // ...
        // Queue your scenes here before starting the loading process.

        // Example:
        // QueueScene("PersistentGameplay");
        // QueueScene("Level_1");

        // StartQueuedSceneLoading();
    }

    // "Replace everything with this scene" (first scene loads as Single).
    // Kept so your existing Unity Events that call LoadScene() keep working.
    public void LoadScene(string levelSceneName)
    {
        if (string.IsNullOrEmpty(levelSceneName))
        {
            debuggerNiAin.Error("Scene to load is not specified!");
            return;
        }

        // Must come BEFORE ClearSceneQueue()
        if (isProcessingScenes)
        {
            debuggerNiAin.Warn($"GameManager: Ignored LoadScene('{levelSceneName}'), scenes are already loading!");
            return;
        }

        ClearSceneQueue();
        QueueScene(levelSceneName);
        StartQueuedSceneLoading();
    }

    // "Queue this scene without loading it yet."
    // Call StartQueuedSceneLoading() afterwards to actually load the queue.
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
        if (LevelManager.Instance == null)
        {
            debuggerNiAin.Error("GameManager: LevelManager.Instance is NULL, cannot load the last level!");
            return;
        }

        // Queue the last level and start loading it
        LoadScene($"Level_{LevelManager.Instance.lastLevel}");
    }

    // Method to load back to Main Menu
    public void LoadToMainMenu(InputAction.CallbackContext context)
    {
        onBackToMenu?.Invoke();
    }

    // Method to quit the game
    public void QuitGame()
    {
        Application.Quit(); // Quit the application
    }

    // ------------------------ ADDITIVE LOAD / UNLOAD (PUBLIC) -----------------------

    // UnityEvent friendly: loads the scene additively, sets it active, then unloads
    // everything in the serialized _scenesToUnload array. Other active scenes are untouched.
    public void LoadSceneAndSetActive(string sceneName)
    {
        LoadSceneAndSetActive(sceneName, _scenesToUnload);
    }

    // Code friendly: pass your own array each time. null / empty array = just load + activate, no error.
    public void LoadSceneAndSetActive(string sceneName, string[] scenesToUnload)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            debuggerNiAin.Error("Scene to load is not specified!");
            return;
        }

        List<string> unloadList = scenesToUnload == null
            ? new List<string>()
            : new List<string>(scenesToUnload);

        BeginProcess(
            new List<string> { sceneName },
            false,                       // Additive, never Single
            sceneName,                   // Becomes the active scene
            unloadList,
            showLoadingScreenOnAdditive
        );
    }

    // Unloads one scene. Safe to call even if the scene isn't loaded.
    public void UnloadScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            debuggerNiAin.Warn("GameManager: Scene to unload is not specified, skipping.");
            return;
        }

        BeginProcess(
            new List<string>(),
            false,
            null,
            new List<string> { sceneName },
            showLoadingScreenOnAdditive
        );
    }

    // Unloads many scenes one after the other. null / empty array is fine (does nothing).
    public void UnloadScenes(string[] sceneNames)
    {
        if (sceneNames == null || sceneNames.Length == 0) return;

        BeginProcess(
            new List<string>(),
            false,
            null,
            new List<string>(sceneNames),
            showLoadingScreenOnAdditive
        );
    }

    // UnityEvent friendly: unloads everything in the serialized _scenesToUnload array
    public void UnloadConfiguredScenes()
    {
        UnloadScenes(_scenesToUnload);
    }

    // -------------------------------- SCENE QUEUE ---------------------------

    // Method to add a scene to the loading queue
    public void QueueScene(string levelSceneName)
    {
        if (string.IsNullOrEmpty(levelSceneName))
        {
            debuggerNiAin.Error("Scene to queue is not specified!");
            return;
        }

        // Ignore queue requests while a load is in progress
        if (isProcessingScenes)
        {
            debuggerNiAin.Warn($"GameManager: Ignored '{levelSceneName}', scenes are already loading!");
            return;
        }

        // Ignore duplicates already waiting in the queue
        if (_sceneQueue.Contains(levelSceneName))
        {
            debuggerNiAin.Warn($"GameManager: '{levelSceneName}' is already queued.");
            return;
        }

        _sceneQueue.Add(levelSceneName);
    }

    // Method to clear every scene currently waiting in the queue
    public void ClearSceneQueue()
    {
        _sceneQueue.Clear();

        debuggerNiAin.Log("GameManager: Scene queue cleared!");
    }

    // Method to start loading all scenes currently in the queue
    // (first scene = Single, the rest = Additive)
    public void StartQueuedSceneLoading()
    {
        // Evaluates if the Scene Process has been already running
        if (isProcessingScenes)
        {
            debuggerNiAin.Warn("GameManager: A scene process is already running!");
            return;
        }

        // Evaluates if there are no scenes waiting in the queue
        if (_sceneQueue.Count <= 0)
        {
            debuggerNiAin.Warn("GameManager: Cannot start loading because the scene queue is empty!");
            return;
        }

        // Take a snapshot and empty the queue RIGHT NOW,
        // so a failed or interrupted load can never leave stale scenes behind
        List<string> snapshot = new List<string>(_sceneQueue);
        _sceneQueue.Clear();

        string sceneToActivate = setLastQueuedSceneActive ? snapshot[snapshot.Count - 1] : null;

        BeginProcess(snapshot, true, sceneToActivate, null, true);
    }

    // --------------------------- PROCESSOR --------------------------

    // Validates a request, takes the busy lock, and starts the processor
    private void BeginProcess(
        List<string> scenesToLoad,
        bool firstSceneIsSingle,
        string sceneToActivate,
        List<string> scenesToUnload,
        bool showLoadingScreen)
    {
        if (scenesToLoad == null) scenesToLoad = new List<string>();
        if (scenesToUnload == null) scenesToUnload = new List<string>();

        // One request at a time, loads and unloads share the same lock.
        // A request that arrives while busy WAITS and runs right after the current one,
        // so "Load + Unload" fired from the same UnityEvent both work, in order.
        if (isProcessingScenes)
        {
            debuggerNiAin.Log("GameManager: Scene request is waiting for the current one to finish.");

            _pendingRequests.Enqueue(new SceneRequest
            {
                scenesToLoad = scenesToLoad,
                firstSceneIsSingle = firstSceneIsSingle,
                sceneToActivate = sceneToActivate,
                scenesToUnload = scenesToUnload,
                showLoadingScreen = showLoadingScreen
            });

            return;
        }

        // Validate every scene to load BEFORE touching anything
        List<string> validLoads = new List<string>();

        foreach (string name in scenesToLoad)
        {
            if (string.IsNullOrEmpty(name)) continue;
            if (validLoads.Contains(name)) continue;

            if (!Application.CanStreamedLevelBeLoaded(name))
            {
                debuggerNiAin.Error($"GameManager: Scene '{name}' is not in the Build Settings, it was skipped!");
                continue;
            }

            validLoads.Add(name);
        }

        // The caller asked to load something but NOTHING valid was found.
        // Abort completely so we never unload the old scenes with nothing to replace them.
        if (scenesToLoad.Count > 0 && validLoads.Count == 0)
        {
            debuggerNiAin.Error("GameManager: None of the requested scenes could be loaded, request cancelled.");
            return;
        }

        // Nothing to load and nothing to unload = nothing to do (not an error)
        if (validLoads.Count == 0 && scenesToUnload.Count == 0)
        {
            debuggerNiAin.Log("GameManager: Nothing to load or unload.");
            return;
        }

        // The scene to activate must be one we are actually loading
        if (!string.IsNullOrEmpty(sceneToActivate) && !validLoads.Contains(sceneToActivate))
            sceneToActivate = null;

        // Take the lock. It is released inside the processor's finally block.
        isProcessingScenes = true;

        StartCoroutine(ProcessScenes(
            validLoads,
            firstSceneIsSingle,
            sceneToActivate,
            scenesToUnload,
            showLoadingScreen
        ));
    }

    // The ONE coroutine that does every load / activate / unload
    private IEnumerator ProcessScenes(
        List<string> scenesToLoad,
        bool firstSceneIsSingle,
        string sceneToActivate,
        List<string> scenesToUnload,
        bool showLoadingScreen)
    {
        // Tracks whether the "finish" event still needs to be fired
        bool finishEventPending = false;

        // Tracks whether this request already released the lock early
        bool lockReleased = false;

        // try/finally guarantees the lock is released even if something throws
        // or this coroutine ends early. (Before, an exception left it locked forever.)
        try
        {
            if (showLoadingScreen)
            {
                onLoadingScenes?.Invoke();
                finishEventPending = true;
            }

            SetBar(0f);

            // ---------- Work out which scenes will really be unloaded ----------

            HashSet<string> loadSet = new HashSet<string>(scenesToLoad);
            List<string> validUnloads = new List<string>();

            foreach (string name in scenesToUnload)
            {
                // Skip empty entries (empty array slots in the Inspector)
                if (string.IsNullOrEmpty(name)) continue;

                // Never unload a scene we are loading in this same request
                if (loadSet.Contains(name)) continue;

                // Skip duplicates
                if (validUnloads.Contains(name)) continue;

                // Skip scenes that aren't loaded (no error)
                Scene check = SceneManager.GetSceneByName(name);
                if (!check.IsValid() || !check.isLoaded)
                {
                    debuggerNiAin.Log($"GameManager: '{name}' isn't loaded, nothing to unload.");
                    continue;
                }

                validUnloads.Add(name);
            }

            int totalSteps = scenesToLoad.Count + validUnloads.Count;
            int completedSteps = 0;
            bool allLoadsOk = true;

            debuggerNiAin.Log(
                $"GameManager: Processing request. Load: {scenesToLoad.Count}, Unload: {validUnloads.Count}"
            );

            // ---------------------------- LOAD ----------------------------

            for (int i = 0; i < scenesToLoad.Count; i++)
            {
                string sceneName = scenesToLoad[i];

                // Only the very first scene of a queue request can be Single
                bool useSingle = firstSceneIsSingle && i == 0;

                // Additive load of a scene that is already open would duplicate it. Skip the load.
                if (!useSingle)
                {
                    Scene existing = SceneManager.GetSceneByName(sceneName);

                    if (existing.IsValid() && existing.isLoaded)
                    {
                        debuggerNiAin.Log($"GameManager: '{sceneName}' is already loaded, skipping the load.");
                        completedSteps++;
                        continue;
                    }
                }

                debuggerNiAin.Log(
                    $"GameManager: Loading '{sceneName}' using LoadSceneMode.{(useSingle ? "Single" : "Additive")}."
                );

                AsyncOperation sceneLoad = SceneManager.LoadSceneAsync(
                    sceneName,
                    useSingle ? LoadSceneMode.Single : LoadSceneMode.Additive
                );

                // Unity failed to create the loading operation
                if (sceneLoad == null)
                {
                    debuggerNiAin.Error($"GameManager: Failed to create loading operation for '{sceneName}'.");
                    allLoadsOk = false;
                    completedSteps++;
                    continue;
                }

                // Wait for this scene while moving the bar
                while (!sceneLoad.isDone)
                {
                    // AsyncOperation.progress goes 0.0 - 0.9, convert it to 0.0 - 1.0
                    float currentSceneProgress = Mathf.Clamp01(sceneLoad.progress / 0.9f);

                    float targetProgress = (completedSteps + currentSceneProgress) / totalSteps;

                    StepBar(targetProgress);

                    yield return null;
                }

                completedSteps++;

                // Let the bar catch up to the finished step
                yield return FillBarTo((float)completedSteps / totalSteps);

                debuggerNiAin.Log(
                    $"GameManager: Finished loading '{sceneName}'. Progress: {completedSteps}/{totalSteps}"
                );
            }

            // ---------------------------- ACTIVATE ----------------------------

            // Must happen AFTER loading and BEFORE unloading,
            // so Unity never has to unload the scene that is currently active
            if (!string.IsNullOrEmpty(sceneToActivate))
            {
                Scene toActivate = SceneManager.GetSceneByName(sceneToActivate);

                if (toActivate.IsValid() && toActivate.isLoaded && SceneManager.SetActiveScene(toActivate))
                {
                    debuggerNiAin.Log($"GameManager: '{sceneToActivate}' is now the active scene.");
                }
                else
                {
                    debuggerNiAin.Error($"GameManager: Could not set '{sceneToActivate}' as the active scene.");
                    allLoadsOk = false;
                }
            }

            // ---------------------------- UNLOAD ----------------------------

            if (!allLoadsOk && validUnloads.Count > 0)
            {
                // Something we wanted to load failed. Keep the old scenes
                // so the player is never left with an empty game.
                debuggerNiAin.Warn("GameManager: A load failed, the unload step was skipped to keep the old scenes.");
            }
            else
            {
                HashSet<string> unloadSet = new HashSet<string>(validUnloads);

                foreach (string name in validUnloads)
                {
                    yield return UnloadSceneRoutine(name, unloadSet);

                    completedSteps++;
                    yield return FillBarTo((float)completedSteps / totalSteps);
                }
            }

            // ---------------------------- FINISH ----------------------------

            // Make absolutely sure the loading bar reaches 100%
            yield return FillBarTo(1.0f);

            debuggerNiAin.Log("GameManager: Scene request finished!");

            if (showLoadingScreen)
            {
                if (enableLoadScreenDelay)
                {
                    debuggerNiAin.Log("Loading Screen Timer Started");

                    // Realtime so it still works when Time.timeScale is 0 (paused)
                    yield return new WaitForSecondsRealtime(loadScreenDelay);

                    debuggerNiAin.Log("Loading Screen Timer Finished");
                }

                // Release the lock BEFORE the event, so listeners can start the next request
                isProcessingScenes = false;
                lockReleased = true;
                finishEventPending = false;

                // The finish event now ALWAYS fires (before, it only fired if the delay was enabled)
                onFinishLoadingScenes?.Invoke();
            }
        }
        finally
        {
            // Always runs. The lock can no longer get stuck on.
            // Skipped if it was already released early, because a listener of the finish
            // event may have started a NEW request and taken the lock again.
            if (!lockReleased)
                isProcessingScenes = false;

            // If we died halfway, still close the loading screen
            if (finishEventPending)
                onFinishLoadingScenes?.Invoke();

            // Start whatever was waiting (does nothing if a new request already took the lock)
            RunNextPendingRequest();
        }
    }

    // Starts the next waiting request, if any
    private void RunNextPendingRequest()
    {
        // BeginProcess may reject a request (bad scene name, nothing to do),
        // so keep going until one takes the lock or the list is empty
        while (!isProcessingScenes && _pendingRequests.Count > 0)
        {
            SceneRequest next = _pendingRequests.Dequeue();

            BeginProcess(
                next.scenesToLoad,
                next.firstSceneIsSingle,
                next.sceneToActivate,
                next.scenesToUnload,
                next.showLoadingScreen
            );
        }
    }

    // Unloads one scene safely and waits until Unity is done
    private IEnumerator UnloadSceneRoutine(string sceneName, HashSet<string> alsoBeingUnloaded)
    {
        Scene scene = SceneManager.GetSceneByName(sceneName);

        // Not loaded anymore? Nothing to do, no error.
        if (!scene.IsValid() || !scene.isLoaded)
        {
            debuggerNiAin.Log($"GameManager: '{sceneName}' isn't loaded, nothing to unload.");
            yield break;
        }

        // Never touch the DontDestroyOnLoad scene (this manager lives there)
        if (scene == gameObject.scene)
        {
            debuggerNiAin.Warn($"GameManager: '{sceneName}' holds the GameManager, it cannot be unloaded.");
            yield break;
        }

        // Unity cannot unload the last remaining scene
        if (SceneManager.sceneCount <= 1)
        {
            debuggerNiAin.Warn($"GameManager: '{sceneName}' is the last loaded scene, it cannot be unloaded.");
            yield break;
        }

        // If this is the active scene, hand "active" to another scene first
        if (scene == SceneManager.GetActiveScene())
        {
            if (!TrySetFallbackActiveScene(scene, alsoBeingUnloaded))
            {
                debuggerNiAin.Warn($"GameManager: No other scene could become active, '{sceneName}' was kept.");
                yield break;
            }
        }

        AsyncOperation unload = SceneManager.UnloadSceneAsync(scene);

        if (unload == null)
        {
            debuggerNiAin.Warn($"GameManager: Could not unload '{sceneName}'.");
            yield break;
        }

        yield return unload;

        debuggerNiAin.Log($"GameManager: Unloaded scene '{sceneName}'.");
    }

    // Picks any other loaded scene (that isn't about to be unloaded) and makes it active
    private bool TrySetFallbackActiveScene(Scene excluded, HashSet<string> alsoExclude)
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene candidate = SceneManager.GetSceneAt(i);

            if (!candidate.isLoaded) continue;
            if (candidate == excluded) continue;
            if (candidate == gameObject.scene) continue;
            if (alsoExclude != null && alsoExclude.Contains(candidate.name)) continue;

            if (SceneManager.SetActiveScene(candidate))
            {
                debuggerNiAin.Log($"GameManager: '{candidate.name}' became the active scene.");
                return true;
            }
        }

        return false;
    }

    // ---------------------------- LOADING BAR ---------------------------

    // Jumps the bar to a value instantly
    private void SetBar(float value)
    {
        _barProgress = value;
        ApplyBar();
    }

    // Writes the stored progress to the Image, if the Image still exists
    private void ApplyBar()
    {
        // Unity's null check also catches destroyed objects
        if (_loadingBar != null)
            _loadingBar.fillAmount = _barProgress;
    }

    // Moves the bar one frame toward a target. Without a bar it just jumps to the target.
    private void StepBar(float target)
    {
        if (_loadingBar == null)
        {
            _barProgress = target;
        }
        else
        {
            // Unscaled so the bar still moves when Time.timeScale is 0
            _barProgress = Mathf.MoveTowards(
                _barProgress,
                target,
                Mathf.Max(loadingBarSpeed, 0.01f) * Time.unscaledDeltaTime
            );
        }

        ApplyBar();
    }

    // Smoothly fills the bar up to the target value
    private IEnumerator FillBarTo(float target)
    {
        while (_barProgress < target - 0.0001f)
        {
            StepBar(target);
            yield return null;
        }
    }
}