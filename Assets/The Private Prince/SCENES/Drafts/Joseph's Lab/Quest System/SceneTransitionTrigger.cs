using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionTrigger : MonoBehaviour
{
    [SerializeField] private string sceneToLoad = "Ain's 2D Charac Lab"; // Your combat scene name

    public void TransitionToCombatScene()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null && GameDataManager.Instance != null)
        {
            GameDataManager.Instance.SaveBeforeCombat(
                player.transform.position,
                SceneManager.GetActiveScene().name
            );
        }

        SceneManager.LoadScene(sceneToLoad);
    }
}