using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionTrigger : MonoBehaviour
{
    [SerializeField] private string sceneToLoad = "CombatArena";

    public void TransitionToCombatScene()
    {
        Debug.Log($"Loading combat scene: {sceneToLoad}");
        SceneManager.LoadScene(sceneToLoad);
    }
}