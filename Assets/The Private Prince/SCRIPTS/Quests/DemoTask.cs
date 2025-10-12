using UnityEngine; // Grants access to Unity's core classes and functions, such as MonoBehaviour, GameObject, Transform, etc.

public class DemoTask : MonoBehaviour
{
    // ------------------------- VARIABLES -------------------------

    [Header("Trigger Settings")]
    public string requiredTag = "Player";

    // -------------------------- METHODS ---------------------------

    private void OnTriggerEnter(Collider actor)
    {
        // Ensures that only objects with the specified tag can trigger the task completion
        if (actor.CompareTag(requiredTag))
        {
            // Auto-find and notify the TaskManager - no references needed!
            if (TaskManager.Instance != null)
            {
                TaskManager.Instance.CompleteTask();
            }
            else
            {
                Debug.LogWarning("TaskManager instance not found!");
            }
        }
    }
}