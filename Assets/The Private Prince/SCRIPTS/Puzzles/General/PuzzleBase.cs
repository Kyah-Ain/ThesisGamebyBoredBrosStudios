using UnityEngine;

public abstract class PuzzleBase : MonoBehaviour
{
    protected PuzzleController controller;

    public virtual void Initialize(PuzzleController puzzleController)
    {
        controller = puzzleController;
    }

    /// <summary>
    /// Called whenever the puzzle starts.
    /// </summary>
    public virtual void BeginPuzzle() { }

    /// <summary>
    /// Called every frame while the puzzle is active.
    /// </summary>
    public abstract void HandleInput();

    /// <summary>
    /// Called whenever the puzzle is reset.
    /// </summary>
    public virtual void ResetPuzzle() { }

    /// <summary>
    /// Called when the puzzle is paused.
    /// </summary>
    public virtual void PausePuzzle() { }

    /// <summary>
    /// Called when the puzzle resumes.
    /// </summary>
    public virtual void ResumePuzzle() { }

    /// <summary>
    /// Call when the puzzle is successfully completed.
    /// </summary>
    protected void CompletePuzzle()
    {
        controller.CompletePuzzle();
    }

    /// <summary>
    /// Call when the puzzle fails.
    /// </summary>
    protected void FailPuzzle()
    {
        controller.FailPuzzle();
    }
}