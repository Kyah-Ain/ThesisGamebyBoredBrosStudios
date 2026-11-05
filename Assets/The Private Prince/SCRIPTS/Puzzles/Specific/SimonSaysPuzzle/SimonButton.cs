using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SimonButton : MonoBehaviour
{
    [Header("Visual")]
    [Tooltip("The parent image (the unlit button).")]
    public Image buttonImage;

    [Tooltip("The child image (the lit button). Hidden when not lit.")]
    public Image litImage;

    [Tooltip("Color of the unlit image when the player has control.")]
    public Color normalColor = Color.white;

    [Tooltip("Tint applied to the unlit image while the player has no control.")]
    public Color disabledTint = new Color(0.75f, 0.75f, 0.75f, 1f);

    [Header("Feedback")]
    [Tooltip("How long the lit image shows when the player presses the button.")]
    public float pressFlashDuration = 0.15f;

    private SimonSaysPuzzle.Direction direction;
    private SimonSaysPuzzle puzzle;

    private Coroutine flashCoroutine;
    private Color litBaseColor = Color.white;

    public SimonSaysPuzzle.Direction Direction => direction;

    private void Awake()
    {
        if (litImage != null)
        {
            litBaseColor = litImage.color;
        }
    }

    public void Setup(
        SimonSaysPuzzle.Direction assignedDirection,
        SimonSaysPuzzle assignedPuzzle)
    {
        direction = assignedDirection;
        puzzle = assignedPuzzle;

        ResetVisual();
        SetInteractable(false);
    }

    // ---------------------------------------------------------
    // Lit / unlit
    // ---------------------------------------------------------

    /// <summary>
    /// Shows or hides the lit child image.
    /// </summary>
    public void SetHighlighted(bool highlighted)
    {
        if (litImage == null)
            return;

        litImage.color = litBaseColor;
        litImage.gameObject.SetActive(highlighted);
    }

    /// <summary>
    /// Stops any flash and returns the button to its unlit state.
    /// </summary>
    public void ResetVisual()
    {
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
            flashCoroutine = null;
        }

        SetHighlighted(false);
    }

    /// <summary>
    /// When false, the parent image is tinted gray to show the
    /// player has no control. The lit child is not affected.
    /// </summary>
    public void SetInteractable(bool value)
    {
        if (buttonImage == null)
            return;

        buttonImage.color = value
            ? normalColor
            : normalColor * disabledTint;
    }

    // ---------------------------------------------------------
    // Flashes
    // ---------------------------------------------------------

    public void FlashPressed()
    {
        StartFlash(litBaseColor, 1, pressFlashDuration, 0f);
    }

    /// <summary>
    /// Flashes the lit image 'count' times using the given tint.
    /// White keeps the lit image's own drawn colors.
    /// </summary>
    public void FlashFeedback(
        Color color,
        int count,
        float onDuration,
        float offDuration)
    {
        StartFlash(color, count, onDuration, offDuration);
    }

    private void StartFlash(
        Color color,
        int count,
        float onDuration,
        float offDuration)
    {
        if (litImage == null)
            return;

        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
        }

        flashCoroutine = StartCoroutine(
            FlashCoroutine(color, count, onDuration, offDuration)
        );
    }

    private IEnumerator FlashCoroutine(
        Color color,
        int count,
        float onDuration,
        float offDuration)
    {
        for (int i = 0; i < count; i++)
        {
            litImage.color = color;
            litImage.gameObject.SetActive(true);

            yield return new WaitForSecondsRealtime(onDuration);

            litImage.gameObject.SetActive(false);

            if (offDuration > 0f)
                yield return new WaitForSecondsRealtime(offDuration);
        }

        litImage.color = litBaseColor;
        flashCoroutine = null;
    }
}