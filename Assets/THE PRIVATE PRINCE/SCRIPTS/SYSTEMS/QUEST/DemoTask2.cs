using UnityEngine; // Grants access to Unity's core classes and functions, such as MonoBehaviour, GameObject, Transform, etc.

public class DemoTask2 : MonoBehaviour
{
    // ------------------------- VARIABLES -------------------------

    public static DemoTask2 Instance { get; private set; }

    [Header("Enemies Left")]
    public int enemiesDefeated = 0;
    public int enemiesToDefeat;

    // -------------------------- METHODS ---------------------------

    // ...
    public void Start()
    {
        enemiesToDefeat = 3; // Ensure this is set
    }

    // ...
    private void Awake()
    {
        // Singleton pattern initialization
        if (Instance == null)
        {
            Instance = this;
            Debug.Log("DemoTask2 instance created and assigned to Instance");
        }
        else
        {
            Debug.LogWarning("Multiple DemoTask2 instances detected! Destroying duplicate.");
            Destroy(gameObject);
        }
    }

    // ...
    public void UpdateSlainedEnemies()
    {
        if (TaskManager.Instance.completedTasks == 3)
        {
            // ...
            enemiesDefeated += 1;

            UpdateTaskDisplay();

            if (enemiesDefeated >= enemiesToDefeat)
            {
                TaskManager.Instance.CompleteTask();
            }

            Debug.Log($"Enemy defeated! Total: {enemiesDefeated}/{enemiesToDefeat}");

            //if (playerKill.playerHits == true)
            //{
            //    // ...
            //    enemiesDefeated += 1;

            //    UpdateTaskDisplay();

            //    if (enemiesDefeated >= enemiesToDefeat)
            //    {
            //        TaskManager.Instance.CompleteTask();
            //    }

            //    Debug.Log($"Enemy defeated! Total: {enemiesDefeated}/{enemiesToDefeat}");
            //}
        }
        //// ...
        //playerKill.playerHits = false;
    }

    // Add this method to update the display properly
    private void UpdateTaskDisplay()
    {
        // Only update if we're still on task 3
        if (TaskManager.Instance.completedTasks == 3)
        {
            TaskManager.Instance.taskOutput.text = $"Task 2: Defeat 3 Enemies({enemiesDefeated}/{enemiesToDefeat})";
        }
    }
}