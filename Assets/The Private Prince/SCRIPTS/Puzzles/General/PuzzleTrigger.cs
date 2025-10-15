using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(Collider))]
public class PuzzleTrigger : MonoBehaviour
{
    [Header("Puzzle Trigger Settings")]
    public PuzzleWall puzzleWall;
    public TextMeshProUGUI triggerText;

    private bool playerInside = false;

    private void Update()
    {
        if (playerInside && Input.GetKeyDown(KeyCode.Tab))
        {
            HandlePuzzleTab();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            triggerText.gameObject.SetActive(true);
            playerInside = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            triggerText.gameObject.SetActive(false);
            playerInside = false;
        }
    }

    private void HandlePuzzleTab()
    {
        if (puzzleWall == null || puzzleWall.puzzle == null) return;
        if (puzzleWall.HasActiveEnemies) return;

        var pm = PuzzleManager.Instance;
        if (pm == null) return;

        if (pm.HasPausedPuzzle(puzzleWall.puzzle))
        {
            pm.ResumePuzzle();
        }
        else if (pm.State == PuzzleState.InProgress && pm.ActivePuzzle == puzzleWall.puzzle)
        {
            pm.PausePuzzle();
        }
        else if (pm.State == PuzzleState.Idle)
        {
            pm.StartPuzzle(puzzleWall.puzzle);
        }
    }
}
