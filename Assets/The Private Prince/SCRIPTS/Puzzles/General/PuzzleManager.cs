using System;
using UnityEngine;

public enum PuzzleState
{
    Idle,
    Playing,
    Paused
}

public class PuzzleManager : MonoBehaviour
{
    public static PuzzleManager Instance { get; private set; }

    public event Action<PuzzleController> OnPuzzleStarted;
    public event Action<PuzzleController, PuzzleResult> OnPuzzleEnded;

    private PuzzleController activePuzzle;
    private PuzzleState state = PuzzleState.Idle;

    public PuzzleController ActivePuzzle => activePuzzle;
    public PuzzleState State => state;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(transform.root);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public bool StartPuzzle(PuzzleController puzzle)
    {
        if (state != PuzzleState.Idle)
            return false;

        activePuzzle = puzzle;
        state = PuzzleState.Playing;

        activePuzzle.StartPuzzle();

        OnPuzzleStarted?.Invoke(activePuzzle);
        return true;
    }

    public void EndPuzzle(PuzzleResult result)
    {
        if (activePuzzle == null)
            return;

        PuzzleController finished = activePuzzle;

        finished.EndPuzzle(result);

        OnPuzzleEnded?.Invoke(finished, result);

        activePuzzle = null;
        state = PuzzleState.Idle;
    }

    public void PausePuzzle()
    {
        if (state != PuzzleState.Playing || activePuzzle == null)
            return;

        state = PuzzleState.Paused;
        activePuzzle.PausePuzzle();
    }

    public void ResumePuzzle()
    {
        if (state != PuzzleState.Paused || activePuzzle == null)
            return;

        state = PuzzleState.Playing;
        activePuzzle.ResumePuzzle();
    }

    public bool IsPuzzleActive(PuzzleController puzzle)
    {
        return activePuzzle == puzzle;
    }

    public bool IsPuzzlePaused(PuzzleController puzzle)
    {
        return state == PuzzleState.Paused && activePuzzle == puzzle;
    }
}