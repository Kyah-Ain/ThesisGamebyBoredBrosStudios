using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Put this on your Settings panel and drag in three UI Sliders
/// (Min 0, Max 1). Works in-game with no restart.
/// </summary>
public class VolumeSettingsUI : MonoBehaviour
{
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    void OnEnable()
    {
        var am = AudioManager.Instance;
        if (am == null) return;

        // Set value first (without notifying), then subscribe.
        if (masterSlider) { masterSlider.SetValueWithoutNotify(am.MasterVolume); masterSlider.onValueChanged.AddListener(am.SetMasterVolume); }
        if (musicSlider) { musicSlider.SetValueWithoutNotify(am.MusicVolume); musicSlider.onValueChanged.AddListener(am.SetMusicVolume); }
        if (sfxSlider) { sfxSlider.SetValueWithoutNotify(am.SfxVolume); sfxSlider.onValueChanged.AddListener(am.SetSfxVolume); }
    }

    void OnDisable()
    {
        var am = AudioManager.Instance;
        if (am == null) return;

        if (masterSlider) masterSlider.onValueChanged.RemoveListener(am.SetMasterVolume);
        if (musicSlider) musicSlider.onValueChanged.RemoveListener(am.SetMusicVolume);
        if (sfxSlider) sfxSlider.onValueChanged.RemoveListener(am.SetSfxVolume);
    }
}