using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Persistent audio system: volume control (Master/Music/SFX), saved settings,
/// automatic music per scene with crossfade, and a simple SFX API.
/// Put this on an empty GameObject in your FIRST scene (e.g. Main Menu).
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Serializable]
    public class SceneMusic
    {
        [Tooltip("Exact scene name as in Build Settings")]
        public string sceneName;
        public AudioClip music;
        [Range(0f, 1f)] public float volume = 1f;
    }

    [Header("Music per scene")]
    [SerializeField] private List<SceneMusic> sceneMusic = new List<SceneMusic>();
    [Tooltip("Played if a scene is not in the list above. Leave empty for silence.")]
    [SerializeField] private AudioClip defaultMusic;
    [SerializeField, Range(0f, 1f)] private float defaultMusicVolume = 1f;
    [SerializeField] private float crossfadeDuration = 1.5f;

    [Header("Default volumes (used on first launch)")]
    [SerializeField, Range(0f, 1f)] private float defaultMaster = 1f;
    [SerializeField, Range(0f, 1f)] private float defaultMusicVol = 0.7f;
    [SerializeField, Range(0f, 1f)] private float defaultSfxVol = 1f;

    public float MasterVolume { get; private set; }
    public float MusicVolume { get; private set; }
    public float SfxVolume { get; private set; }

    const string MasterKey = "Audio_Master";
    const string MusicKey = "Audio_Music";
    const string SfxKey = "Audio_SFX";

    AudioSource sourceA, sourceB, sfxSource, puzzleSource;

    // Puzzle music state
    bool puzzleActive;
    bool puzzleEnding;
    float puzzleClipVolume = 1f;
    Coroutine puzzleRoutine;
    bool hasPending;                  // a scene/music change requested during a puzzle
    AudioClip pendingClip;
    float pendingVolume = 1f;

    AudioSource activeMusic;          // the one currently audible
    float activeClipVolume = 1f;      // per-clip volume of the active track
    Coroutine fadeRoutine;
    Dictionary<string, SceneMusic> lookup;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        transform.SetParent(null); // DontDestroyOnLoad only works on root objects
        DontDestroyOnLoad(gameObject);

        sourceA = CreateSource("Music A", true);
        sourceB = CreateSource("Music B", true);
        sfxSource = CreateSource("SFX", false);
        puzzleSource = CreateSource("Puzzle Music", true);
        activeMusic = sourceA;

        lookup = new Dictionary<string, SceneMusic>();
        foreach (var s in sceneMusic)
            if (!string.IsNullOrEmpty(s.sceneName)) lookup[s.sceneName] = s;

        MasterVolume = PlayerPrefs.GetFloat(MasterKey, defaultMaster);
        MusicVolume = PlayerPrefs.GetFloat(MusicKey, defaultMusicVol);
        SfxVolume = PlayerPrefs.GetFloat(SfxKey, defaultSfxVol);
        ApplyVolumes();
    }

    void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    // Awake runs before sceneLoaded for the first scene only if enabled early,
    // so also handle the very first scene explicitly.
    void Start() => PlayMusicForScene(SceneManager.GetActiveScene().name, instant: true);

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (mode == LoadSceneMode.Single)
            PlayMusicForScene(scene.name, instant: false);
    }

    AudioSource CreateSource(string objName, bool loop)
    {
        var go = new GameObject(objName);
        go.transform.SetParent(transform);
        var src = go.AddComponent<AudioSource>();
        src.playOnAwake = false;
        src.loop = loop;
        src.spatialBlend = 0f; // 2D
        return src;
    }

    // ---------------- Music ----------------

    void PlayMusicForScene(string sceneName, bool instant)
    {
        if (lookup.TryGetValue(sceneName, out var data))
            PlayMusic(data.music, data.volume, instant);
        else
            PlayMusic(defaultMusic, defaultMusicVolume, instant);
    }

    /// <summary>Crossfade to a new track. Same clip already playing = no restart.</summary>
    public void PlayMusic(AudioClip clip, float clipVolume = 1f, bool instant = false)
    {
        // During a puzzle, remember the request and apply it when the puzzle ends
        if (puzzleActive || puzzleEnding)
        {
            hasPending = true;
            pendingClip = clip;
            pendingVolume = clipVolume;
            return;
        }

        if (clip != null && activeMusic.clip == clip && activeMusic.isPlaying)
        {
            activeClipVolume = clipVolume;
            activeMusic.volume = TargetMusicVolume();
            return;
        }
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(Crossfade(clip, clipVolume, instant ? 0f : crossfadeDuration));
    }

    public void StopMusic(float fadeTime = 1f)
    {
        if (puzzleActive || puzzleEnding)
        {
            hasPending = true;
            pendingClip = null;
            pendingVolume = 1f;
            return;
        }

        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(Crossfade(null, 1f, fadeTime));
    }

    IEnumerator Crossfade(AudioClip newClip, float newClipVolume, float duration)
    {
        AudioSource from = activeMusic;
        AudioSource to = (from == sourceA) ? sourceB : sourceA;

        float fromStart = from.volume;
        float oldClipVolume = activeClipVolume;
        activeClipVolume = newClipVolume;

        to.clip = newClip;
        to.volume = 0f;
        if (newClip != null) to.Play();
        activeMusic = to;

        if (duration <= 0f)
        {
            from.Stop();
            to.volume = TargetMusicVolume();
        }
        else
        {
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / duration);
                from.volume = Mathf.Lerp(fromStart, 0f, k);
                to.volume = Mathf.Lerp(0f, TargetMusicVolume(), k);
                yield return null;
            }
            from.Stop();
            to.volume = TargetMusicVolume();
        }
        fadeRoutine = null;
    }

    float TargetMusicVolume() => MasterVolume * MusicVolume * activeClipVolume;
    float TargetPuzzleVolume() => MasterVolume * MusicVolume * puzzleClipVolume;

    // ---------------- Puzzle music ----------------

    public bool IsPuzzleMusicActive => puzzleActive;

    /// <summary>
    /// Pauses the current music (remembering its position) and plays the puzzle song.
    /// </summary>
    public void StartPuzzleMusic(AudioClip clip, float clipVolume = 1f, float fadeTime = 0.75f)
    {
        if (clip == null) return;

        // Already in a puzzle: just swap the puzzle track, keep the original music paused
        if (puzzleActive)
        {
            puzzleClipVolume = clipVolume;
            puzzleSource.clip = clip;
            puzzleSource.volume = TargetPuzzleVolume();
            puzzleSource.Play();
            return;
        }

        // Settle any scene crossfade in progress
        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
            fadeRoutine = null;
            AudioSource other = (activeMusic == sourceA) ? sourceB : sourceA;
            other.Stop();
            activeMusic.volume = TargetMusicVolume();
        }
        if (puzzleRoutine != null) { StopCoroutine(puzzleRoutine); puzzleRoutine = null; }

        puzzleActive = true;
        puzzleEnding = false;
        hasPending = false;
        puzzleClipVolume = clipVolume;

        puzzleSource.clip = clip;
        puzzleSource.volume = 0f;
        puzzleSource.Play();

        puzzleRoutine = StartCoroutine(PuzzleStartRoutine(fadeTime));
    }

    /// <summary>
    /// Stops the puzzle song and resumes the previous music where it left off
    /// (or the new scene's music, if the scene changed during the puzzle).
    /// </summary>
    public void EndPuzzleMusic(float fadeTime = 0.75f)
    {
        if (!puzzleActive) return;

        puzzleActive = false;
        puzzleEnding = true;
        if (puzzleRoutine != null) StopCoroutine(puzzleRoutine);
        puzzleRoutine = StartCoroutine(PuzzleEndRoutine(fadeTime));
    }

    IEnumerator PuzzleStartRoutine(float duration)
    {
        float startVol = activeMusic.volume;
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / duration);
            activeMusic.volume = Mathf.Lerp(startVol, 0f, k);
            puzzleSource.volume = Mathf.Lerp(0f, TargetPuzzleVolume(), k);
            yield return null;
        }
        activeMusic.Pause(); // keeps playback position
        puzzleSource.volume = TargetPuzzleVolume();
        puzzleRoutine = null;
    }

    IEnumerator PuzzleEndRoutine(float duration)
    {
        // If the scene changed during the puzzle, don't resume the old track
        bool resumeOld = !hasPending;
        if (resumeOld)
        {
            activeMusic.volume = 0f;
            activeMusic.UnPause();
        }

        float puzzleStart = puzzleSource.volume;
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / duration);
            puzzleSource.volume = Mathf.Lerp(puzzleStart, 0f, k);
            if (resumeOld) activeMusic.volume = Mathf.Lerp(0f, TargetMusicVolume(), k);
            yield return null;
        }

        puzzleSource.Stop();
        puzzleSource.clip = null;
        if (resumeOld) activeMusic.volume = TargetMusicVolume();

        puzzleEnding = false;
        puzzleRoutine = null;

        // Apply any music change requested while the puzzle was running
        if (hasPending)
        {
            hasPending = false;
            PlayMusic(pendingClip, pendingVolume, instant: false);
        }
    }

    // ---------------- SFX ----------------

    public void PlaySFX(AudioClip clip, float volume = 1f, float pitch = 1f)
    {
        if (clip == null) return;
        sfxSource.pitch = pitch;
        sfxSource.PlayOneShot(clip, volume * MasterVolume * SfxVolume);
    }

    /// <summary>3D sound at a world position (doors, explosions, NPCs...).</summary>
    public void PlaySFXAt(AudioClip clip, Vector3 position, float volume = 1f)
    {
        if (clip == null) return;
        AudioSource.PlayClipAtPoint(clip, position, volume * MasterVolume * SfxVolume);
    }

    // ---------------- Volume settings ----------------

    public void SetMasterVolume(float v) { MasterVolume = Mathf.Clamp01(v); PlayerPrefs.SetFloat(MasterKey, MasterVolume); ApplyVolumes(); }
    public void SetMusicVolume(float v) { MusicVolume = Mathf.Clamp01(v); PlayerPrefs.SetFloat(MusicKey, MusicVolume); ApplyVolumes(); }
    public void SetSfxVolume(float v) { SfxVolume = Mathf.Clamp01(v); PlayerPrefs.SetFloat(SfxKey, SfxVolume); ApplyVolumes(); }

    void ApplyVolumes()
    {
        // Don't fight an in-progress crossfade; it reads TargetMusicVolume() each frame.
        if (fadeRoutine == null && !puzzleActive && !puzzleEnding && activeMusic != null)
            activeMusic.volume = TargetMusicVolume();

        if (puzzleActive && puzzleRoutine == null && puzzleSource != null)
            puzzleSource.volume = TargetPuzzleVolume();
    }

    void OnApplicationQuit() => PlayerPrefs.Save();
}