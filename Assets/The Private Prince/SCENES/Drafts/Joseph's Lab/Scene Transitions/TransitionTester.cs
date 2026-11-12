using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TransitionTester : MonoBehaviour
{
    [SerializeField] private ScanTransition transition;
    [SerializeField] private string startScene;
    [SerializeField] private string nextScene;
    [SerializeField] private bool loadStartSceneOnPlay = true;

    [Header("Loading rules")]
    [Tooltip("If off, a scene that is already loaded will not be loaded again.")]
    [SerializeField] private bool allowLoadingAlreadyLoadedScenes = false;

    [Header("Manual control")]
    [SerializeField] private bool manualControl;
    [Range(0f, 2f)]
    [SerializeField] private float scrub;

    [Header("Timing")]
    [Tooltip("Extra time the screen stays covered after the scene has finished loading. 0 = no extra delay.")]
    [Min(0f)]
    [SerializeField] private float afterLoadDelay = 0f;
    
    private Scene currentScene; // handle of the scene this script treats as current
    private bool wasManual;
    private bool busy;

    private IEnumerator Start()
    {
        // If the start scene is already open, adopt it as the current scene
        Scene existing = SceneManager.GetSceneByName(startScene);
        if (existing.IsValid() && existing.isLoaded)
        {
            SceneManager.SetActiveScene(existing);
            currentScene = existing;
        }
        else if (loadStartSceneOnPlay)
        {
            yield return LoadAndActivate(startScene);
        }
    }

    private void Update()
    {
        if (manualControl)
        {
            transition.Scrub(scrub);
            wasManual = true;
        }
        else if (wasManual)
        {
            transition.Hide();
            wasManual = false;
        }
    }

    public void SetSceneToLoad(string scene)
    {
        nextScene = scene;
    }

    public void SetSceneToUnload(string scene)
    {
        startScene = scene;
    }

    public void SetAllowLoadingAlreadyLoadedScenes(bool allow)
    {
        allowLoadingAlreadyLoadedScenes = allow;
    }

    public void Play()
    {
        GoTo(nextScene);
    }
    
    public void PlayAnimationOnly(float duration = 1f)
    {
        if (busy)
        {
            return;
        }

        manualControl = false;
        transition.Play(duration);
    }

    public void GoTo(string target)
    {
        if (busy || !CanLoad(target))
        {
            return;
        }

        manualControl = false;
        transition.RunRoutine(() => SwapRoutine(target));
    }

    public void ResetToStart()
    {
        if (busy || !CanLoad(startScene))
        {
            return;
        }

        manualControl = false;
        transition.Hide();
        StartCoroutine(SwapRoutine(startScene, false));
    }

    private bool CanLoad(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            return false;
        }

        if (allowLoadingAlreadyLoadedScenes)
        {
            return true;
        }

        Scene scene = SceneManager.GetSceneByName(sceneName);
        bool alreadyLoaded = scene.IsValid() && scene.isLoaded;

        if (alreadyLoaded)
        {
            Debug.LogWarning($"[TransitionTester] '{sceneName}' is already loaded. Enable 'Allow Loading Already Loaded Scenes' to load it again.");
        }

        return !alreadyLoaded;
    }

    private IEnumerator SwapRoutine(string target, bool useDelay = true)
    {
        busy = true;

        Scene previous = currentScene.IsValid()
            ? currentScene
            : SceneManager.GetSceneByName(startScene);

        yield return LoadAndActivate(target);

        if (previous.IsValid() && previous.isLoaded && previous != currentScene)
        {
            yield return SceneManager.UnloadSceneAsync(previous);
        }

        if (useDelay && afterLoadDelay > 0f)
        {
            yield return new WaitForSecondsRealtime(afterLoadDelay);
        }

        busy = false;
    }

    private IEnumerator LoadAndActivate(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            yield break;
        }

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        if (operation == null) yield break; // scene not in Build Settings or name is wrong

        while (!operation.isDone)
        {
            yield return null;
        }

        // The scene we just added is the last one in the list, which stays correct even with duplicates
        Scene loaded = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
        SceneManager.SetActiveScene(loaded);
        currentScene = loaded;
    }
}