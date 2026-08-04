using TMPro;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PuzzleTrigger : MonoBehaviour
{
    [SerializeField] private PuzzleController controller;
    [SerializeField] private TextMeshProUGUI triggerText;

    private bool playerInside;

    private void Update()
    {
        if (!playerInside)
            return;

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            HandleInteraction();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInside = true;

        if (triggerText != null)
            triggerText.gameObject.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInside = false;

        if (triggerText != null)
            triggerText.gameObject.SetActive(false);
    }

    private void HandleInteraction()
    {
        PuzzleManager manager = PuzzleManager.Instance;

        if (manager == null)
            return;

        if (manager.IsPuzzlePaused(controller))
        {
            manager.ResumePuzzle();
        }
        else if (manager.IsPuzzleActive(controller))
        {
            manager.PausePuzzle();
        }
        else if (manager.State == PuzzleState.Idle)
        {
            manager.StartPuzzle(controller);
        }
    }
}