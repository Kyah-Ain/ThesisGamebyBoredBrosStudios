using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PuzzleResult { None, Solved, Failed }
public enum PuzzleState { Idle, InProgress, Paused, Completed }

public class PuzzleManager : MonoBehaviour
{
    public static PuzzleManager Instance;
    public event System.Action<PuzzleBase> OnPuzzleStarted;
    public event System.Action<PuzzleBase, PuzzleResult> OnPuzzleEnded;

    private PuzzleBase activePuzzle;
    private PuzzleState state = PuzzleState.Idle;

    public PuzzleBase ActivePuzzle => activePuzzle;
    public PuzzleState State => state;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void StartPuzzle(PuzzleBase puzzle)
    {
        if (state == PuzzleState.InProgress) return;

        activePuzzle = puzzle;
        state = PuzzleState.InProgress;

        activePuzzle.StartPuzzle();

        OnPuzzleStarted?.Invoke(puzzle);
    }

    public void EndPuzzle(PuzzleResult result)
    {
        if (activePuzzle == null) return;

        PuzzleBase finished = activePuzzle; // Capture finished puzzle reference
        state = PuzzleState.Completed; // Set state to Completed before calling EndPuzzle
        finished.EndPuzzle(result); // Call EndPuzzle on the finished puzzle
        Debug.Log($"Finished {finished.name} with result: {result}");
        OnPuzzleEnded?.Invoke(finished, result); // Invoke event with finished puzzle reference
        activePuzzle = null; // Clear active puzzle reference
        state = PuzzleState.Idle; // Reset state to Idle to allow new puzzles to start
    }

    public void PausePuzzle()
    {
        if (activePuzzle == null || !activePuzzle.Pausable) return;

        state = PuzzleState.Paused;
        activePuzzle.PausePuzzle();
    }

    public void ResumePuzzle()
    {
        if (activePuzzle == null || state != PuzzleState.Paused) return;

        state = PuzzleState.InProgress;
        activePuzzle.ResumePuzzle();
    }

    public bool HasPausedPuzzle(PuzzleBase puzzle)
    {
        return (state == PuzzleState.Paused && activePuzzle == puzzle);
    }
}
