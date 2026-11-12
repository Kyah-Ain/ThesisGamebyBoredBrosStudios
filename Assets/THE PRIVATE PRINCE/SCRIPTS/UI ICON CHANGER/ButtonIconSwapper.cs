using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.UI;

public class ButtonIconSwapper : MonoBehaviour
{
    [SerializeField] private Image target;
    [SerializeField] private Sprite keyboardMouse;
    [SerializeField] private Sprite xbox;
    [SerializeField] private Sprite playStation;

    private void Reset()
    {
        target = GetComponent<Image>();
    }

    private void OnEnable()
    {
        LastInputDetector.Changed += OnChanged;
        OnChanged(LastInputDetector.Current);
    }

    private void OnDisable()
    {
        LastInputDetector.Changed -= OnChanged;
    }

    private void OnChanged(InputFamily family)
    {
        Sprite sprite = Pick(family);

        if (sprite != null)
        {
            target.sprite = sprite;
        }
    }

    private Sprite Pick(InputFamily family)
    {
        if (family == InputFamily.KeyboardMouse)
        {
            return keyboardMouse;
        }

        if (playStation != null && Gamepad.current is DualShockGamepad)
        {
            return playStation;
        }

        return xbox != null ? xbox : keyboardMouse;
    }
}