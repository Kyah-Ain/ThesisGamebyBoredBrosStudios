using UnityEngine; // Grants access to Unity's core classes and functions, such as MonoBehaviour, GameObject, Transform, etc.

public class DemoTask2 : MonoBehaviour
{
    // ------------------------- VARIABLES -------------------------

    public Player2Point5D playerKill; // ...

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
    public void Update()
    {
        UpdateSlainedEnemies();
    }

    // ...
    public void UpdateSlainedEnemies()
    {
        if (TaskManager.Instance.completedTasks == 3)
        {
            if (playerKill.playerAttacked == true)
            {
                enemiesDefeated += 1;
                playerKill.playerAttacked = false; // Reset immediately

                UpdateTaskDisplay();

                if (enemiesDefeated >= enemiesToDefeat)
                {
                    TaskManager.Instance.CompleteTask();
                }

                Debug.Log($"Enemy defeated! Total: {enemiesDefeated}/{enemiesToDefeat}");
            }
        }
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