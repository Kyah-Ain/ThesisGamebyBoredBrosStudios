using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectPuzzle : PuzzleBase
{
    [Header("Typing Puzzle Settings")]
    [TextArea] public string targetString;
    public int glitchCount = 0;
    public int typoLimit = 0;

    private string currentInput = "";
    private int typoCounter = 0;

    private void OnEnable()
    {
        currentInput = "";
        typoCounter = 0;
    }

    private void OnGUI()
    {
        if (!active) return;

        Event e = Event.current;
        if (e.type == EventType.KeyDown && e.character != '\0')
        {
            HandleCharacterInput(e.character);
        }
    }

    private void HandleCharacterInput(char input)
    {
        if (!active) return;

        int currentIndex = currentInput.Length;
        if (currentIndex >= targetString.Length) return;

        char expectedChar = targetString[currentIndex];
        if (input == expectedChar)
        {
            currentInput += input;
        }
        else if (input == '\b')
        {
            if (currentInput.Length > 0)
                currentInput = currentInput.Substring(0, currentInput.Length - 1);
        }
        else
        {
            typoCounter++;
            if (typoCounter >= typoLimit)
            {
                PuzzleManager.Instance.EndPuzzle(PuzzleResult.Failed);
                return;
            }
        }

        if (currentInput == targetString)
        {
            PuzzleManager.Instance.EndPuzzle(PuzzleResult.Solved);
        }
    }

    public override void HandleInput()
    {
        // Input is handled in OnGUI for real-time typing
    }
}
