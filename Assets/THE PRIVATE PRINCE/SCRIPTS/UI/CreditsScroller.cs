using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CreditsScroller : MonoBehaviour, IBeginDragHandler, IEndDragHandler
{
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform logo;
    [SerializeField] private float autoSpeed = 70f;
    [SerializeField] private float manualSpeed = 900f;
    [SerializeField] private float wheelDistance = 250f;
    [SerializeField] private float wheelSmoothness = 6f;
    [SerializeField] private float startDelay = 1f;
    [SerializeField] private float resumeDelay = 1.5f;
    [Range(0f, 1f)]
    [SerializeField] private float logoPositionOnScreen = 0.5f;
    [SerializeField] private bool useGamepadAndKeyboard = true;
    [SerializeField] private UnityEvent onReachedEnd = new UnityEvent();

    private RectTransform content;
    private RectTransform viewport;
    private float endY;
    private float lastY;
    private float pauseTimer;
    private float wheelVelocity;
    private bool ready;
    private bool dragging;
    private bool reportedEnd;

    private void Awake()
    {
        content = scrollRect.content;
        viewport = scrollRect.viewport != null ? scrollRect.viewport : (RectTransform)scrollRect.transform;
        scrollRect.scrollSensitivity = 0f;
    }

    private void OnEnable()
    {
        StartCoroutine(Prepare());
    }

    private void OnDisable()
    {
        ready = false;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        dragging = true;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        dragging = false;
        pauseTimer = resumeDelay;
    }

    private IEnumerator Prepare()
    {
        ready = false;
        dragging = false;
        reportedEnd = false;
        wheelVelocity = 0f;
        pauseTimer = startDelay;

        yield return null;

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(content);
        content.anchoredPosition = new Vector2(content.anchoredPosition.x, 0f);

        RecalculateEnd();
        lastY = 0f;
        ready = true;
    }

    private void RecalculateEnd()
    {
        Vector3 logoCenter = logo.TransformPoint(logo.rect.center);
        float distanceFromTop = content.rect.yMax - content.InverseTransformPoint(logoCenter).y;
        float maxScroll = Mathf.Max(0f, content.rect.height - viewport.rect.height);

        endY = Mathf.Clamp(distanceFromTop - viewport.rect.height * logoPositionOnScreen, 0f, maxScroll);
    }

    private void Update()
    {
        if (!ready)
        {
            return;
        }

        RecalculateEnd();

        float currentY = content.anchoredPosition.y;
        float deltaTime = Time.unscaledDeltaTime;

        if (dragging)
        {
            pauseTimer = resumeDelay;
            wheelVelocity = 0f;
            lastY = currentY;
            return;
        }

        if (Mathf.Abs(currentY - lastY) > 0.01f)
        {
            pauseTimer = resumeDelay;
        }

        float manual = useGamepadAndKeyboard ? ReadManualInput() : 0f;
        float wheel = ReadWheel();

        if (Mathf.Abs(wheel) > 0.001f)
        {
            wheelVelocity += -wheel * wheelDistance * wheelSmoothness;
        }

        if (Mathf.Abs(manual) > 0.01f || Mathf.Abs(wheelVelocity) > 1f)
        {
            pauseTimer = resumeDelay;
        }

        float delta = (manual * manualSpeed + wheelVelocity) * deltaTime;
        wheelVelocity *= Mathf.Exp(-wheelSmoothness * deltaTime);

        if (Mathf.Abs(wheelVelocity) < 0.5f)
        {
            wheelVelocity = 0f;
        }

        pauseTimer -= deltaTime;

        if (pauseTimer <= 0f)
        {
            delta += autoSpeed * deltaTime;
        }

        float newY = Mathf.Clamp(currentY + delta, 0f, endY);

        if (newY <= 0f || newY >= endY)
        {
            wheelVelocity = 0f;
        }

        content.anchoredPosition = new Vector2(content.anchoredPosition.x, newY);
        lastY = newY;

        if (!reportedEnd && newY >= endY - 0.5f)
        {
            reportedEnd = true;
            onReachedEnd.Invoke();
        }
    }

    private float ReadWheel()
    {
        Mouse mouse = Mouse.current;

        if (mouse == null)
        {
            return 0f;
        }

        float raw = mouse.scroll.ReadValue().y;

        if (Mathf.Abs(raw) > 10f)
        {
            raw /= 120f;
        }

        return raw;
    }

    private float ReadManualInput()
    {
        float value = 0f;
        Gamepad pad = Gamepad.current;

        if (pad != null)
        {
            float left = pad.leftStick.ReadValue().y;
            float right = pad.rightStick.ReadValue().y;
            value -= Mathf.Abs(right) > Mathf.Abs(left) ? right : left;
            value -= pad.dpad.ReadValue().y;
        }

        Keyboard keyboard = Keyboard.current;

        if (keyboard != null)
        {
            if (keyboard.downArrowKey.isPressed || keyboard.sKey.isPressed)
            {
                value += 1f;
            }

            if (keyboard.upArrowKey.isPressed || keyboard.wKey.isPressed)
            {
                value -= 1f;
            }
        }

        return Mathf.Clamp(value, -1f, 1f);
    }
}