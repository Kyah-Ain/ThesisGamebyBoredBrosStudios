using UnityEngine;

public class PuzzleMovementBlocker : MonoBehaviour
{
    private void OnEnable()
    {
        PuzzleManager.Instance.OnPuzzleStarted += OnPuzzleStarted;
        PuzzleManager.Instance.OnPuzzleEnded += OnPuzzleEnded;
    }

    private void OnDisable()
    {
        if (PuzzleManager.Instance == null)
            return;

        PuzzleManager.Instance.OnPuzzleStarted -= OnPuzzleStarted;
        PuzzleManager.Instance.OnPuzzleEnded -= OnPuzzleEnded;
    }

    private void OnPuzzleStarted(PuzzleController puzzle)
    {
        Time.timeScale = 0f;
    }

    private void OnPuzzleEnded(PuzzleController puzzle, PuzzleResult result)
    {
        Time.timeScale = 1f;
    }
}