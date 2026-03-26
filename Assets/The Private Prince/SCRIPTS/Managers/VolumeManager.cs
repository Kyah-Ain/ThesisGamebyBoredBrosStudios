using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VolumeManager : MonoBehaviour
{
    public Slider musicSlider;
    public Slider sfxSlider;

    void Start()
    {
        if (PlayerPrefs.HasKey("MusicVolume"))
        {
            LoadMusicVolume();
        }
        else
        {
            PlayerPrefs.SetFloat("MusicVolume", 1);
            LoadMusicVolume();
        }

        if (PlayerPrefs.HasKey("SFXVolume"))
        {
            LoadSFXVolume();
        }
        else
        {
            PlayerPrefs.SetFloat("SFXVolume", 1);
            LoadSFXVolume();
        }
    }

    public void SetMusicVolume()
    {
        MusicManager.Instance.audioSource.volume = musicSlider.value;
        SaveMusicVolume();
    }

    public void SetSFXVolume()
    {
        // Add instance of SFX Manager here
        SaveSFXVolume();
    }

    public void SaveMusicVolume()
    {
        PlayerPrefs.SetFloat("MusicVolume", musicSlider.value);
    }

    public void SaveSFXVolume()
    {
        PlayerPrefs.SetFloat("SFXVolume", sfxSlider.value);
    }

    public void LoadMusicVolume()
    {
        musicSlider.value = PlayerPrefs.GetFloat("MusicVolume");
    }

    public void LoadSFXVolume()
    {
        sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume");
    }
}
