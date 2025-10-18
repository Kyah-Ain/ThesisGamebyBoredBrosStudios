using UnityEngine;
using UnityEngine.SceneManagement;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance;

    [SerializeField] private string mainMenuScene = "MainMenu"; // Set this to your main menu scene name

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        // Only handle ESC in game scenes (not in main menu)
        if (Input.GetKeyDown(KeyCode.Escape) && !IsInMainMenuScene())
        {
            ReturnToMainMenu();
        }
    }

    private bool IsInMainMenuScene()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        return currentScene == mainMenuScene;
    }

    public void ReturnToMainMenu()
    {
        Debug.Log("ESC pressed - returning to Main Menu");
        SceneManager.LoadScene(mainMenuScene);
    }
}