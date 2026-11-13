using UnityEngine;

/// <summary>
/// Hook this to your puzzle's event system (UnityEvents).
/// - Puzzle starts  -> call StartPuzzleMusic()
/// - Puzzle solved / closed -> call EndPuzzleMusic()
/// Each puzzle gets its own song via the Puzzle Clip field.
/// </summary>
public class PuzzleMusicTrigger : MonoBehaviour
{
    [SerializeField] private AudioClip puzzleClip;
    [SerializeField, Range(0f, 1f)] private float volume = 1f;
    [SerializeField] private float fadeTime = 0.75f;

    public void StartPuzzleMusic()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.StartPuzzleMusic(puzzleClip, volume, fadeTime);
    }

    public void EndPuzzleMusic()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.EndPuzzleMusic(fadeTime);
    }

    // Safety: if the puzzle object is destroyed/disabled mid-puzzle
    // (e.g. a scene change), don't leave the puzzle song stuck on.
    void OnDestroy()
    {
        if (AudioManager.Instance != null && AudioManager.Instance.IsPuzzleMusicActive && gameObject.scene.isLoaded == false)
            AudioManager.Instance.EndPuzzleMusic(fadeTime);
    }
}