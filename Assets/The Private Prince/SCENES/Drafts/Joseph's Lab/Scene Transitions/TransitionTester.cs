using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TransitionTester : MonoBehaviour
{
    [SerializeField] private ScanTransition transition;
    [SerializeField] private string startScene;
    [SerializeField] private string nextScene;
    [SerializeField] private bool loadStartSceneOnPlay = true;

    [Header("Manual control")]
    [SerializeField] private bool manualControl;
    [Range(0f, 2f)]
    [SerializeField] private float scrub;

    private string currentScene;
    private bool wasManual;
    private bool busy;

    private IEnumerator Start()
    {
        if (loadStartSceneOnPlay)
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

    public void Play()
    {
        GoTo(currentScene == startScene ? nextScene : startScene);
    }

    public void GoTo(string target)
    {
        if (busy || target == currentScene)
        {
            return;
        }

        manualControl = false;
        transition.RunRoutine(() => SwapRoutine(target));
    }

    public void ResetToStart()
    {
        if (busy || currentScene == startScene)
        {
            return;
        }

        manualControl = false;
        transition.Hide();
        StartCoroutine(SwapRoutine(startScene));
    }

    private IEnumerator SwapRoutine(string target)
    {
        busy = true;
        string previous = currentScene;
        yield return LoadAndActivate(target);

        if (!string.IsNullOrEmpty(previous) && previous != target)
        {
            yield return SceneManager.UnloadSceneAsync(previous);
        }

        busy = false;
    }

    private IEnumerator LoadAndActivate(string sceneName)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

        while (!operation.isDone)
        {
            yield return null;
        }

        SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));
        currentScene = sceneName;
    }
}