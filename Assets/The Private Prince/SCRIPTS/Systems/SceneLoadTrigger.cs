using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;

public class SceneLoadTrigger : MonoBehaviour
{
    // ------------------------- VARIABLES -------------------------

    [SerializeField] private SceneField[] _scenesToLoad;
    [SerializeField] private SceneField[] _scenesToUnload;

    // -------------------------- METHODS -------------------------

    // ...
    private void OnTriggerEnter(Collider actor)
    {
        // ...
        if (actor.CompareTag("Player")) 
        {
            LoadScenes();
            UnloadScenes();
        }
    }

    // ...
    private void LoadScenes() 
    {
        // ...
        for (int i = 0; i < _scenesToLoad.Length; i++) 
        {
            // ...
            bool isSceneLoaded = false;

            // ...
            for (int j = 0; j < SceneManager.sceneCount; j++) 
            {
                // ...
                Scene loadedScene = SceneManager.GetSceneAt(j);

                // ...
                if (loadedScene.name == _scenesToLoad[i].SceneName) 
                {
                    isSceneLoaded = true;
                    break;
                }
            }

            if (!isSceneLoaded) 
            {
                SceneManager.LoadSceneAsync(_scenesToLoad[i], LoadSceneMode.Additive);
            }
        }
    }

    // ...
    private void UnloadScenes()
    {
        // ...
        for (int i = 0; i < _scenesToUnload.Length; i++)
        {
            // ...
            for (int j = 0; j < SceneManager.sceneCount; j++)
            {
                // ...
                Scene loadedScene = SceneManager.GetSceneAt(j);

                // ...
                if (loadedScene.name == _scenesToUnload[i].SceneName)
                {
                    SceneManager.UnloadSceneAsync(_scenesToUnload[i]);
                }
            }
        }
    }
}
