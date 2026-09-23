using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VolumeSliderAssignment : MonoBehaviour
{
    public enum SliderType { Music, SFX }
    public SliderType type;

    private void OnEnable()
    {
        Slider slider = GetComponent<Slider>();

        slider.onValueChanged.RemoveAllListeners();

        if (type == SliderType.Music)
        {
            slider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);
            slider.onValueChanged.AddListener(VolumeManager.Instance.SetMusicVolume);
        }
        else
        {
            slider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);
            slider.onValueChanged.AddListener(VolumeManager.Instance.SetSFXVolume);
        }
    }
}
