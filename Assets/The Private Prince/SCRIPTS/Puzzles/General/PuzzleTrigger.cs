using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PuzzleTrigger : MonoBehaviour
{
    [Header("Puzzle Trigger Settings")]
    public PuzzleWall puzzleWall;

    private bool playerInside = false; // Keep boolean in case if puzzle is entered by pressing a button while in trigger

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = true;
            StartPuzzle();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;
        }
    }

    private void StartPuzzle()
    {
        if (puzzleWall == null || puzzleWall.puzzle == null) return;
        PuzzleManager.Instance.StartPuzzle(puzzleWall.puzzle);
    }
}
