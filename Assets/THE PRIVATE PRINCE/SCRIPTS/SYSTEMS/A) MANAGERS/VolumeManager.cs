using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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

    // ----------------------- UNITY METHODS -----------------------

    // Built-In Unity method that being called 1st (when Enabled)
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.transform.root.gameObject);
            Debug.Log("[VolumeManager] Instance created and marked DontDestroyOnLoad.");
        }
        else
        {
            Debug.LogWarning("[VolumeManager] Duplicate instance found, destroying this one.");
            Destroy(this.gameObject);
            return;
        }
    }

    // Built-In Unity method that being called last (upon Quitting the Game)
    private void OnApplicationQuit()
    {
        Debug.Log("[VolumeManager] OnApplicationQuit() � saving all volumes.");
        SaveMusicVolume();
        SaveSFXVolume();
    }

    // ------------------------- APPLY/SET METHODS -------------------------

    // For Start method to apply loaded volume values to Music
    private void ApplyVolumes()
    {
        float music = PlayerPrefs.GetFloat(MUSIC_VOL_KEY, 1f);
        float sfx = PlayerPrefs.GetFloat(SFX_VOL_KEY, 1f);

        if (musicSlider != null) musicSlider.value = music;
        if (sfxSlider != null) sfxSlider.value = sfx;

        if (MusicManager.Instance != null && MusicManager.Instance.audioSource != null)
            MusicManager.Instance.audioSource.volume = music;
        SfxManager.Instance?.SetVolume(sfx);
    }

    // Method that applies the Slider Values to set the loudness of the BGM
    public void SetMusicVolume(float value)
    {
        if (MusicManager.Instance != null)
            MusicManager.Instance.audioSource.volume = value;
        PlayerPrefs.SetFloat(MUSIC_VOL_KEY, value);
        PlayerPrefs.Save();
    }

    // Method that applies the Slider Values to set the loudness of the SFX
    public void SetSFXVolume(float value)
    {
        SfxManager.Instance?.SetVolume(value);
        PlayerPrefs.SetFloat(SFX_VOL_KEY, value);
        PlayerPrefs.Save();
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