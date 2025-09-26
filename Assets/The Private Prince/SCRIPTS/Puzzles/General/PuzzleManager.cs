using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PuzzleResult { None, Solved, Failed }
public enum PuzzleState { Idle, InProgress, Paused, Completed }

public class PuzzleManager : MonoBehaviour
{
    public static PuzzleManager Instance;
    private PuzzleBase activePuzzle;
    private PuzzleState state = PuzzleState.Idle;
    [SerializeField] private GameObject puzzleUIRoot;

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

        puzzleUIRoot.SetActive(true);
        activePuzzle.StartPuzzle();
    }

    public void EndPuzzle(PuzzleResult result)
    {
        if (activePuzzle == null) return;

        state = PuzzleState.Completed;
        activePuzzle.EndPuzzle(result);

        puzzleUIRoot.SetActive(false);
        activePuzzle = null;
    }

    public void PausePuzzle()
    {
        if (activePuzzle == null || !activePuzzle.Pausable) return;

        state = PuzzleState.Paused;
        activePuzzle.PausePuzzle();
        puzzleUIRoot.SetActive(false);
    }

    public void ResumePuzzle()
    {
        if (activePuzzle == null || state != PuzzleState.Paused) return;

        state = PuzzleState.InProgress;
        activePuzzle.ResumePuzzle();
        puzzleUIRoot.SetActive(true);
    }
}
