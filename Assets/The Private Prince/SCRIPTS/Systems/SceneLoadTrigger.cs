using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

using UnityEngine.SceneManagement;

public class SceneLoadTrigger : MonoBehaviour
{
    // ------------------------- VARIABLES -------------------------

    [SerializeField] private SceneField[] _scenesToLoad;
    [SerializeField] private SceneField[] _scenesToUnload;

    [Header("Set Here Only When Necessary (IT'S OKAY TO LEAVE IT EMPTY)")]

    [SerializeField] private string regionName = null; // Current region where the saving happen

    // -------------------------- METHODS -------------------------

    // ...
    private void OnTriggerEnter(Collider actor)
    {
        // ...
        if (actor.CompareTag("Player")) 
        {
            LoadScenes();
            //UnloadScenes();

            if (regionName != null && regionName.Length > 0)
            {
                // ...
                SaveManager.Instance.previousRegion = SaveManager.Instance.currentRegionPoint;

                // ...
                SaveManager.Instance.currentRegionPoint = regionName;
                SaveManager.Instance.onEnteringNewRegion?.Invoke();
            }

            //// ...
            //if (actor.transform.position.x < this.transform.position.x &&
            //    regionName != null && regionName.Length > 0)
            //{
            //    // ...
            //    SaveManager.Instance.previousRegion = SaveManager.Instance.currentRegionPoint;

            //    // ...
            //    SaveManager.Instance.currentRegionPoint = regionName;
            //    SaveManager.Instance.onEnteringNewRegion?.Invoke();
            //}
        }
    }

    // ...
    private void OnTriggerExit(Collider actor)
    {
        // ...
        if (actor.CompareTag("Player"))
        {
            if (regionName != null && regionName.Length > 0)
            {
                // ...
                SaveManager.Instance.currentRegionPoint = SaveManager.Instance.previousRegion;
                SaveManager.Instance.onEnteringNewRegion?.Invoke();
            }

            //if (actor.transform.position.x < this.transform.position.x &&
            //    (regionName != null && regionName.Length > 0))
            //{
            //    // ...
            //    SaveManager.Instance.currentRegionPoint = SaveManager.Instance.previousRegion;
            //    SaveManager.Instance.onEnteringNewRegion?.Invoke();
            //}
        }
    }

    // ...
    public void LoadScenes() 
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
    public void UnloadScenes()
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