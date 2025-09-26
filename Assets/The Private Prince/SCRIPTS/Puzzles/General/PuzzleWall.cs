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
        // Subscribe to puzzle results via PuzzleManager
        // PuzzleBase calls EndPuzzle, PuzzleManager controls wall state
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
