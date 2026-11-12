using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public enum InputFamily
{
    KeyboardMouse,
    Gamepad
}

public class LastInputDetector : MonoBehaviour
{
    public static event Action<InputFamily> Changed;
    public static InputFamily Current { get; private set; } = InputFamily.KeyboardMouse;

    [SerializeField] private UnityEvent onKeyboardMouse = new UnityEvent();
    [SerializeField] private UnityEvent onGamepad = new UnityEvent();
    [SerializeField] private float stickThreshold = 0.5f;
    [SerializeField] private float mouseThreshold = 2f;
    [SerializeField] private bool hideCursorOnGamepad;

    private void Start()
    {
        Apply(Current);
    }

    private void Update()
    {
        InputFamily detected = Current;

        if (Current == InputFamily.KeyboardMouse)
        {
            if (UsedGamepad())
            {
                detected = InputFamily.Gamepad;
            }
        }
        else if (UsedKeyboardOrMouse())
        {
            detected = InputFamily.KeyboardMouse;
        }

        if (detected != Current)
        {
            Apply(detected);
        }
    }

    private void Apply(InputFamily family)
    {
        Current = family;

        if (hideCursorOnGamepad)
        {
            Cursor.visible = family != InputFamily.Gamepad;
        }

        Changed?.Invoke(family);

        if (family == InputFamily.Gamepad)
        {
            onGamepad.Invoke();
        }
        else
        {
            onKeyboardMouse.Invoke();
        }
    }

    private bool UsedGamepad()
    {
        Gamepad pad = Gamepad.current;

        if (pad == null)
        {
            return false;
        }

        if (pad.leftStick.ReadValue().magnitude > stickThreshold || pad.rightStick.ReadValue().magnitude > stickThreshold)
        {
            return true;
        }

        var controls = pad.allControls;

        for (int i = 0; i < controls.Count; i++)
        {
            if (controls[i] is ButtonControl button && button.wasPressedThisFrame)
            {
                return true;
            }
        }

        return false;
    }

    private bool UsedKeyboardOrMouse()
    {
        Keyboard keyboard = Keyboard.current;

        if (keyboard != null && keyboard.anyKey.wasPressedThisFrame)
        {
            return true;
        }

        Mouse mouse = Mouse.current;

        if (mouse == null)
        {
            return false;
        }

        if (mouse.delta.ReadValue().sqrMagnitude > mouseThreshold * mouseThreshold)
        {
            return true;
        }

        if (mouse.leftButton.wasPressedThisFrame || mouse.rightButton.wasPressedThisFrame || mouse.middleButton.wasPressedThisFrame)
        {
            return true;
        }

        return mouse.scroll.ReadValue().sqrMagnitude > 0f;
    }
}