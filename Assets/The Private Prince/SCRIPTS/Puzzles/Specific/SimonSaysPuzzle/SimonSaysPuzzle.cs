using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SimonSaysPuzzle : PuzzleBase
{
    public enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }

    private enum SimonState
    {
        None,
        ShowingSequence,
        Review,
        PlayerInput,
        Completed,
        Failed
    }

    [Header("Simon Says Settings")]
    [Min(1)]
    public int numberOfLevels = 3;

    [Tooltip("Number of button presses required for each level. Element 0 = Level 1, Element 1 = Level 2, etc.")]
    public List<int> pressesPerLevel = new List<int> { 3, 4, 5 };

    [Tooltip("If enabled, each level extends the sequence from the previous level. If disabled, every level gets a completely new sequence.")]
    public bool progressiveSequence = false;

    [Header("Mistake Settings")]
    [Tooltip("If true, the player can make unlimited mistakes.")]
    public bool unlimitedMistakes = false;

    [Tooltip("Maximum number of mistakes allowed before the puzzle fails.")]
    [Min(0)]
    public int mistakeLimit = 3;

    private int mistakeCount = 0;

    [Header("Sequence Timing")]
    [Min(0.05f)]
    public float lightDuration = 0.4f;

    [Min(0.05f)]
    public float pauseBetweenLights = 0.2f;

    [Min(0f)]
    public float delayBeforePlayerInput = 0.3f;

    [Header("UI")]
    public SimonButton upButton;
    public SimonButton downButton;
    public SimonButton leftButton;
    public SimonButton rightButton;
    public Button reviewButton;
    public TextMeshProUGUI mistakeText;

    [Header("Feedback Colors")]
    public Color successColor = Color.green;
    public Color mistakeColor = Color.red;

    private List<Direction> sequence = new List<Direction>();
    private int currentInputIndex = 0;
    private int currentLevel = 0;
    private bool hasMadeDirectionalInput = false;

    private SimonState state = SimonState.None;

    private Coroutine sequenceCoroutine;

    protected override void OnUIVisibilityChanged(bool visible)
    {
        if (reviewButton != null)
            reviewButton.gameObject.SetActive(visible);
    }

    public override void HandleInput()
    {
        if (state != SimonState.Review &&
            state != SimonState.PlayerInput)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            PlayerPressed(Direction.Up);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            PlayerPressed(Direction.Down);
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            PlayerPressed(Direction.Left);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            PlayerPressed(Direction.Right);
        }
    }

    public override void StartPuzzle()
    {
        base.StartPuzzle();

        currentLevel = 1;
        currentInputIndex = 0;
        mistakeCount = 0;

        state = SimonState.ShowingSequence;

        SetupButtons();
        UpdateMistakeUI();

        if (reviewButton != null)
        {
            reviewButton.onClick.RemoveListener(ReviewSequence);
            reviewButton.onClick.AddListener(ReviewSequence);
            reviewButton.interactable = false;
        }

        GenerateSequenceForLevel();

        sequenceCoroutine = StartCoroutine(ShowSequenceCoroutine());
    }

    private void SetupButtons()
    {
        if (upButton != null)
        {
            upButton.Setup(Direction.Up, this);
        }

        if (downButton != null)
        {
            downButton.Setup(Direction.Down, this);
        }

        if (leftButton != null)
        {
            leftButton.Setup(Direction.Left, this);
        }

        if (rightButton != null)
        {
            rightButton.Setup(Direction.Right, this);
        }
    }

    private void GenerateSequenceForLevel()
    {
        int requiredLength = GetCurrentLevelSequenceLength();

        if (!progressiveSequence)
        {
            // New independent sequence for this level.
            sequence.Clear();
        }

        // If the existing sequence is already long enough,
        // nothing needs to be added.
        while (sequence.Count < requiredLength)
        {
            Direction randomDirection =
                (Direction)Random.Range(0, 4);

            sequence.Add(randomDirection);
        }
    }

    private int GetCurrentLevelSequenceLength()
    {
        int index = currentLevel - 1;

        if (index < 0 || index >= pressesPerLevel.Count)
        {
            Debug.LogWarning(
                $"Simon Says: No sequence length configured for Level {currentLevel}."
            );

            return 1;
        }

        return Mathf.Max(1, pressesPerLevel[index]);
    }

    private IEnumerator ShowSequenceCoroutine()
    {
        state = SimonState.ShowingSequence;

        SetReviewInteractable(false);
        SetDirectionButtonsInteractable(false);

        yield return new WaitForSecondsRealtime(0.5f);

        for (int i = 0; i < sequence.Count; i++)
        {
            yield return StartCoroutine(
                FlashDirection(sequence[i])
            );

            yield return new WaitForSecondsRealtime(pauseBetweenLights);
        }

        yield return new WaitForSecondsRealtime(delayBeforePlayerInput);

        StartReviewPhase();
    }

    private IEnumerator FlashDirection(Direction direction)
    {
        SimonButton button = GetButton(direction);

        if (button == null)
            yield break;

        button.SetHighlighted(true);

        yield return new WaitForSecondsRealtime(lightDuration);

        button.SetHighlighted(false);
    }

    private void StartReviewPhase()
    {
        state = SimonState.Review;

        currentInputIndex = 0;

        // Review is available because the player
        // has not made any directional input yet.
        SetDirectionButtonsInteractable(true);
        SetReviewInteractable(!hasMadeDirectionalInput);
    }

    public void ReviewSequence()
    {
        if (!active)
            return;

        // Review is only possible before the player
        // has entered any directional input.
        if (state != SimonState.Review)
            return;

        if (hasMadeDirectionalInput)
            return;

        if (sequenceCoroutine != null)
        {
            StopCoroutine(sequenceCoroutine);
        }

        sequenceCoroutine = StartCoroutine(
            ReviewSequenceCoroutine()
        );
    }

    private IEnumerator ReviewSequenceCoroutine()
    {
        state = SimonState.ShowingSequence;

        // Disable both while the sequence is being displayed.
        SetReviewInteractable(false);
        SetDirectionButtonsInteractable(false);

        yield return new WaitForSecondsRealtime(0.2f);

        for (int i = 0; i < sequence.Count; i++)
        {
            yield return StartCoroutine(
                FlashDirection(sequence[i])
            );

            yield return new WaitForSecondsRealtime(
                pauseBetweenLights
            );
        }

        yield return new WaitForSecondsRealtime(
            delayBeforePlayerInput
        );

        // If the player somehow made input while the coroutine
        // was running, don't restore review.
        if (hasMadeDirectionalInput)
        {
            state = SimonState.PlayerInput;
            SetReviewInteractable(false);
        }
        else
        {
            state = SimonState.Review;

            SetReviewInteractable(true);
        }

        currentInputIndex = 0;

        SetDirectionButtonsInteractable(true);
    }

    public void PlayerPressed(Direction direction)
    {
        if (!active)
            return;

        if (state != SimonState.Review &&
            state != SimonState.PlayerInput)
        {
            return;
        }

        FlashDirectionButton(direction);

        // The player has now committed to the sequence.
        // Review is no longer available for this level.
        if (!hasMadeDirectionalInput)
        {
            hasMadeDirectionalInput = true;

            SetReviewInteractable(false);

            state = SimonState.PlayerInput;
        }

        Direction expectedDirection =
            sequence[currentInputIndex];

        if (direction != expectedDirection)
        {
            HandleMistake();
            return;
        }

        currentInputIndex++;

        if (currentInputIndex >= sequence.Count)
        {
            CompleteLevel();
        }
    }

    private void FlashDirectionButton(Direction direction)
    {
        switch (direction)
        {
            case Direction.Up:
                upButton?.FlashPressed();
                break;

            case Direction.Down:
                downButton?.FlashPressed();
                break;

            case Direction.Left:
                leftButton?.FlashPressed();
                break;

            case Direction.Right:
                rightButton?.FlashPressed();
                break;
        }
    }

    private void HandleMistake()
    {
        mistakeCount++;

        UpdateMistakeUI();

        if (!unlimitedMistakes &&
            mistakeCount > mistakeLimit)
        {
            FlashAllButtons(mistakeColor);

            FailPuzzle();
            return;
        }

        currentInputIndex = 0;
        hasMadeDirectionalInput = false;

        if (sequenceCoroutine != null)
        {
            StopCoroutine(sequenceCoroutine);
        }

        StartCoroutine(
            MistakeFeedbackCoroutine()
        );
    }

    private IEnumerator MistakeFeedbackCoroutine()
    {
        FlashAllButtons(mistakeColor);

        yield return new WaitForSecondsRealtime(
            0.3f
        );

        sequenceCoroutine = StartCoroutine(
            ShowSequenceCoroutine()
        );
    }

    private void UpdateMistakeUI()
    {
        if (mistakeText == null)
            return;

        if (unlimitedMistakes)
        {
            mistakeText.text = "";
        }
        else
        {
            mistakeText.text =
                $"Mistakes: {mistakeCount} / {mistakeLimit}";
        }
    }

    private void CompleteLevel()
    {
        SetDirectionButtonsInteractable(false);
        SetReviewInteractable(false);

        if (currentLevel >= numberOfLevels)
        {
            FlashAllButtons(successColor);

            state = SimonState.Completed;

            PuzzleManager.Instance.EndPuzzle(
                PuzzleResult.Solved
            );

            return;
        }

        StartCoroutine(
            CompleteLevelFeedbackCoroutine()
        );
    }

    private IEnumerator CompleteLevelFeedbackCoroutine()
    {
        FlashAllButtons(successColor);

        yield return new WaitForSecondsRealtime(
            0.3f
        );

        currentLevel++;

        currentInputIndex = 0;
        hasMadeDirectionalInput = false;

        GenerateSequenceForLevel();

        state = SimonState.ShowingSequence;

        sequenceCoroutine = StartCoroutine(
            ShowSequenceCoroutine()
        );
    }

    private void FailPuzzle()
    {
        state = SimonState.Failed;

        SetDirectionButtonsInteractable(false);
        SetReviewInteractable(false);

        if (sequenceCoroutine != null)
        {
            StopCoroutine(sequenceCoroutine);
        }

        PuzzleManager.Instance.EndPuzzle(
            PuzzleResult.Failed
        );
    }

    private SimonButton GetButton(Direction direction)
    {
        switch (direction)
        {
            case Direction.Up:
                return upButton;

            case Direction.Down:
                return downButton;

            case Direction.Left:
                return leftButton;

            case Direction.Right:
                return rightButton;
        }

        return null;
    }

    private void SetDirectionButtonsInteractable(bool value)
    {
        if (upButton != null)
            upButton.SetInteractable(value);

        if (downButton != null)
            downButton.SetInteractable(value);

        if (leftButton != null)
            leftButton.SetInteractable(value);

        if (rightButton != null)
            rightButton.SetInteractable(value);
    }

    private void SetReviewInteractable(bool value)
    {
        if (reviewButton != null)
            reviewButton.interactable = value;
    }

    protected override void OnPuzzleReset()
    {
        if (sequenceCoroutine != null)
        {
            StopCoroutine(sequenceCoroutine);
            sequenceCoroutine = null;
        }

        sequence.Clear();

        currentLevel = 0;
        currentInputIndex = 0;
        mistakeCount = 0;

        state = SimonState.None;

        SetDirectionButtonsInteractable(false);
        SetReviewInteractable(false);
    }

    public int CurrentLevel => currentLevel;
    public int CurrentInput => currentInputIndex;
    public int SequenceLength => sequence.Count;

    private void FlashAllButtons(Color color)
    {
        if (upButton != null)
            upButton.FlashFeedback(color);

        if (downButton != null)
            downButton.FlashFeedback(color);

        if (leftButton != null)
            leftButton.FlashFeedback(color);

        if (rightButton != null)
            rightButton.FlashFeedback(color);
    }
}