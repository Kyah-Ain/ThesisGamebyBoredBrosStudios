using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    [Header("Music Settings")]
    public AudioClip mainMenuMusic;
    public AudioSource audioSource;

    [Header("Scene Names")]
    public string mainMenuScene = "MainMenu";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;

            // Set up audio source if not assigned
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
            StopMusic();
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
