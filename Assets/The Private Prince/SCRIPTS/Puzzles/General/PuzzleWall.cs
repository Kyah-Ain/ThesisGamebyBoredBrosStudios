using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleWall : MonoBehaviour
{
    [Header("Wall Settings")]
    public PuzzleBase puzzle;
    public PuzzleEnemies puzzleEnemies;
    public bool replayableAfterFailure = false;

    private bool puzzleResolved = false;

    public bool HasActiveEnemies
    {
        get
        {
            return puzzleEnemies != null && puzzleEnemies.HasAliveEnemies;
        }
    }

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
        if (replayableAfterFailure)
        {
            puzzleResolved = false;
            if (puzzle != null)
            {
                puzzle.ResetPuzzle();
            }
        }
        else
        {
            DestroyWall();
        }

    }

    private void DestroyWall()
    {
        gameObject.SetActive(false);
    }
}
