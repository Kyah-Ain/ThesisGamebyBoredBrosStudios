using System.Collections; // Grants access to collections and data structures like ArrayList, Hashtable, etc.
using System.Collections.Generic; // Grants access to generic collections like List, Dictionary, etc.
using UnityEngine; // Grants access to Unity's core classes and functions, such as MonoBehaviour, GameObject, Transform, etc.

public class PauseManager : ActivationManager
{
    // --------------------------- VARIABLES --------------------------
    [Header("Audio Clips to UnPause/Pause")]
    public AudioSource[] audioSource; // Reference to the sound effect that will be paused

    // ---------------------------- METHODS ---------------------------
    // Built-in Unity method that is first to be called than any methods
    public void Awake()
    {
        Time.timeScale = 1f; // Ensures the game starts in normal speed
    }

    // Method that pauses the game when the settings menu is opened
    public void OpenSettings() 
    {
        Time.timeScale = 0f; // Pauses the game
        PauseAudio(); // Pauses the audio sources
        base.Switch(); // Switches visual
    }

    // Method that resumes the game when the settings menu is closed
    public void CloseSettings()
    {
        Time.timeScale = 1f; // Continues the game 
        ResumeAudio(); // Resumes the audio sources
        base.Switch(); // Switches visual
    }

    // Method that pauses the audio when the settings menu is closed
    public void PauseAudio() 
    {
        foreach (AudioSource audio in audioSource) 
        {
            audio.Pause(); // Pauses the audio source
        }
    }

    // Method that resumes the audio when the settings menu is closed
    public void ResumeAudio()
    {
        foreach (AudioSource audio in audioSource)
        {
            audio.UnPause(); // Resumes the audio source
        }
    }
}
