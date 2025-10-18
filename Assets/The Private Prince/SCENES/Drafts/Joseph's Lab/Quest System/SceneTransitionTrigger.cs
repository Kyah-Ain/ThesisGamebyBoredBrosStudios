using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionTrigger : MonoBehaviour
{
    [SerializeField] private string sceneToLoad = "Ain's 2D Charac Lab"; // Your exact combat scene name;

    public void TransitionToCombatScene()
    {
        Debug.Log($"Loading combat scene: {sceneToLoad}");
        SceneManager.LoadScene(sceneToLoad);
    }
}