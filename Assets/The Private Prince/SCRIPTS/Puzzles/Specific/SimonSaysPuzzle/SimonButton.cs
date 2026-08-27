using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SimonButton : MonoBehaviour
{
    [Header("Button")]
    public Button button;

    [Header("Visual")]
    public Image buttonImage;

    [Tooltip("Normal color of the button when it is available for input.")]
    public Color normalColor = Color.white;

    [Tooltip("Color used when this button is highlighted by the Simon sequence.")]
    public Color highlightedColor = Color.yellow;

    [Tooltip("Multiplier applied to the normal color while disabled.")]
    public Color disabledTint = new Color(0.6f, 0.6f, 0.6f, 1f);

    [Header("Feedback")]
    [Tooltip("How long the button flashes when the player presses it.")]
    public float pressFlashDuration = 0.15f;

    [Tooltip("How long each color is displayed during success/mistake feedback.")]
    public float feedbackFlashDuration = 0.15f;

    private SimonSaysPuzzle.Direction direction;
    private SimonSaysPuzzle puzzle;

    private Coroutine flashCoroutine;

    public void Setup(
        SimonSaysPuzzle.Direction assignedDirection,
        SimonSaysPuzzle assignedPuzzle)
    {
        direction = assignedDirection;
        puzzle = assignedPuzzle;

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnButtonPressed);
        }

        SetHighlighted(false);
        SetInteractable(false);
    }

    private void OnButtonPressed()
    {
        if (puzzle == null)
            return;

        // Flash immediately when the player presses the button.
        FlashPressed();

        puzzle.PlayerPressed(direction);
    }

    public void SetHighlighted(bool highlighted)
    {
        if (buttonImage == null)
            return;

        if (highlighted)
        {
            // Highlight is always shown at full intensity.
            // Disabled tint does NOT affect it.
            buttonImage.color = highlightedColor;
        }
        else
        {
            if (button != null && !button.interactable)
            {
                buttonImage.color =
                    normalColor * disabledTint;
            }
            else
            {
                buttonImage.color =
                    normalColor;
            }
        }
    }

    public void SetInteractable(bool value)
    {
        if (button != null)
            button.interactable = value;

        if (buttonImage == null)
            return;

        if (value)
        {
            buttonImage.color = normalColor;
        }
        else
        {
            buttonImage.color = normalColor * disabledTint;
        }
    }

    public void FlashPressed()
    {
        StartFlash(
            highlightedColor,
            pressFlashDuration
        );
    }

    public void FlashFeedback(Color color)
    {
        StartFlash(
            color,
            feedbackFlashDuration
        );
    }

    private void StartFlash(Color color, float duration)
    {
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
        }

        flashCoroutine = StartCoroutine(
            FlashCoroutine(color, duration)
        );
    }

    private IEnumerator FlashCoroutine(Color color, float duration)
    {
        if (buttonImage == null)
            yield break;

        Color previousColor = buttonImage.color;

        buttonImage.color = color;

        yield return new WaitForSecondsRealtime(duration);

        buttonImage.color = previousColor;

        flashCoroutine = null;
    }
}