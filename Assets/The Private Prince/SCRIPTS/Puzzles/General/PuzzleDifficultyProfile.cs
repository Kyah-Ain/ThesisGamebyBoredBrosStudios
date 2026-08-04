using UnityEngine;

public abstract class PuzzleDifficultyProfile : ScriptableObject
{
    [Header("General")]
    public PuzzleDifficultyLevel difficulty;
}