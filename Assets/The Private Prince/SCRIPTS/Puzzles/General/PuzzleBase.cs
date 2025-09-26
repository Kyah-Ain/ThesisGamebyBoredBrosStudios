using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PuzzleBase : MonoBehaviour
{
    [Header("Puzzle Settings")]
    public bool Pausable = true;
    public float timeLimit = 0f; // 0 means no time limit

    protected float timer = 0f;
    protected bool active = false;

    public virtual void StartPuzzle()
    {
        timer = timeLimit;
        active = true;
    }

    public virtual void EndPuzzle(PuzzleResult result)
    {
        active = false;
        Debug.Log($"Puzzle ended with result: {result}");
    }

    public virtual void PausePuzzle()
    {
        if (Pausable) active = false;
    }

    public virtual void ResumePuzzle()
    {
        if (Pausable) active = true;
    }

    protected virtual void Update()
    {
        if (!active) return;

        if (timeLimit > 0f)
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                PuzzleManager.Instance.EndPuzzle(PuzzleResult.Failed);
            }
        }
    }

    // To be overridden for specific puzzle logic
    public abstract void HandleInput();
}
