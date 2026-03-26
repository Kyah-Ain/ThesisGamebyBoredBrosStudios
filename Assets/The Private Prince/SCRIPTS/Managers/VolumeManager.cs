using UnityEngine;
using UnityEngine.UI;

public class VolumeManager : MonoBehaviour
{
    // ------------------------- VARIABLES -------------------------

    private static VolumeManager instance;
    public static VolumeManager Instance => instance;

    private const string MUSIC_VOL_KEY = "MusicVolume";
    private const string SFX_VOL_KEY = "SFXVolume";

    [Header("VISUAL REFERENCES")]
    public Slider musicSlider;
    public Slider sfxSlider;

    // Blocks Set/Save calls from firing during initialization
    private bool isInitializing = true;

    // ----------------------- UNITY METHODS -----------------------

    // Built-In Unity method that being called 1st (when Enabled)
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.transform.root);
            Debug.Log("[VolumeManager] Instance created and marked DontDestroyOnLoad.");
        }
        else
        {
            Debug.LogWarning("[VolumeManager] Duplicate instance found, destroying this one.");
            Destroy(this.gameObject);
            return;
        }
    }

    // Built-In Unity method that being called 3rd (when Enabled)
    void Start()
    {
        Debug.Log($"[VolumeManager] RAW KEY CHECK — Music: {PlayerPrefs.GetFloat(MUSIC_VOL_KEY, -1f)}, SFX: {PlayerPrefs.GetFloat(SFX_VOL_KEY, -1f)}");

        isInitializing = true;

        Debug.Log("[VolumeManager] Start() — init flag ON, loading saved volumes.");

        LoadMusicVolume();
        LoadSFXVolume();
        isInitializing = false;

        Debug.Log("[VolumeManager] Init flag OFF — slider events will now save normally.");

        SetMusicVolume();
        SetSFXVolume();
    }

    // Built-In Unity method that being called last (upon Quitting the Game)
    private void OnApplicationQuit()
    {
        Debug.Log("[VolumeManager] OnApplicationQuit() — saving all volumes.");
        SaveMusicVolume();
        SaveSFXVolume();
    }

    // ------------------------- SET METHODS -------------------------

    // Method that applies the Slider Values to set the loudness of the BGM
    public void SetMusicVolume()
    {
        // Slider fired during init, ignore it
        if (isInitializing)
        {
            Debug.Log("[VolumeManager] SetMusicVolume() blocked during initialization.");
            return;
        }

        MusicManager.Instance.audioSource.volume = musicSlider.value;
        Debug.Log($"[VolumeManager] Music volume set to: {musicSlider.value}");
        SaveMusicVolume();
    }

    // Method that applies the Slider Values to set the loudness of the SFX
    public void SetSFXVolume()
    {
        // Slider fired during init, ignore it
        if (isInitializing)
        {
            Debug.Log("[VolumeManager] SetSFXVolume() blocked during initialization.");
            return;
        }

        // Add SFX Manager instance here
        Debug.Log($"[VolumeManager] SFX volume set to: {sfxSlider.value}");
        SaveSFXVolume();
    }

    // ---------------------- SAVE/LOAD METHODS -----------------------

    // Method to save the BGM's loudness
    public void SaveMusicVolume()
    {
        PlayerPrefs.SetFloat(MUSIC_VOL_KEY, musicSlider.value);
        PlayerPrefs.Save();
        Debug.Log($"[VolumeManager] Music volume saved: {musicSlider.value}");
    }

    // Method to save the SFX's loudness
    public void SaveSFXVolume()
    {
        PlayerPrefs.SetFloat(SFX_VOL_KEY, sfxSlider.value);
        PlayerPrefs.Save();
        Debug.Log($"[VolumeManager] SFX volume saved: {sfxSlider.value}");
    }

    // Method to load the BGM's loudness
    public void LoadMusicVolume()
    {
        float saved = PlayerPrefs.GetFloat(MUSIC_VOL_KEY, 1f);
        musicSlider.value = saved;
        Debug.Log($"[VolumeManager] Music volume loaded: {saved}");
    }

    // Method to save the BGM's loudness
    public void LoadSFXVolume()
    {
        float saved = PlayerPrefs.GetFloat(SFX_VOL_KEY, 1f);
        sfxSlider.value = saved;
        Debug.Log($"[VolumeManager] SFX volume loaded: {saved}");
    }

}