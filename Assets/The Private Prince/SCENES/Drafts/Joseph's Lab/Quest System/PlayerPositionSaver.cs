using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerPositionSaver : MonoBehaviour
{
    public static PlayerPositionSaver Instance;

    private Vector3 savedPosition;
    private string savedScene;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"Scene loaded: {scene.name}");

        // Restore player position when returning to overworld
        if (scene.name == "Prototype Gameworld")
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null && savedScene == "Prototype Gameworld")
            {
                player.transform.position = savedPosition;
                Debug.Log($"Restored player to position: {savedPosition}");
            }
        }
    }

    public void SavePlayerPosition(Vector3 position, string sceneName)
    {
        savedPosition = position;
        savedScene = sceneName;
        Debug.Log($"Saved position: {position} in scene: {sceneName}");
    }
}