using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    [Header("Music Settings")]
    public AudioClip mainMenuMusic;
    public AudioClip puzzleMusic;
    public AudioClip gameMusic; // Optional: regular game music
    public AudioSource audioSource;

    [Header("Scene Names")]
    public string mainMenuScene = "MainMenu";

    private AudioClip previousMusic; // Store what was playing before puzzle
    private bool wasPlayingBeforePuzzle = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;

            if (audioSource == null)
                audioSource = GetComponent<AudioSource>();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"Scene loaded: {scene.name}");

        if (scene.name == mainMenuScene)
        {
            PlayMainMenuMusic();
        }
        else
        {
            // Play regular game music if available, otherwise stop music
            if (gameMusic != null)
            {
                PlayGameMusic();
            }
            else
            {
                StopMusic();
            }
        }
    }

    private void PlayMainMenuMusic()
    {
        if (mainMenuMusic != null && audioSource != null)
        {
            audioSource.clip = mainMenuMusic;
            audioSource.loop = true;
            audioSource.Play();
            Debug.Log("Main menu music started");
        }
    }

    private void PlayGameMusic()
    {
        if (gameMusic != null && audioSource != null)
        {
            audioSource.clip = gameMusic;
            audioSource.loop = true;
            audioSource.Play();
            Debug.Log("Game music started");
        }
    }

    public void PlayPuzzleMusic()
    {
        if (puzzleMusic != null && audioSource != null)
        {
            // Store current state before switching to puzzle music
            previousMusic = audioSource.clip;
            wasPlayingBeforePuzzle = audioSource.isPlaying;

            audioSource.clip = puzzleMusic;
            audioSource.loop = true;
            audioSource.Play();
            Debug.Log("Puzzle music started");
        }
    }

    public void ResumePreviousMusic()
    {
        if (previousMusic != null && audioSource != null && wasPlayingBeforePuzzle)
        {
            audioSource.clip = previousMusic;
            audioSource.loop = true;
            audioSource.Play();
            Debug.Log("Resumed previous music");
        }
        else
        {
            // If no previous music, play game music or stop
            if (gameMusic != null)
            {
                PlayGameMusic();
            }
            else
            {
                StopMusic();
            }
        }

        // Reset puzzle state
        previousMusic = null;
        wasPlayingBeforePuzzle = false;
    }

    private void StopMusic()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
            Debug.Log("Music stopped");
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}

