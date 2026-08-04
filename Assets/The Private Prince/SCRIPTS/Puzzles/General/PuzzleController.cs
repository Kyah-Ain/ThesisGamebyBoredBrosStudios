using UnityEngine;

public enum PuzzleResult
{
    Solved,
    Failed
}

public class PuzzleController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PuzzleBase puzzle;
    [SerializeField] private PuzzleUI puzzleUI;
    [SerializeField] private PuzzleGate puzzleGate;

    [Header("Difficulty")]
    [SerializeField]
    private PuzzleDifficultyProfile difficultyProfile;

    public PuzzleDifficultyProfile DifficultyProfile => difficultyProfile;

    [Header("Timer")]
    [SerializeField] private bool useTimer = false;
    [SerializeField] private float timeLimit = 60f;

    private float timer;
    private bool active;

    public bool IsActive => active;
    public PuzzleDifficultyLevel Difficulty => difficulty;

    private void Awake()
    {
        if (puzzle == null)
            puzzle = GetComponent<PuzzleBase>();

        if (puzzleUI == null)
            puzzleUI = GetComponentInChildren<PuzzleUI>(true);

        if (puzzleGate == null)
            puzzleGate = GetComponentInChildren<PuzzleGate>(true);

        puzzle.Initialize(this);
    }

    private void Update()
    {
        if (!active)
            return;

        if (difficultyProfile.timeLimit > 0f)
        {
            timer -= Time.unscaledDeltaTime;

            puzzleUI?.UpdateTimer(timer);

            if (timer <= 0)
            {
                FailPuzzle();
                return;
            }
        }

        puzzle.HandleInput();
    }

    public void StartPuzzle()
    {
        active = true;

        timer = difficultyProfile.timeLimit;

        puzzleUI?.Show();

        puzzle.BeginPuzzle();
    }

    public void EndPuzzle(PuzzleResult result)
    {
        active = false;

        puzzleUI?.Hide();

        if (result == PuzzleResult.Solved)
        {
            puzzleGate?.Open();
        }
    }

    public void PausePuzzle()
    {
        active = false;

        puzzle.PausePuzzle();

        puzzleUI?.Hide();
    }

    public void ResumePuzzle()
    {
        active = true;

        puzzle.ResumePuzzle();

        puzzleUI?.Show();
    }

    public void ResetPuzzle()
    {
        active = false;

        timer = timeLimit;

        puzzle.ResetPuzzle();

        puzzleUI?.Hide();
    }

    public void CompletePuzzle()
    {
        PuzzleManager.Instance.EndPuzzle(PuzzleResult.Solved);
    }

    public void FailPuzzle()
    {
        PuzzleManager.Instance.EndPuzzle(PuzzleResult.Failed);
    }
}