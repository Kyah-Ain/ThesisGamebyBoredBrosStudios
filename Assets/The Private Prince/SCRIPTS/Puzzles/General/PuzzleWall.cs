using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleWall : MonoBehaviour
{
    [Header("Wall Settings")]
    public PuzzleBase puzzle;
    public PuzzleEnemies puzzleEnemies;

    private bool puzzleResolved = false;

    private void OnEnable()
    {
        Subscribe();
        Debug.Log($"{name} subscribing to PuzzleManager");
    }

    private void Start()
    {
        Subscribe();
    }

    private void OnDisable()
    {
        if (PuzzleManager.Instance != null)
            PuzzleManager.Instance.OnPuzzleEnded -= HandlePuzzleResult;
        Debug.Log($"{name} unsubscribing to PuzzleManager");
    }

    private void Subscribe()
    {
        if (PuzzleManager.Instance != null)
        {
            PuzzleManager.Instance.OnPuzzleEnded -= HandlePuzzleResult; // Prevent multiple subscriptions
            PuzzleManager.Instance.OnPuzzleEnded += HandlePuzzleResult;
        }
    }

    private void HandlePuzzleResult(PuzzleBase endedPuzzle, PuzzleResult result)
    {
        if (endedPuzzle != this.puzzle) return;

        Debug.Log($"Wall {name} received end event for puzzle {endedPuzzle?.name}");
        if (result == PuzzleResult.Solved) OnPuzzleSolved();
        else if (result == PuzzleResult.Failed) OnPuzzleFailed();
    }

    public void OnPuzzleSolved()
    {
        if (puzzleResolved) return;
        puzzleResolved = true;
        DestroyWall();
    }

    public void OnPuzzleFailed()
    {
        if (puzzleResolved) return;

        if (puzzleEnemies != null)
        {
            puzzleEnemies.SpawnEnemies(OnEnemiesCleared);
        }
        else
        {
            DestroyWall();
        }
    }

    private void OnEnemiesCleared()
    {
        DestroyWall();
    }

    private void DestroyWall()
    {
        Destroy(gameObject);
    }
}
