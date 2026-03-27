using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SfxManager : MonoBehaviour
{
    public static SfxManager Instance;

    [Header("Audio Source Prefab (3D SFX)")]
    public AudioSource sfxPrefab;

    [Header("2D Audio Source (UI SFX)")]
    public AudioSource uiAudioSource;

    [Header ("Pooling Settings")]
    public int poolSize = 10;

    [Header("UI SFX Settings")]
    public AudioClip audioStart;
    public AudioClip audioSelect;

    private List<AudioSource> pool = new List<AudioSource>();
    private float sfxVolume = 1f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.transform.root);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }

        for (int i = 0; i < poolSize; i++)
        {
            AudioSource source = Instantiate(sfxPrefab, transform);
            source.gameObject.SetActive(false);
            pool.Add(source);
        }
    }

    // Ensure all looping SFX stops playing when switching scenes
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StopAllLoops();
    }

    // Stop all looping SFX in the pool
    private void StopAllLoops()
    {
        if (pool.Count <= 0) return;

        foreach (var src in pool)
        {
            if (src.isPlaying && src.loop)
            {
                src.Stop();
                src.loop = false;
                src.gameObject.SetActive(false);
            }
        }
    }

    // Called by VolumeManager when SFX volume changes
    public void SetVolume(float volume)
    {
        sfxVolume = volume;
        uiAudioSource.volume = sfxVolume;

        foreach (var src in pool)
        {
            if (src.isPlaying)
                src.volume = sfxVolume;
        }
    }

    // Get an available AudioSource
    private AudioSource GetSource()
    {
        foreach (var src in pool)
        {
            if (!src.isPlaying)
                return src;
        }

        //Expand pool if needed
        AudioSource newSrc = Instantiate(sfxPrefab, transform);
        pool.Add(newSrc);
        return newSrc;
    }

    // Play 3D SFX
    public AudioSource PlaySFX(AudioClip clip, Vector3 position, float volume = 1f, bool loop = false)
    {
        AudioSource src = GetSource();

        src.transform.position = position;
        src.clip = clip;
        src.volume = volume * sfxVolume;
        src.spatialBlend = 1f;
        src.loop = loop;

        src.gameObject.SetActive(true);
        src.Play();

        return src;
    }

    // Stop a looping SFX
    public void StopSFX(AudioSource src)
    {
        if (src != null)
        {
            src.Stop();
            src.loop = false;
            src.gameObject.SetActive(false);
        }
    }

    // Play a UI SFX (2D sound)
    public void PlayUISFX(AudioClip clip, float volume = 1f)
    {
        uiAudioSource.Stop(); // Stop any currently playing UI SFX to prevent overlap
        uiAudioSource.PlayOneShot(clip, volume * sfxVolume);
    }
}
