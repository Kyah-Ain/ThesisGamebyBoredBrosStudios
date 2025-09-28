using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TypingPuzzle : PuzzleBase
{
    [Header("Typing Puzzle Settings")]
    [TextArea] public string targetString; // Leave null if using randomly generated string
    public int targetLength = 10; // Length of random string if targetString is null/empty
    public int glitchCount = 0;
    public int typoLimit = 0; // 0 means no limit

    [Header("Typing UI Reference")]
    public TextMeshProUGUI puzzleText;
    public TextMeshProUGUI typoText;

    private string currentInput = "";
    private int typoCounter = 0;

    private HashSet<int> glitchIndices = new HashSet<int>();
    private Dictionary<int, char> glitchDisplay = new Dictionary<int, char>();
    private static readonly string glitchChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

    private void OnEnable()
    {
        currentInput = "";
        typoCounter = 0;

        if (string.IsNullOrEmpty(targetString))
        {
            targetString = GenerateRandomString(targetLength);
        }

        AssignGlitches();
        UpdatePuzzleDisplay();
        UpdateTypoUI(true);
    }

    protected override void Update()
    {
        base.Update();

        if (!active) return;

        foreach (char c in Input.inputString)
            HandleCharacterInput(c);
    }

    private void HandleCharacterInput(char input)
    {
        if (!active) return;

        if (input == '\b')
        {
            if (currentInput.Length > 0)
                currentInput = currentInput.Substring(0, currentInput.Length - 1);
            UpdateGlitches();
            UpdatePuzzleDisplay();
            return;
        }

        int currentIndex = currentInput.Length;
        if (currentIndex >= targetString.Length) return;

        char expectedChar = targetString[currentIndex];
        if (input == expectedChar)
        {
            currentInput += input;
        }
        else
        {
            currentInput += input;
            typoCounter++;
            UpdateTypoUI(true);

            if (typoLimit > 0 && typoCounter >= typoLimit)
            {
                PuzzleManager.Instance.EndPuzzle(PuzzleResult.Failed);
                return;
            }
        }

        UpdateGlitches();
        UpdatePuzzleDisplay();

        if (currentInput == targetString)
        {
            PuzzleManager.Instance.EndPuzzle(PuzzleResult.Solved);
        }
    }

    private void UpdatePuzzleDisplay()
    {
        if (puzzleText == null) return;

        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        for (int i = 0; i < targetString.Length; i++)
        {
            bool isTyped = i < currentInput.Length;

            char displayChar = targetString[i];
            if (glitchDisplay.ContainsKey(i))
                displayChar = glitchDisplay[i];

            string colorTag = "<color=white>";
            if (glitchIndices.Contains(i))
            {
                colorTag = "<color=yellow>";
            }
            else if (isTyped)
            {
                if (currentInput[i] == targetString[i])
                    colorTag = "<color=green>";
                else
                    colorTag = "<color=red>";
            }

            sb.Append(colorTag);
            sb.Append(displayChar);
            sb.Append("</color>");
        }

        puzzleText.text = sb.ToString();
    }

    private void UpdateTypoUI(bool visible)
    {
        if (typoText == null) return;

        if (typoLimit <= 0)
        {
            typoText.gameObject.SetActive(false);
            return;
        }

        typoText.gameObject.SetActive(visible);

        if (visible)
            typoText.text = $"Typos: {typoCounter} / {typoLimit}";
    }

    private void UpdateGlitches()
    {
        int nextIndex = currentInput.Length;

        if (glitchIndices.Contains(nextIndex))
        {
            glitchIndices.Remove(nextIndex);
            glitchDisplay.Remove(nextIndex);
        }

        foreach (int i in glitchIndices)
        {
            glitchDisplay[i] = glitchChars[Random.Range(0, glitchChars.Length)];
        }
    }

    protected override void OnUIVisibilityChanged(bool visible)
    {

        UpdateTypoUI(visible);
    }

    private string GenerateRandomString(int length)
    {
        string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        for (int i = 0; i < length; i++)
        {
            sb.Append(chars[Random.Range(0, chars.Length)]);
        }
        return sb.ToString();
    }

    private void AssignGlitches()
    {
        glitchIndices.Clear();
        glitchDisplay.Clear();

        if (glitchCount <= 0) return;

        List<int> candidates = new List<int>();
        for (int i = 1; i < targetString.Length; i++)
            candidates.Add(i);

        for (int i = 0; i < glitchCount && candidates.Count > 0; i++)
        {
            int randIndex = candidates[Random.Range(0, candidates.Count)];
            candidates.Remove(randIndex);
            glitchIndices.Add(randIndex);
            glitchDisplay[randIndex] = glitchChars[Random.Range(0, glitchChars.Length)];
        }
    }

    public override void HandleInput()
    {
        // Handled in Update() via Input.inputString
    }
}
