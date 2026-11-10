using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class ScanTransition : MonoBehaviour
{
    [SerializeField] private Image overlay;
    [SerializeField] private Volume pulseVolume;
    [SerializeField] private float coverDuration = 0.6f;
    [SerializeField] private float revealDuration = 0.6f;
    [SerializeField] private float holdDuration = 0.15f;
    [SerializeField] private AnimationCurve curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private const float Min = -0.2f;
    private const float Max = 1.2f;
    private static readonly int StartId = Shader.PropertyToID("_Start");
    private static readonly int EndId = Shader.PropertyToID("_End");

    private Material material;
    private bool running;

    private void Awake()
    {
        material = Instantiate(overlay.material);
        overlay.material = material;
        SetState(Min, Min);
        SetPulse(0f);
        overlay.gameObject.SetActive(false);
    }

    public void Run(Action onCovered)
    {
        RunRoutine(() => InvokeOnce(onCovered));
    }

    public void RunRoutine(Func<IEnumerator> whileCovered)
    {
        if (running)
        {
            return;
        }

        StartCoroutine(Sequence(whileCovered));
    }

    public void Scrub(float value)
    {
        if (running)
        {
            return;
        }

        overlay.gameObject.SetActive(true);
        float cover = Mathf.Clamp01(value);
        float reveal = Mathf.Clamp01(value - 1f);
        SetState(Mathf.Lerp(Min, Max, reveal), Mathf.Lerp(Min, Max, cover));
        SetPulse(value <= 1f ? cover : 1f - reveal);
    }

    public void Hide()
    {
        if (running)
        {
            return;
        }

        SetState(Min, Min);
        SetPulse(0f);
        overlay.gameObject.SetActive(false);
    }

    [ContextMenu("Play Test")]
    private void PlayTest()
    {
        Run(null);
    }

    private static IEnumerator InvokeOnce(Action action)
    {
        action?.Invoke();
        yield break;
    }

    private IEnumerator Sequence(Func<IEnumerator> whileCovered)
    {
        running = true;
        overlay.gameObject.SetActive(true);

        yield return Animate(coverDuration, t =>
        {
            SetState(Min, Mathf.Lerp(Min, Max, t));
            SetPulse(t);
        });

        if (whileCovered != null)
        {
            yield return StartCoroutine(whileCovered());
        }

        yield return new WaitForSecondsRealtime(holdDuration);

        yield return Animate(revealDuration, t =>
        {
            SetState(Mathf.Lerp(Min, Max, t), Max);
            SetPulse(1f - t);
        });

        SetState(Min, Min);
        SetPulse(0f);
        overlay.gameObject.SetActive(false);
        running = false;
    }

    private IEnumerator Animate(float duration, Action<float> apply)
    {
        float time = 0f;

        while (time < duration)
        {
            time += Time.unscaledDeltaTime;
            apply(curve.Evaluate(Mathf.Clamp01(time / duration)));
            yield return null;
        }

        apply(1f);
    }

    private void SetState(float start, float end)
    {
        material.SetFloat(StartId, start);
        material.SetFloat(EndId, end);
    }

    private void SetPulse(float weight)
    {
        if (pulseVolume != null)
        {
            pulseVolume.weight = weight;
        }
    }
}