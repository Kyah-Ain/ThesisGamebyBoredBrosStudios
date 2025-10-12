using System.Collections; // Grants access to collections and data structures like ArrayList, Hashtable, etc.
using System.Collections.Generic; // Grants access to collections and data structures like ArrayList, Hashtable, List, Dictionary, etc.
using UnityEngine; // Grants access to Unity's core classes and functions, such as MonoBehaviour, GameObject, Transform, etc.
using TMPro; // Grants access to TextMeshPro features such as TextMeshProUGUI, TMP_InputField, etc.

public class TaskManager : MonoBehaviour
{
    // ------------------------- VARIABLES -------------------------

    // Singleton instance for global access (On Reading Onleh)
    public static TaskManager Instance { get; private set; }

    [Header("TASK SYSTEM")]
    public int totalTasks = 4;
    public int completedTasks = 0;

    [Header("DISPLAY")]
    public TextMeshProUGUI taskOutput; // Reference to the UI text component on the game for displaying tasks

    // -------------------------- METHODS ---------------------------

    // Awake is called when the script instance is being loaded
    private void Awake()
    {
        // Check for existing instance and destroy duplicates if theres any
        if (Instance == null)
        {
            // Sets the singleton instance to the object this script is attached to
            Instance = this;
        }
        else
        {
            // If an instance already exists, destroy this duplicate to enforce singleton pattern
            Destroy(gameObject);
        }

        // Update the task display and reset tasks at start
        UpdateDisplay();
        ResetTasks();
    }

    // Method to progress through tasks list
    public void CompleteTask()
    {
        completedTasks++;
        UpdateDisplay();
    }

    // Method to display current task status
    private void UpdateDisplay()
    {
        // Output variations based on number of completed tasks
        switch (completedTasks)
        {
            case 0:
                taskOutput.text = null;
                break;
            case 1:
                taskOutput.text = $"Task 1: Infiltrate Enemy Base ({completedTasks}/{totalTasks - 1})";
                break;
            case 2:
                taskOutput.text = $"Task 1: Infiltrate Enemy Base ({completedTasks}/{totalTasks - 1})";
                break;
            case 3:
                taskOutput.text = $"Task 1: Infiltrate Enemy Base (3/3) \n" +
                                  $"Task 2: Defeat 3 Enemies(0/3)";
                break;
            case 4:
                taskOutput.text = "Congratulations, you passed the test!!";
                break;
            default:
                taskOutput.text = $"Progress: {completedTasks}/{totalTasks}";
                break;
        }
    }

    // Method to reset tasks to initial state
    public void ResetTasks()
    {
        completedTasks = 0;
        UpdateDisplay();
    }
}