using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public abstract class PuzzleBase : MonoBehaviour
{
    [Header("Puzzle Settings")]
    public bool Pausable = true;
    public float timeLimit = 0f; // 0 means no time limit

    [Header("UI References")]
    public GameObject uiRoot;
    public TextMeshProUGUI timerText;

    protected float timer = 0f;
    protected bool active = false;

    public virtual void StartPuzzle()
    {
        timer = timeLimit;
        active = true;
        ShowUI(true);
    }

    public virtual void EndPuzzle(PuzzleResult result)
    {
        active = false;
        Debug.Log($"Puzzle ended with result: {result}");
        ShowUI(false);
    }

    public virtual void PausePuzzle()
    {
        if (Pausable)
        {
            active = false;
            ShowUI(false);
        }
    }

    public virtual void ResumePuzzle()
    {
        if (Pausable)
        {
            active = true;
            ShowUI(true);
        }
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
                return;
            }
            UpdateTimerUI();
        }

        HandleInput();
    }

    private void ShowUI(bool visible)
    {
        if (uiRoot != null)
            uiRoot.SetActive(visible);

        if (timerText != null)
        {
            if (timeLimit > 0f)
                timerText.gameObject.SetActive(visible);
            else
                timerText.gameObject.SetActive(false);
        }

        OnUIVisibilityChanged(visible);
    }

    protected void UpdateTimerUI()
    {
        if (timerText == null || timeLimit <= 0f) return;

        int seconds = Mathf.CeilToInt(timer);
        timerText.text = $"Time: {seconds}s";
    }

    protected virtual void OnUIVisibilityChanged(bool visible) { } // To be overwritten for specific puzzle UI handling

    public abstract void HandleInput(); // To be overwritten for specific puzzle input handling
}
