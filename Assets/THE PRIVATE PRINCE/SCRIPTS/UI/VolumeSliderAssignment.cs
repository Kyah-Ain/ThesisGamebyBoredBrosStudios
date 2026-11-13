using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Connects a slider of ANY range (e.g. 0-18 for a segmented bar) to AudioManager.
/// The slider value is converted to 0-1 automatically using the slider's Min/Max.
/// </summary>
[RequireComponent(typeof(Slider))]
public class VolumeSliderAssignment : MonoBehaviour
{
    public enum VolumeType { Music, SFX, Master }

    public VolumeType Type = VolumeType.Music;

    Slider slider;

    void Awake()
    {
        slider = GetComponent<Slider>();
    }

    void OnEnable()
    {
        var am = AudioManager.Instance;
        if (am == null) return;

        // Show the saved volume on the slider (without triggering a change)
        slider.SetValueWithoutNotify(Denormalize(GetCurrent(am)));
        slider.onValueChanged.AddListener(OnSliderChanged);
    }

    void OnDisable()
    {
        slider.onValueChanged.RemoveListener(OnSliderChanged);
    }

    void OnSliderChanged(float value)
    {
        var am = AudioManager.Instance;
        if (am == null) return;

        float normalized = Normalize(value); // 0-18 -> 0-1

        switch (Type)
        {
            case VolumeType.Music: am.SetMusicVolume(normalized); break;
            case VolumeType.SFX: am.SetSfxVolume(normalized); break;
            case VolumeType.Master: am.SetMasterVolume(normalized); break;
        }
    }

    float GetCurrent(AudioManager am)
    {
        switch (Type)
        {
            case VolumeType.SFX: return am.SfxVolume;
            case VolumeType.Master: return am.MasterVolume;
            default: return am.MusicVolume;
        }
    }

    float Normalize(float v) => Mathf.InverseLerp(slider.minValue, slider.maxValue, v);
    float Denormalize(float n) => Mathf.Lerp(slider.minValue, slider.maxValue, n);
}